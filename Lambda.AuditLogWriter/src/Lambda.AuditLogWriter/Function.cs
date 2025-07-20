using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using LPCalendar.DataStructure.Events;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.AuditLogWriter;

public class Function
{
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


    private void ProcessAuditLogEvent(AuditLogEvent auditEvent)
    {
        
    }
}