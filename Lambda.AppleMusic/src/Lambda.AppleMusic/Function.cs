using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AppleDeveloperToken;
using Common.Utils.Cache;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.AppleMusic;

public class Function
{
    private readonly string _privateKey = Environment.GetEnvironmentVariable("APPLE_MUSIC_PRIVATE_KEY") ?? throw new Exception("Environment variable 'APPLE_MUSIC_PRIVATE_KEY' missing");
    private readonly string _teamId = Environment.GetEnvironmentVariable("APPLE_TEAM_ID") ?? throw new Exception("Environment variable 'APPLE_TEAM_ID' missing");
    private readonly string _keyId = Environment.GetEnvironmentVariable("APPLE_MUSIC_KEY_ID") ??  throw new Exception("Environment variable 'APPLE_MUSIC_KEY_ID' missing");


    /// <summary>
    /// Generates a developer token for the Apple Music API
    /// </summary>
    public APIGatewayProxyResponse FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogDebug($"Path: {request.Path}");
        
        if (request is { Resource: "/apple-music/token", HttpMethod: "GET" })
        {
            var tokenGenerator = new TokenGenerator(_privateKey, _teamId, _keyId);
            var token = tokenGenerator.Generate(TimeSpan.FromDays(3));
            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = token,
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" },
                    { "Cache-Control", CacheControlHeaderFactory.CacheFor(CacheExpiration.Maximum) }
                }
            };
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
}