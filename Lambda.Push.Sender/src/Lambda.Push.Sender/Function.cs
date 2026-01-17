using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Entities;
using LPCalendar.DataStructure.Events;
using LPCalendar.DataStructure.Events.PushNotifications;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.Push.Sender;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly string _platformApplicationArn;
    private readonly Dictionary<string, string> _endpointToUserIdCache = new();
    private readonly Dictionary<string, bool> _endpointStatusCache = new();
    
    public Function()
    {
        _platformApplicationArn = Environment.GetEnvironmentVariable("PLATFORM_APPLICATION_ARN")
                                  ?? throw new Exception("Missing environment variable PLATFORM_APPLICATION_ARN");
        
        _sns = new AmazonSimpleNotificationServiceClient();
        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    
    /// <summary>
    /// Set bookmark status for a user on a concert
    /// </summary>
    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        var sqsJson = JsonSerializer.Serialize(sqsEvent, global::Common.SQS.JsonContext.SqsEventJsonSerializer.Default.SQSEvent);
        context.Logger.LogInformation($"SQS event: {sqsJson}");

        var sendNotificationsTasks = await sqsEvent.Records
            .ToAsyncEnumerable()
            .SelectMany<SQSEvent.SQSMessage, PushNotificationEvent>(msg =>
            {
                if (msg.Body == null)
                {
                    context.Logger.LogWarning("Message body is NULL!");
                    return AsyncEnumerable.Empty<PushNotificationEvent>();
                }

                context.Logger.LogDebug($"Handle message with body: {msg.Body}");

                var notificationType = PushNotificationType.Custom;
                if (msg.MessageAttributes.TryGetValue("notificationType", out var notificationTypeAttr))
                {
                    context.Logger.LogDebug($"Parsing notification type: {notificationTypeAttr.StringValue}");
                    notificationType = Enum.Parse<PushNotificationType>(notificationTypeAttr.StringValue);
                }

                context.Logger.LogInformation($"Notification type: {notificationType}");

                return GetPushNotificationsForType(notificationType, msg.Body, context.Logger);
            })
            .Select(pushNotificationEvent =>
            {
                context.Logger.LogDebug("Try to send push notification: {0}", JsonSerializer.Serialize(pushNotificationEvent, DataStructureJsonContext.Default.PushNotificationEvent));
                return SendNotification(pushNotificationEvent, context.Logger);
            })
            .ToListAsync();

        await Task.WhenAll(sendNotificationsTasks);
        context.Logger.LogInformation("All {count} notifications sent.", sendNotificationsTasks.Count);
    }
    
    
    private IAsyncEnumerable<PushNotificationEvent> GetPushNotificationsForType(PushNotificationType notificationType, string msgBody, ILambdaLogger logger)
    {
        logger.LogDebug("Getting push notifications for {notificationType}. Message body: {messageBody}", notificationType, msgBody);
        var pushEvent = JsonSerializer.Deserialize(msgBody,
            DataStructureJsonContext.Default.ConcertRelatedPushNotificationEvent);
        if (pushEvent == null)
        {
            logger.LogWarning("No ConcertRelatedPushNotificationEvent found.");
            return AsyncEnumerable.Empty<PushNotificationEvent>();
        }

        // find users to send the message to
        logger.LogDebug("Get list of recipients...");
        var recipients = GetAllUsersRegisteredForNotifications(logger);
        return recipients
            .Distinct()
            .WhereAwait(async userId => await UserCanReceiveNotificationFor(pushEvent.Concert,
                PushNotificationType.MainStageTimeConfirmed, userId, logger))
            .Select(recipient => new PushNotificationEvent
            {
                UserId = recipient,
                Title = $"Linkin Park in {pushEvent.Concert.City}",
                Body =
                    $"Stage time for Linkin Park confirmed: {pushEvent.Concert.MainStageTime:HH:mm} ({pushEvent.Concert.TimeZoneId})",
                CollapseId = $"{pushEvent.Concert.Id}#{nameof(PushNotificationType.MainStageTimeConfirmed)}",
                Thread = pushEvent.Concert.Id
            });
    }
    

    private async Task SendNotification(PushNotificationEvent pushNotificationEvent, ILambdaLogger logger)
    {
        logger.LogDebug("Message recipient: {userId}", pushNotificationEvent.UserId);
        var queryConfig = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.NotificationRegistrations);
        var query = _dynamoDbContext.QueryAsync<NotificationUserEndpoint>(pushNotificationEvent.UserId, queryConfig);
        var endpoints = await query.GetRemainingAsync() ?? [];
        
        var sendTasks = await endpoints
            .ToAsyncEnumerable()
            .WhereAwait(async endpoint => await IsEndpointEnabled(endpoint.EndpointArn, logger))
            .Select(async endpoint =>
        {
            logger.LogDebug("Publishing to endpoint: {endpoint}", endpoint.EndpointArn);
            try
            {
                var pushMessagePayload = new NotificationWrapper
                {
                    Apple = new AppleNotificationAlert
                    {
                        Alert = new AppleNotificationPayload
                        {
                            Title =  pushNotificationEvent.Title,
                            Body = pushNotificationEvent.Body
                        },
                        ThreadId = pushNotificationEvent.Thread
                    }
                };

                var snsMessage = new SnsMessage
                {
                    Default = pushNotificationEvent.Body,
                    AppleNotificationService = JsonSerializer.Serialize(pushMessagePayload, NotificationJsonSerializer.Default.NotificationWrapper)
                };

                var snsMessageJson = JsonSerializer.Serialize(snsMessage, NotificationJsonSerializer.Default.SnsMessage);
                logger.LogDebug("SNS Message: {messageJson}", snsMessageJson);
                
                var publishRequest = new PublishRequest
                {
                    TargetArn = endpoint.EndpointArn,
                    MessageStructure = "json",
                    Message = snsMessageJson,
                    MessageAttributes = new Dictionary<string, MessageAttributeValue>()
                };

                if (!string.IsNullOrEmpty(pushNotificationEvent.CollapseId))
                {
                    logger.LogDebug("Add CollapseId: {collapseId}", pushNotificationEvent.CollapseId);
                    publishRequest.MessageAttributes["AWS.SNS.MOBILE.APNS.COLLAPSE_ID"] = new MessageAttributeValue
                    {
                        StringValue = pushNotificationEvent.CollapseId,
                        DataType = "String"
                    };
                }
                
                var publishResponse = await _sns.PublishAsync(publishRequest);
                logger.LogDebug("Published message: {id} (Sequence: {seq})", publishResponse.MessageId, publishResponse.SequenceNumber);
            } catch (Exception e)
            {
                logger.LogError(e, "Failed to publish message!");
            }
        })
            .ToListAsync();
        await Task.WhenAll(sendTasks);
        logger.LogDebug("Sent notifications to: {userId}", pushNotificationEvent.UserId);
    }


    private async IAsyncEnumerable<string> GetAllUsersRegisteredForNotifications(ILambdaLogger logger)
    {
        var request = new ListEndpointsByPlatformApplicationRequest
        {
            PlatformApplicationArn = _platformApplicationArn
        };
        
        ListEndpointsByPlatformApplicationResponse response;
        do
        {
            response = await _sns.ListEndpointsByPlatformApplicationAsync(request);
            var userIdFetchingEnumerable = response.Endpoints
                .Where(ep =>
                {
                    var enabled = ep.IsEnabled();
                    _endpointStatusCache.Add(ep.EndpointArn, enabled);
                    return enabled;
                })
                .Select(ep => ep.EndpointArn)
                .Select(arn => GetUserIdForEndpointArn(arn, logger));
            foreach (var userIdTask in userIdFetchingEnumerable)
            {
                var foundUserId = await userIdTask;
                if (foundUserId != null)
                {
                    yield return foundUserId!;
                }
            }
            
            request.NextToken = response.NextToken;
        } while (response.Endpoints.Count > 0 && !string.IsNullOrEmpty(response.NextToken));
    }


    /// <summary>
    /// Search in DB which user has registered the given endpoint ARN
    /// </summary>
    /// <param name="endpointArn"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private async Task<string?> GetUserIdForEndpointArn(string endpointArn, ILambdaLogger logger)
    {
        logger.LogDebug("GetUserIdForEndpointArn: {endpointArn}", endpointArn);
        if (_endpointToUserIdCache.TryGetValue(endpointArn, out var userId))
        {
            logger.LogDebug("UserId found in cache: {userId}", userId);
            return userId;
        }
        
        // fetch UserId from Database
        var getEndpointQueryConfig =
            _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.NotificationRegistrations);
        getEndpointQueryConfig.IndexName = NotificationUserEndpoint.EndpointArnIndex;
        var dbResponse = _dynamoDbContext.QueryAsync<NotificationUserEndpoint>(endpointArn, getEndpointQueryConfig);
        var notificationUserEndpoints = await dbResponse.GetRemainingAsync();
        var foundUserId = notificationUserEndpoints.FirstOrDefault()?.UserId;
        logger.LogDebug("Found userId: {foundUserId}", foundUserId);
        if (foundUserId != null)
        {
            _endpointToUserIdCache.Add(endpointArn, foundUserId);
        }
        
        return foundUserId;
    }


    private async Task<bool> UserCanReceiveNotificationFor(Concert concert, PushNotificationType pushNotificationType, string userId, ILambdaLogger logger)
    {
        logger.LogDebug("Checking if user '{{userId}}' can receive notification '{{pushNotificationType}}' for concert '{concertId}'...", userId, pushNotificationType, concert.Id);
        var queryNotificationSettingsConfig =
            _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.UserNotificationSettings);
        var queryNotificationSettings =
            _dynamoDbContext.QueryAsync<UserNotificationSettings>(userId, queryNotificationSettingsConfig);
        var notificationSettings = (await queryNotificationSettings.GetRemainingAsync()).FirstOrDefault() ?? new UserNotificationSettings
        {
            UserId = userId
        };
        
        var userBookmark = await GetBookmarkStatusForUserAtConcert(concert.Id, notificationSettings.UserId, logger);

        var canReceive = pushNotificationType switch
        {
            PushNotificationType.ConcertReminder => notificationSettings.ReceiveConcertReminders &&
                                                    notificationSettings.ConcertRemindersStatus.Any(r =>
                                                        r == userBookmark),
            PushNotificationType.MainStageTimeConfirmed => notificationSettings.ReceiveMainStageTimeUpdates &&
                                                           notificationSettings.MainStageTimeUpdatesStatus.Any(r =>
                                                               r == userBookmark),
            _ => false
        };
        
        logger.LogDebug("Can user receive notification? {canReceive}", canReceive);
        
        return canReceive;
    }
    
    
    private async Task<ConcertBookmark.BookmarkStatus> GetBookmarkStatusForUserAtConcert(string concertId,
        string? userId, ILambdaLogger logger)
    {
        if (userId == null)
        {
            return ConcertBookmark.BookmarkStatus.None;
        }
        
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.ConcertBookmarks);
        config.IndexName = ConcertBookmark.UserBookmarksIndex;
        
        logger.LogDebug($"Query index: {config.IndexName}");
        
        var query = _dynamoDbContext.QueryAsync<ConcertBookmark>(
            userId, // PartitionKey value
            QueryOperator.Equal,
            [ concertId ],
            config);
        var bookmarkList = await query.GetRemainingAsync();
        return bookmarkList.FirstOrDefault()?.Status ?? ConcertBookmark.BookmarkStatus.None;
    }


    /// <summary>
    /// Checks if the endpoint is enabled. Uses a cache to avoid rate limits.
    /// </summary>
    /// <param name="endpointArn"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    private async Task<bool> IsEndpointEnabled(string endpointArn, ILambdaLogger logger)
    {
        if (_endpointStatusCache.TryGetValue(endpointArn, out var cachedEnabledStatus))
        {
            return cachedEnabledStatus;
        }

        var request = new GetEndpointAttributesRequest
        {
            EndpointArn = endpointArn
        };
        var response = await _sns.GetEndpointAttributesAsync(request);
        var enabled = response.IsEnabled();
        _endpointStatusCache.Add(endpointArn, enabled);
        return enabled;
    }
}