using System.Net;
using Amazon.Lambda.APIGatewayEvents;

namespace Lambda.Auth;

public static class ForbiddenResponseHelper
{
    public static APIGatewayProxyResponse GetResponse(string corsMethods, string contentType = "application/json", string corsOrigin = "*")
    {
        return new APIGatewayProxyResponse()
        {
            // TODO: Error message?
            StatusCode = (int)HttpStatusCode.Forbidden,
            Headers = new Dictionary<string, string>
            {
                { "Content-Type", contentType },
                { "Access-Control-Allow-Origin", corsOrigin },
                { "Access-Control-Allow-Methods", corsMethods }
            }
        };
    }
}