using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Converters;
using LPCalendar.DataStructure.DbConfig;
using LPCalendar.DataStructure.Entities;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.UserNotificationsSettings;

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


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (request.Resource != "/users/{id}/notifications/settings")
        {
            var badRequestResponse = new ErrorResponse
            {
                Message = $"Path '{request.Resource}' can't be handled by this function."
            };
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET, PUT" }
                }
            };
        }
        
        var foundPathParam = request.PathParameters.TryGetValue("id", out var userId);
        if (!foundPathParam || userId == null)
        {
            var badRequestResponse = new ErrorResponse
            {
                Message = "UserId not set"
            };
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        
        context.Logger.LogInformation("Found ID: {id}", userId);

        switch (request.HttpMethod)
        {
            case "GET":
                return await GetNotificationSettingsForUser(userId);
            case "PUT":
                return await SaveNotificationSettingsForUser(userId, request.Body);
            default:
            {
                var errorResponse = new ErrorResponse
                {
                    Message = $"Unsupported method '{request.HttpMethod}'"
                };
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                    Body = JsonSerializer.Serialize(errorResponse, DataStructureJsonContext.Default.ErrorResponse),
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" },
                        { "Access-Control-Allow-Origin", "*" },
                        { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                    }
                };
            }
        }
    }


    private async Task<APIGatewayProxyResponse> GetNotificationSettingsForUser(string userId)
    {
        var config = _dbConfigProvider.GetQueryConfigFor(DynamoDbConfigProvider.Table.UserNotificationSettings);
        var query = _dynamoDbContext.QueryAsync<UserNotificationSettings>(userId, config);
        var settingsList = await query.GetRemainingAsync();
        var settings = settingsList.FirstOrDefault(s => s.UserId == userId, new UserNotificationSettings
        {
            UserId =  userId
        });
        
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(settings, DataStructureJsonContext.Default.UserNotificationSettings),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };
    }
    
    
    private async Task<APIGatewayProxyResponse> SaveNotificationSettingsForUser(string userId, string jsonBody)
    {
        var settings = JsonSerializer.Deserialize(jsonBody, DataStructureJsonContext.Default.UserNotificationSettings);
        if (settings == null)
        {
            var badRequestResponse = new ErrorResponse
            {
                Message = "Failed to parse request body"
            };
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(badRequestResponse, DataStructureJsonContext.Default.ErrorResponse),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, PUT" }
                }
            };
        }
        
        settings.UserId = userId;
        //settings.LastUpdated = DateTimeOffset.UtcNow;
        
        var config = _dbConfigProvider.GetSaveConfigFor(DynamoDbConfigProvider.Table.UserNotificationSettings);
        
        await _dynamoDbContext.SaveAsync(settings, config);
        
        return new APIGatewayProxyResponse
        {
            StatusCode = 204,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, PUT" }
            }
        };
    }
}