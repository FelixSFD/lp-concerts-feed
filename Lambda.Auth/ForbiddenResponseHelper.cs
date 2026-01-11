using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using LPCalendar.DataStructure.Responses;

namespace Lambda.Auth;

public static class ForbiddenResponseHelper
{
    public static APIGatewayProxyResponse GetResponse(string corsMethods, string message = "Missing permission to perform this action", string corsOrigin = "*")
    {
        var errResponse = new ErrorResponse
        {
            Message = message
        };
        var bodyJson = JsonSerializer.Serialize(errResponse, AuthJsonContext.Default.ErrorResponse);
        
        return new APIGatewayProxyResponse()
        {
            Body = bodyJson,
            StatusCode = (int)HttpStatusCode.Forbidden,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Access-Control-Allow-Origin", corsOrigin },
                { "Access-Control-Allow-Methods", corsMethods }
            }
        };
    }
}