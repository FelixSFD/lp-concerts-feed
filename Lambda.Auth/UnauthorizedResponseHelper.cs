using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using LPCalendar.DataStructure.Responses;

namespace Lambda.Auth;

public static class UnauthorizedResponseHelper
{
    public static APIGatewayProxyResponse GetResponse(string corsMethods, string message = "You are not logged in", string corsOrigin = "*")
    {
        var errResponse = new ErrorResponse
        {
            Message = message
        };
        var bodyJson = JsonSerializer.Serialize(errResponse);
        
        return new APIGatewayProxyResponse()
        {
            Body = bodyJson,
            StatusCode = (int)HttpStatusCode.Unauthorized,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", corsOrigin },
                { "Access-Control-Allow-Methods", corsMethods }
            }
        };
    }
}