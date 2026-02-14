using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Database.Concerts;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Events;
using LPCalendar.DataStructure.Events.PushNotifications;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.AddConcert;

public class Function
{
    private readonly IConcertRepository _concertRepository;
    
    private readonly IAmazonSQS _sqsClient = new AmazonSQSClient();

    public Function(ILambdaContext context)
    {
        _concertRepository = DynamoDbConcertRepository.CreateDefault(context.Logger);
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var hasAddPermission = request.CanAddConcerts();
        var hasUpdatePermission = request.CanUpdateConcerts();
        if (!hasAddPermission && !hasUpdatePermission)
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, POST");
        }
        
        if (request.Body == null)
        {
            return new APIGatewayProxyResponse()
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = "{\"message\": \"Request body not found\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
                }
            };
        }
        
        // Parse JSON
        context.Logger.LogInformation("Start parsing JSON...");
        var concert = MakeConcertFromJsonBody(request.Body);
        
        // check if concerts exists to get old value
        Concert? oldValue = null;
        if (!string.IsNullOrEmpty(concert.Id))
        {
            oldValue = await _concertRepository.GetByIdAsync(concert.Id);
        }
        
        var action = oldValue == null ? "Add" : "Update";
        
        // Check if it's update and user has permission
        if (oldValue != null && !hasUpdatePermission)
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, POST");
        }

        // Check if it's add and user has permission
        if (oldValue == null && !hasAddPermission)
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, POST");
        }

        context.Logger.LogInformation("Validate request");
        var isValid = RequestIsValid(concert, out var errors);
        if (!isValid)
        {
            context.Logger.LogWarning("Request was not valid. Will return 400");
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = JsonSerializer.Serialize(errors!, DataStructureJsonContext.Default.InvalidFieldsErrorResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
                }
            };
        }
        
        context.Logger.LogInformation("Start writing to DB...");
        await _concertRepository.SaveAsync(concert);
        context.Logger.LogInformation("Concert written to DB");
        
        await SendNotificationsIfNeeded(concert, oldValue, context.Logger);
        await LogChanges(oldValue, concert, request.GetUserId(), action, context.Logger);
        
        var response = new APIGatewayProxyResponse()
        {
            StatusCode = (int)HttpStatusCode.Created,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
            }
        };

        return response;
    }


    private bool RequestIsValid(Concert concert, [NotNullWhen(true)] out InvalidFieldsErrorResponse? errors)
    {
        var valid = true;
        var tmpResponse = new InvalidFieldsErrorResponse
        {
            Message = "Some fields are not valid"
        };

        if (concert is { LpuEarlyEntryTime: not null, LpuEarlyEntryConfirmed: false })
        {
            valid = false;
            tmpResponse.AddInvalidField(nameof(concert.LpuEarlyEntryTime), $"LPU Early Entry time can only be set, if {concert.LpuEarlyEntryConfirmed} is set to true");
        }

        errors = valid ? null : tmpResponse;
        return valid;
    }


    private static Concert MakeConcertFromJsonBody(string json)
    {
        var guid = Guid.NewGuid().ToString();
        var concert = JsonSerializer.Deserialize(json, DataStructureJsonContext.Default.Concert) ?? throw new InvalidDataContractException("JSON could not be parsed to Concert!");
        concert.Id = Guid.TryParse(concert.Id, out _) ? concert.Id : guid;
        return concert;
    }


    private async Task LogChanges(Concert? oldValue, Concert? newValue, string? userId, string action, ILambdaLogger logger)
    {
        logger.LogDebug($"Log action '{action}' made by {userId}...");
        var auditLogEvent = new AuditLogEvent
        {
            UserId = userId ?? "unknown",
            Action = $"Concert_{action}",
            AffectedEntity = $"{Concert.ConcertTableName}#{newValue?.Id ?? oldValue?.Id}",
            Timestamp = DateTime.UtcNow
        };

        if (oldValue != null)
        {
            auditLogEvent.OldValue = JsonSerializer.Serialize(oldValue, DataStructureJsonContext.Default.Concert);
        }

        if (newValue != null)
        {
            auditLogEvent.NewValue = JsonSerializer.Serialize(newValue, DataStructureJsonContext.Default.Concert);
        }
        
        var auditMessage = new SendMessageRequest
        {
            MessageGroupId = "default",
            QueueUrl = Environment.GetEnvironmentVariable("AUDIT_LOG_QUEUE_URL"),
            MessageBody = JsonSerializer.Serialize(auditLogEvent, DataStructureJsonContext.Default.AuditLogEvent)
        };
        
        logger.LogDebug("Sending SQS message: {json}", JsonSerializer.Serialize(auditMessage, LocalJsonContext.Default.SendMessageRequest));

        await _sqsClient.SendMessageAsync(auditMessage);
        
        logger.LogDebug($"Successfully sent message to URL: {auditMessage.QueueUrl}");
    }


    private async Task SendNotificationsIfNeeded(Concert newValue, Concert? oldValue, ILambdaLogger logger)
    {
        logger.LogDebug("Check if notifications need to be sent");
        
        var mainStageTimeConfirmed = false;
        if (oldValue != null)
        {
            mainStageTimeConfirmed = newValue.MainStageTime != null && newValue.MainStageTime != oldValue.MainStageTime;
        }

        if (mainStageTimeConfirmed)
        {
            logger.LogDebug($"Send notification for stage time confirmed: {newValue.MainStageTime}");
            var notification = new ConcertRelatedPushNotificationEvent
            {
                Concert = newValue
            };
            
            await SendPushNotification(notification, PushNotificationType.MainStageTimeConfirmed, logger);
        }
    }


    /// <summary>
    /// Send the notification to the SQS queue
    /// </summary>
    private async Task SendPushNotification(ConcertRelatedPushNotificationEvent pushNotificationEvent, PushNotificationType notificationType, ILambdaLogger logger)
    {
        var sqsMessage = new SendMessageRequest
        {
            MessageGroupId = "default",
            QueueUrl = Environment.GetEnvironmentVariable("PUSH_QUEUE_URL"),
            MessageBody = JsonSerializer.Serialize(pushNotificationEvent, DataStructureJsonContext.Default.ConcertRelatedPushNotificationEvent),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                {
                    "notificationType", new MessageAttributeValue
                    {
                        StringValue = notificationType.ToString(),
                        DataType = "String"
                    }
                }
            }
        };
        
        await _sqsClient.SendMessageAsync(sqsMessage);
    }
}