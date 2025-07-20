using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.SQSEvents;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.Entities;
using LPCalendar.DataStructure.Events;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.AuditLogWriter;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DBOperationConfigProvider _dbOperationConfigProvider = new();
    
    
    public Function()
    {
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        _dynamoDbContext.RegisterCustomConverters();
    }
    
    
    public void HandleSQSMessages(List<SQSEvent.SQSMessage> messages, ILambdaContext context)
    {
        foreach (var auditLogEvent in messages
                     .Select(msg => JsonSerializer.Deserialize<AuditLogEvent>(msg.Body))
                     .Where(auditLogEvent => auditLogEvent != null)
                     .Cast<AuditLogEvent>())
        {
            context.Logger.LogInformation("Received event: " + JsonSerializer.Serialize(auditLogEvent));
            ProcessAuditLogEvent(auditLogEvent);
        }
    }
    
    
    public async Task HandleConcertsDynamoDBEvent(DynamoDBEvent dynamoEvent, ILambdaContext context)
    {
        var saveAuditLogTasks = new List<Task>();
        foreach (var record in dynamoEvent.Records)
        {
            context.Logger.LogInformation("Record: " + JsonSerializer.Serialize(record));
            
            if (record.EventName == OperationType.MODIFY)
            {
                var oldImage = record.Dynamodb.OldImage;
                var newImage = record.Dynamodb.NewImage;

                var auditLogEntry = new AuditLogEntry
                {
                    UserId = "TODO",
                    Action = "Concerts_Modify",
                    Timestamp = record.Dynamodb.ApproximateCreationDateTime,
                    AffectedEntity = string.Join(",", record.Dynamodb.Keys.Values.Select(v => v.S)),
                    OldValue = JsonSerializer.Serialize(oldImage),
                    NewValue = JsonSerializer.Serialize(newImage)
                };
                
                var task = SaveAuditLog(auditLogEntry);
                saveAuditLogTasks.Add(task);
            } else if (record.EventName == OperationType.INSERT)
            {
                var newImage = record.Dynamodb.NewImage;

                var auditLogEntry = new AuditLogEntry
                {
                    UserId = "TODO",
                    Action = "Concerts_Add",
                    Timestamp = record.Dynamodb.ApproximateCreationDateTime,
                    AffectedEntity = string.Join(",", record.Dynamodb.Keys.Values.Select(v => v.S)),
                    NewValue = JsonSerializer.Serialize(newImage)
                };
                
                var task = SaveAuditLog(auditLogEntry);
                saveAuditLogTasks.Add(task);
            } else if (record.EventName == OperationType.REMOVE)
            {
                var oldImage = record.Dynamodb.OldImage;

                var auditLogEntry = new AuditLogEntry
                {
                    UserId = "TODO",
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


    private void ProcessAuditLogEvent(AuditLogEvent auditEvent)
    {
        
    }


    private async Task SaveAuditLog(AuditLogEntry entry)
    {
        await _dynamoDbContext.SaveAsync(entry, _dbOperationConfigProvider.GetAuditLogEntryConfigWithEnvTableName());
    }
}