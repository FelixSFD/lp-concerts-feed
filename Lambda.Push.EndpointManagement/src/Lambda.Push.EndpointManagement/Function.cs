using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Entities;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.Push.EndpointManagement;

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
    
    
    /// <summary>
    /// Set bookmark status for a user on a concert
    /// </summary>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogDebug($"Path: {request.Path}");
        
        if (request is { Resource: "/notifications/push", HttpMethod: "PUT" })
        {
            context.Logger.LogInformation("Registering device for push notifications");
            try
            {
                var registration =
                    JsonSerializer.Deserialize(request.Body, DataStructureJsonContext.Default.NotificationRegistration);
                return await Register(registration!);
            } catch (Exception e)
            {
                context.Logger.LogError(e, "Error parsing notification registration");
                var jsonError = new ErrorResponse
                {
                    Message = "Failed to parse input"
                };
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Body = JsonSerializer.Serialize(jsonError, DataStructureJsonContext.Default.ErrorResponse),
                    Headers = new Dictionary<string, string>
                    {
                        { "Access-Control-Allow-Origin", "*" },
                        { "Access-Control-Allow-Methods", "OPTIONS, PUT" }
                    }
                };
            }
        }

        var error = new ErrorResponse
        {
            Message = "Invalid route"
        };
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.BadRequest,
            Body = JsonSerializer.Serialize(error, DataStructureJsonContext.Default.ErrorResponse),
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }

    private async Task<APIGatewayProxyResponse> Register(NotificationRegistration registration)
    {
        await _dynamoDbContext.SaveAsync(registration, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.NotificationRegistrations));
        
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.NoContent,
            Headers = new Dictionary<string, string>
            {
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, PUT" }
            }
        };
    }
}