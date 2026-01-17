using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Lambda.Auth;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Entities;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.Push.EndpointManagement;

public class Function
{
    private readonly DynamoDBContext _dynamoDbContext;
    private readonly DynamoDbConfigProvider _dbConfigProvider = new();
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly string _platformApplicationArn;
    
    public Function()
    {
        _platformApplicationArn = Environment.GetEnvironmentVariable("PLATFORM_APPLICATION_ARN")
                                  ?? throw new Exception("Missing environment variable PLATFORM_APPLICATION_ARN");
        
        _sns = new AmazonSimpleNotificationServiceClient();
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
            
            RegisterNotificationDeviceRequest? registerRequest;
            try
            {
                registerRequest = JsonSerializer.Deserialize(request.Body, DataStructureJsonContext.Default.RegisterNotificationDeviceRequest);
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
            
            return await Register(registerRequest!, context.Logger);
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

    private async Task<APIGatewayProxyResponse> Register(RegisterNotificationDeviceRequest request, ILambdaLogger logger)
    {
        logger.LogDebug("Registering device for push notifications for user '{user}'", request.UserId);
        var snsCreateEndpointRequest = new CreatePlatformEndpointRequest
        {
            PlatformApplicationArn = _platformApplicationArn,
            Token = request.DeviceToken,
            CustomUserData = request.UserId
        };

        var snsEndpointResponse = await _sns.CreatePlatformEndpointAsync(snsCreateEndpointRequest);
        logger.LogDebug("Created platform endpoint for push notifications: {arn}",  snsEndpointResponse.EndpointArn);

        var notificationUserEndpoint = new NotificationUserEndpoint
        {
            UserId = request.UserId,
            EndpointArn = snsEndpointResponse.EndpointArn,
            LastChange = DateTimeOffset.UtcNow
        };
        await _dynamoDbContext.SaveAsync(notificationUserEndpoint, _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.NotificationRegistrations));
        
        logger.LogDebug("Saved notification user endpoint: {arn}", notificationUserEndpoint.EndpointArn);
        
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