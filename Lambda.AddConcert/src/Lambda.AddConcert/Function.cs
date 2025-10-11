using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime.Internal;
using Amazon.SQS;
using Amazon.SQS.Model;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Events;
using LPCalendar.DataStructure.Responses;
using ErrorResponse = LPCalendar.DataStructure.Responses.ErrorResponse;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.AddConcert;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDbClient = new AmazonDynamoDBClient();
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private readonly IAmazonSQS _sqsClient = new AmazonSQSClient();

    public Function()
    {
        _dynamoDbContext = new DynamoDBContext(_dynamoDbClient);
        _dynamoDbContext.RegisterCustomConverters();
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
            oldValue = await _dynamoDbContext.LoadAsync<Concert>(concert.Id, _dbConfigProvider.GetLoadConfigFor(DynamoDbConfigProvider.Table.Concerts));
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
                Body = JsonSerializer.Serialize(errors),
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET, POST" }
                }
            };
        }
        
        context.Logger.LogInformation("Start writing to DB...");
        await SaveConcert(concert);
        context.Logger.LogInformation("Concert written to DB");
        
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
        bool valid = true;
        var tmpResponse = new InvalidFieldsErrorResponse
        {
            Message = "Some fields are not valid"
        };

        if (concert.LpuEarlyEntryTime.HasValue && !concert.LpuEarlyEntryConfirmed)
        {
            valid = false;
            tmpResponse.AddInvalidField(nameof(concert.LpuEarlyEntryTime), $"LPU Early Entry time can only be set, if {concert.LpuEarlyEntryConfirmed} is set to true");
        }

        errors = valid ? null : tmpResponse;
        return valid;
    }


    private Concert MakeConcertFromJsonBody(string json)
    {
        string guid = Guid.NewGuid().ToString();
        Concert concert = JsonSerializer.Deserialize<Concert>(json) ?? throw new InvalidDataContractException("JSON could not be parsed to Concert!");
        concert.Id = Guid.TryParse(concert.Id, out _) ? concert.Id : guid;
        return concert;
    }


    private async Task FixNonOverridableFields(Concert concert)
    {
        var existing = await _dynamoDbContext.LoadAsync<Concert>(concert.Id, _dbConfigProvider.GetLoadConfigFor(DynamoDbConfigProvider.Table.Concerts));
        if (existing != null)
        {
            concert.ScheduleImageFile = existing.ScheduleImageFile;
        }
    }


    private async Task SaveConcert(Concert concert)
    {
        await FixNonOverridableFields(concert);
        await _dynamoDbContext.SaveAsync(concert, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.Concerts));
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
            auditLogEvent.OldValue = JsonSerializer.Serialize(oldValue);
        }

        if (newValue != null)
        {
            auditLogEvent.NewValue = JsonSerializer.Serialize(newValue);
        }
        
        var auditMessage = new SendMessageRequest
        {
            MessageGroupId = "default",
            QueueUrl = Environment.GetEnvironmentVariable("AUDIT_LOG_QUEUE_URL"),
            MessageBody = JsonSerializer.Serialize(auditLogEvent)
        };
        
        logger.LogDebug($"Sending SQS message: {JsonSerializer.Serialize(auditMessage)}");

        await _sqsClient.SendMessageAsync(auditMessage);
        
        logger.LogDebug($"Successfully sent message to URL: {auditMessage.QueueUrl}");
    }
}