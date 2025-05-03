using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Lambda.GetTimeZone;

public class Function(HttpClient httpClient)
{
    private const string TzApiBaseUrl = "https://api.timezonedb.com/v2.1";
    private readonly string _apiKey = Environment.GetEnvironmentVariable("TZDB_API_KEY") ?? throw new Exception("Environment variable TZDB_API_KEY not found!");
    
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogInformation($"Path: {request.Path}");
        context.Logger.LogInformation($"Request: {JsonSerializer.Serialize(request)}");
        
        var getTzRequest = JsonSerializer.Deserialize<GetTimeZoneByCoordinatesRequest>(request.Body);
        if (getTzRequest == null)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "{\"message\": \"Could not parse request JSON.\"}",
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }
        
        var uri = new Uri($"{TzApiBaseUrl}/get-time-zone?key={_apiKey}&by=position&lat={getTzRequest.Latitude}&lng={getTzRequest.Longitude}&format=json");
        var httpResponseMessage = await httpClient.GetAsync(uri);
        var responseJson = await httpResponseMessage.Content.ReadAsStringAsync();
        var responseDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);
        if (responseDictionary?.TryGetValue("zoneName", out var zoneName) ?? false)
        {
            GetTimeZoneByCoordinatesResponse response = new()
            {
                TimeZoneId = zoneName
            };
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(response),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Access-Control-Allow-Origin", "*" },
                    { "Access-Control-Allow-Methods", "OPTIONS, GET" }
                }
            };
        }

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
}