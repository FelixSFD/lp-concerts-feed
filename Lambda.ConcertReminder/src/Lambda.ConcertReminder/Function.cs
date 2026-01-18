using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Entities;
using LPCalendar.DataStructure.Events;
using LPCalendar.DataStructure.Events.PushNotifications;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.ConcertReminder;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private readonly IAmazonSQS _sqsClient = new AmazonSQSClient();
    
    public Function()
    {
        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    
    /// <summary>
    /// Set bookmark status for a user on a concert
    /// </summary>
    public async Task FunctionHandler(ScheduledEvent evt, ILambdaContext context)
    {
        // get next concert
        var concert = await ReturnNextConcert(context);
        
        var eventTime = evt.Time.ToUniversalTime();
        context.Logger.LogInformation("Event time: {time}", eventTime);

        if (concert != null)
        {
            var startTime = concert.MainStageTime ?? concert.PostedStartTime!.Value;
            var utcStartTime = startTime.UtcDateTime;
            var remindAfter = utcStartTime.AddHours(-1);
            
            context.Logger.LogDebug("Concert starts at: {time}; Will remind after: {reminder}", utcStartTime, remindAfter);

            if (eventTime > remindAfter)
            {
                context.Logger.LogDebug("Concert is in range for reminder.");
                var notificationLastSentAt = await GetLastNotificationSent(concert.Id, context);
                if (notificationLastSentAt != null && notificationLastSentAt != DateTime.MinValue)
                {
                    context.Logger.LogDebug("Reminder was already sent at '{date}'. Will not send again.", notificationLastSentAt);
                    return;
                }
                
                await SendReminder(concert, context);
            }
            else
            {
                context.Logger.LogDebug("Concert is too far away to send a reminder");
            }
        }
        else
        {
            context.Logger.LogInformation("No upcoming concert found");
        }
    }
    
    
    private async Task<DateTimeOffset?> GetLastNotificationSent(string concertId, ILambdaContext context)
    {
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.ConcertNotificationHistory);
        var query = _dynamoDbContext.QueryAsync<ConcertNotificationHistory>(
            concertId, // PartitionKey value
            config);

        var notifications = await query.GetRemainingAsync();
        return notifications
            .Where(nh => nh.EventType == ConcertEventType.Reminder)
            .Select(nh => nh.SentAt)
            .Order()
            .FirstOrDefault();
    }
    
    
    private async Task<Concert?> ReturnNextConcert(ILambdaContext context)
    {
        var now = DateTimeOffset.UtcNow.AddHours(-4);
        var dateNowStr = now.ToString("O");
        
        context.Logger.LogInformation("Query concerts after: {time}", dateNowStr);

        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.Concerts);
        config.IndexName = "PostedStartTimeGlobalIndex";
        
        var query = _dynamoDbContext.QueryAsync<Concert>(
            "PUBLISHED", // PartitionKey value
            QueryOperator.GreaterThanOrEqual,
            [new AttributeValue { S = dateNowStr }],
            config);

        var concerts = await query.GetRemainingAsync();
        return concerts.FirstOrDefault();
    }
    
    
    private async Task SendReminder(Concert concert, ILambdaContext context)
    {
        var pushEvent = new ConcertRelatedPushNotificationEvent
        {
            Concert = concert
        };
        
        context.Logger.LogDebug("Will send reminder of type '{type}' for concert with id '{id}'", PushNotificationType.ConcertReminder, pushEvent.Concert.Id);
        
        var sqsMessage = new SendMessageRequest
        {
            MessageGroupId = "default",
            QueueUrl = Environment.GetEnvironmentVariable("PUSH_QUEUE_URL"),
            MessageBody = JsonSerializer.Serialize(pushEvent, DataStructureJsonContext.Default.ConcertRelatedPushNotificationEvent),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "notificationType", new MessageAttributeValue
                    {
                        StringValue = nameof(PushNotificationType.ConcertReminder),
                        DataType = "String"
                    }
                }
            }
        };
        
        context.Logger.LogDebug("Prepared SQS Message");
        context.Logger.LogDebug("Sending SQS message: {json}", JsonSerializer.Serialize(sqsMessage, LocalJsonSerializer.Default.SendMessageRequest));

        await _sqsClient.SendMessageAsync(sqsMessage);
        await StoreConcertNotificationHistory(concert, context);
    }


    /// <summary>
    /// Save that a reminder was sent
    /// </summary>
    private async Task StoreConcertNotificationHistory(Concert concert, ILambdaContext context)
    {
        context.Logger.LogDebug("Store concert notification history");

        var historyEntry = new ConcertNotificationHistory
        {
            ConcertId = concert.Id,
            SentAt = DateTimeOffset.UtcNow,
            EventType = ConcertEventType.Reminder
        };
        
        await _dynamoDbContext.SaveAsync(historyEntry, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.ConcertNotificationHistory));
        context.Logger.LogDebug("Saved concert notification history entry for concert with id '{id}'", concert.Id);
    }
}