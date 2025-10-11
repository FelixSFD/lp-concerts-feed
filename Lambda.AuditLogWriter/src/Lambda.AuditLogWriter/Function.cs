using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.SQSEvents;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Entities;
using LPCalendar.DataStructure.Events;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.AuditLogWriter;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    
    
    public Function()
    {
        _dynamoDbContext = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => new AmazonDynamoDBClient())
            .Build();
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    
    public async Task HandleSQSMessages(SQSEvent sqsEvent, ILambdaContext context)
    {
        var saveAuditLogTasks = new List<Task>();
        foreach (var auditLogEvent in sqsEvent.Records
                     .Select(msg => JsonSerializer.Deserialize<AuditLogEvent>(msg.Body))
                     .Where(auditLogEvent => auditLogEvent != null)
                     .Cast<AuditLogEvent>())
        {
            context.Logger.LogInformation("Received event: " + JsonSerializer.Serialize(auditLogEvent));
            var task = ProcessAuditLogEvent(auditLogEvent, context.Logger);
            saveAuditLogTasks.Add(task);
        }
        
        await Task.WhenAll(saveAuditLogTasks);
    }
    
    
    [Obsolete("Doesn't provide identity info")]
    public async Task HandleConcertsDynamoDBEvent(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        var saveAuditLogTasks = new List<Task>();
        foreach (var record in dynamoEvent.Records)
        {
            context.Logger.LogInformation("Record: " + JsonSerializer.Serialize(record));
            
            if (record.EventName == DynamoDBEventNames.Modify)
            {
                var oldImage = record.Dynamodb.OldImage;
                var newImage = record.Dynamodb.NewImage;

                var auditLogEntry = new AuditLogEntry
                {
                    UserId = "unknown",
                    Action = "Concerts_Modify",
                    Timestamp = record.Dynamodb.ApproximateCreationDateTime,
                    AffectedEntity = string.Join(",", record.Dynamodb.Keys.Values.Select(v => v.S)),
                    OldValue = JsonSerializer.Serialize(oldImage),
                    NewValue = JsonSerializer.Serialize(newImage)
                };
                
                var task = SaveAuditLog(auditLogEntry);
                saveAuditLogTasks.Add(task);
            } else if (record.EventName == DynamoDBEventNames.Insert)
            {
                var newImage = record.Dynamodb.NewImage;

                var auditLogEntry = new AuditLogEntry
                {
                    UserId = "unknown",
                    Action = "Concerts_Add",
                    Timestamp = record.Dynamodb.ApproximateCreationDateTime,
                    AffectedEntity = string.Join(",", record.Dynamodb.Keys.Values.Select(v => v.S)),
                    NewValue = JsonSerializer.Serialize(newImage)
                };
                
                var task = SaveAuditLog(auditLogEntry);
                saveAuditLogTasks.Add(task);
            } else if (record.EventName == DynamoDBEventNames.Remove)
            {
                var oldImage = record.Dynamodb.OldImage;

                var auditLogEntry = new AuditLogEntry
                {
                    UserId = "unknown",
                    Action = "Concerts_Delete",
                    Timestamp = record.Dynamodb.ApproximateCreationDateTime,
                    AffectedEntity = string.Join(",", record.Dynamodb.Keys.Values.Select(v => v.S)),
                    OldValue = JsonSerializer.Serialize(oldImage),
                };
                
                var task = SaveAuditLog(auditLogEntry);
                saveAuditLogTasks.Add(task);
            }
        }
        
        // wait for all writes to finish
        await Task.WhenAll(saveAuditLogTasks);
    }


    private async Task ProcessAuditLogEvent(AuditLogEvent auditEvent, ILambdaLogger logger)
    {
        var entry = new AuditLogEntry
        {
            UserId = auditEvent.UserId ?? "unknown",
            Action = auditEvent.Action,
            Timestamp = auditEvent.Timestamp,
            AffectedEntity = auditEvent.AffectedEntity,
            OldValue = auditEvent.OldValue,
            NewValue = auditEvent.NewValue
        };
        
        logger.LogDebug($"AuditLogEntry: {JsonSerializer.Serialize(entry)}");

        await SaveAuditLog(entry);
    }


    private async Task SaveAuditLog(AuditLogEntry entry)
    {
        await _dynamoDbContext.SaveAsync(entry, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.AuditLog));
    }
}