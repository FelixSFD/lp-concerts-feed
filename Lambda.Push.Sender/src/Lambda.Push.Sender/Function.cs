using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
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
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.Push.Sender;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private readonly IAmazonSimpleNotificationService _sns;
    
    public Function()
    {
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
        var sqsJson = JsonSerializer.Serialize(sqsEvent, SqsEventJsonSerializer.Default.SQSEvent);
        context.Logger.LogInformation($"SQS event: {sqsJson}");
        
        var sendNotificationsTasks = new List<Task>();
        foreach (var notificationEventsTask in sqsEvent.Records
                     .Select<SQSEvent.SQSMessage, Task<IEnumerable<PushNotificationEvent>>>(msg =>
                     {
                         if (msg.Body == null)
                         {
                             context.Logger.LogWarning("Message body is NULL!");
                             return Task.FromResult(Enumerable.Empty<PushNotificationEvent>());
                         }
                         context.Logger.LogDebug($"Handle message with body: {msg.Body}");

                         var notificationType = PushNotificationType.Custom;
                         if (msg.MessageAttributes.TryGetValue("notificationType", out var notificationTypeAttr))
                         {
                             context.Logger.LogDebug($"Parsing notification type: {notificationTypeAttr.StringValue}");
                             notificationType = Enum.Parse<PushNotificationType>(notificationTypeAttr.StringValue);
                         }
                         
                         context.Logger.LogInformation($"Notification type: {notificationType}");
                         
                         return GetPushNotificationFor(notificationType, msg.Body, context.Logger);
                     }))
        {
            var notificationEvents = (await notificationEventsTask).ToArray();
            context.Logger.LogInformation("Sending {number} notifications...", notificationEvents.Length);
            foreach (var notificationEvent in notificationEvents)
            {
                context.Logger.LogDebug("Try to send push notification: {0}", JsonSerializer.Serialize(notificationEvent, DataStructureJsonContext.Default.PushNotificationEvent));
                var task = SendNotification(notificationEvent, context.Logger);
                sendNotificationsTasks.Add(task);
            }
        }
        
        await Task.WhenAll(sendNotificationsTasks);
        context.Logger.LogInformation("All notifications sent.");
    }

    private async Task<IEnumerable<PushNotificationEvent>> GetPushNotificationFor(PushNotificationType pushNotificationType, string msgBody, ILambdaLogger logger)
    {
        return pushNotificationType switch
        {
            PushNotificationType.Custom => GetPushNotificationsForCustom(msgBody),
            PushNotificationType.ConcertReminder => await GetPushNotificationsForConcertReminder(msgBody, logger),
            PushNotificationType.MainStageTimeConfirmed => await GetPushNotificationsForMainStageTimeConfirmed(msgBody, logger),
            _ => throw new ArgumentOutOfRangeException(nameof(pushNotificationType), pushNotificationType, null)
        };
    }

    private static IEnumerable<PushNotificationEvent> GetPushNotificationsForCustom(string msgBody)
    {
        var msg = JsonSerializer.Deserialize(msgBody, DataStructureJsonContext.Default.PushNotificationEvent);
        return msg != null ? [msg] : [];
    }
    
    private async Task<IEnumerable<PushNotificationEvent>> GetPushNotificationsForConcertReminder(string msgBody, ILambdaLogger logger)
    {
        logger.LogDebug("Getting push notifications for concert reminder. Message body: {messageBody}", msgBody);
        var pushEvent = JsonSerializer.Deserialize(msgBody,
            DataStructureJsonContext.Default.ConcertRelatedPushNotificationEvent);
        if (pushEvent == null)
        {
            logger.LogWarning("No ConcertRelatedPushNotificationEvent found.");
            return [];
        }

        // find users to send the message to
        logger.LogDebug("Get list of recipients...");
        var recipients = await GetRecipientUserIdsFor(pushEvent.Concert, PushNotificationType.ConcertReminder);
        return recipients.Select(userId => new PushNotificationEvent
        {
            UserId = userId,
            Title = $"Linkin Park in {pushEvent.Concert.City}",
            Body = "The concert is starting soon! 🔥\nOpen the app to see the exact start time in your timezone.",
            Thread = pushEvent.Concert.Id
        });
    }
    
    
    private async Task<IEnumerable<PushNotificationEvent>> GetPushNotificationsForMainStageTimeConfirmed(string msgBody, ILambdaLogger logger)
    {
        logger.LogDebug("Getting push notifications for confirmed stage time. Message body: {messageBody}", msgBody);
        var pushEvent = JsonSerializer.Deserialize(msgBody,
            DataStructureJsonContext.Default.ConcertRelatedPushNotificationEvent);
        if (pushEvent == null)
        {
            logger.LogWarning("No ConcertRelatedPushNotificationEvent found.");
            return [];
        }

        // find users to send the message to
        logger.LogDebug("Get list of recipients...");
        var recipients = await GetRecipientUserIdsFor(pushEvent.Concert, PushNotificationType.MainStageTimeConfirmed);
        return recipients.Select(userId => new PushNotificationEvent
        {
            UserId = userId,
            Title = $"Linkin Park in {pushEvent.Concert.City}",
            Body = $"Stage time for Linkin Park confirmed: {pushEvent.Concert.MainStageTime:HH:mm} ({pushEvent.Concert.TimeZoneId})",
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
        
        var sendTasks = endpoints.Select(async endpoint =>
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
                    AppleNotificationService = JsonSerializer.Serialize(pushMessagePayload, SqsEventJsonSerializer.Default.NotificationWrapper)
                };

                var snsMessageJson = JsonSerializer.Serialize(snsMessage, SqsEventJsonSerializer.Default.SnsMessage);
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
        });
        
        await Task.WhenAll(sendTasks);
        logger.LogDebug("Sent notifications to: {userId}", pushNotificationEvent.UserId);
    }

    private async Task<IEnumerable<string>> GetRecipientUserIdsFor(Concert concert, PushNotificationType pushNotificationType)
    {
        return pushNotificationType switch
        {
            PushNotificationType.ConcertReminder => await GetRecipientUserIdsForConcertReminder(concert),
            PushNotificationType.MainStageTimeConfirmed => await GetRecipientUserIdsForConcertReminder(concert),
            _ => []
        };
    }
    
    
    private async Task<IEnumerable<string>> GetRecipientUserIdsForConcertReminder(Concert concert)
    {
        var bookmarked = await GetUsersWithBookmarkStatusForConcert(concert, ConcertBookmark.BookmarkStatus.Bookmarked);
        var attending = await GetUsersWithBookmarkStatusForConcert(concert, ConcertBookmark.BookmarkStatus.Attending);
        
        return bookmarked.Concat(attending);
    }


    private async Task<IEnumerable<string>> GetUsersWithBookmarkStatusForConcert(Concert concert, ConcertBookmark.BookmarkStatus bookmarkStatus)
    {
        var queryConfig = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.ConcertBookmarks);
        queryConfig.IndexName = ConcertBookmark.ConcertBookmarkStatusIndex;

        var query = _dynamoDbContext.QueryAsync<ConcertBookmark>(concert.Id, QueryOperator.Equal, [bookmarkStatus.ToString()], queryConfig);
        var results = await query.GetRemainingAsync();
        return results.Select(bm => bm.UserId);
    }
}