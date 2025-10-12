using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lambda.Common.ApiGateway;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.GetTimeZone;

public class Function(HttpClient httpClient, string apiKey)
{
    private const string TzApiBaseUrl = "https://api.timezonedb.com/v2.1";

    public Function() : this(new HttpClient(), Environment.GetEnvironmentVariable("TZDB_API_KEY") ?? throw new Exception("Environment variable TZDB_API_KEY not found!"))
    {
    }


    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogDebug("Path: {path}", request.Path);
        context.Logger.LogDebug("Request: {request}", JsonSerializer.Serialize(request, ApiGatewayJsonContext.Default.APIGatewayProxyRequest));

        if (!request.QueryStringParameters.TryGetValue("lat", out var lat))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "{\"message\": \"Parameter 'lat' missing!\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        
        if (!request.QueryStringParameters.TryGetValue("lon", out var lon))
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "{\"message\": \"Parameter 'lon' missing!\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        
        var uri = new Uri($"{TzApiBaseUrl}/get-time-zone?key={apiKey}&by=position&lat={lat}&lng={lon}&format=json");
        var httpResponseMessage = await httpClient.GetAsync(uri);
        var responseJson = await httpResponseMessage.Content.ReadAsStringAsync();
        var apiResponse = JsonSerializer.Deserialize(responseJson, LocalJsonContext.Default.TimeZoneDbResponse);

        if (apiResponse == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = "{\"message\": \"Failed to parse API response.\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }

        GetTimeZoneByCoordinatesResponse response = new()
        {
            TimeZoneId = apiResponse.ZoneName
        };
            
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(response, DataStructureJsonContext.Default.GetTimeZoneByCoordinatesResponse),
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", "*" },
                { "Access-Control-Allow-Methods", "OPTIONS, GET" }
            }
        };

    }
}