using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
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
        foreach (var notificationEvent in sqsEvent.Records
                     .Select(msg =>
                     {
                         if (msg.Body == null)
                         {
                             context.Logger.LogWarning("Message body is NULL!");
                         }
                         context.Logger.LogDebug($"Handle message with body: {msg.Body}");
                         return JsonSerializer.Deserialize(msg.Body,
                             DataStructureJsonContext.Default.PushNotificationEvent);
                     })
                     .Where(pn => pn != null)
                     .Cast<PushNotificationEvent>())
        {
            context.Logger.LogDebug("Try to send push notification: {0}", JsonSerializer.Serialize(notificationEvent, DataStructureJsonContext.Default.PushNotificationEvent));
            var task = SendNotification(notificationEvent, context.Logger);
            sendNotificationsTasks.Add(task);
        }
        
        await Task.WhenAll(sendNotificationsTasks);
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
                        }
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
                    Message = snsMessageJson
                };
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
}