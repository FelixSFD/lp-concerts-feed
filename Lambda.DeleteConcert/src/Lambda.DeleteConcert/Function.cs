using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Events;
using LPCalendar.DataStructure.Requests;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.DeleteConcert;

/// <summary>
/// AWS Lambda function to delete a concert from the database
/// </summary>
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
    
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (!request.CanDeleteConcerts())
        {
            return ForbiddenResponseHelper.GetResponse("OPTIONS, GET, POST, DELETE");
        }
        
        var response = new APIGatewayProxyResponse
        {
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET, POST, DELETE" }
            }
        };
        
        if (!request.PathParameters.ContainsKey("concertId"))
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            return response;
        }

        var concertId = request.PathParameters["concertId"]!;

        DeleteConcertRequest deleteRequest;
        try
        {
            deleteRequest = new DeleteConcertRequest
            {
                ConcertId = concertId
            };
        }
        catch (Exception e)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            response.Body = $"{{\"message\": \"Failed to parse request: {e.GetType().Name} - {e.Message}\"}}";
            return response;
        }
        
        var oldValue = await _dynamoDbContext.LoadAsync<Concert>(deleteRequest.ConcertId, _dbConfigProvider.GetLoadConfigFor(DynamoDbConfigProvider.Table.Concerts));
        if (oldValue == null)
        {
            context.Logger.LogWarning("Could not find concert with ID '{0}'", concertId);
            response.StatusCode = (int)HttpStatusCode.NotFound;
            return response;
        }
        
        await _dynamoDbContext.DeleteAsync<Concert>(deleteRequest.ConcertId, _dbConfigProvider.GetDeleteConfigFor(DynamoDbConfigProvider.Table.Concerts));

        try
        {
            await LogChanges(oldValue, request.GetUserId(), context.Logger);
        }
        catch (Exception e)
        {
            context.Logger.LogError(e, "Failed to log the deletion in the audit log");
        }
        
        response.StatusCode = (int)HttpStatusCode.NoContent;

        return response;
    }


    private static DeleteConcertRequest? GetRequestFromJson(string json)
    {
        return JsonSerializer.Deserialize<DeleteConcertRequest>(json);
    }
    
    
    private async Task LogChanges(Concert? oldValue, string? userId, ILambdaLogger logger)
    {
        logger.LogDebug($"Log action 'Delete' made by {userId}...");
        var auditLogEvent = new AuditLogEvent
        {
            UserId = userId ?? "unknown",
            Action = "Concert_Delete",
            AffectedEntity = $"{Concert.ConcertTableName}#{oldValue?.Id}",
            Timestamp = DateTime.UtcNow
        };

        if (oldValue != null)
        {
            auditLogEvent.OldValue = JsonSerializer.Serialize(oldValue);
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