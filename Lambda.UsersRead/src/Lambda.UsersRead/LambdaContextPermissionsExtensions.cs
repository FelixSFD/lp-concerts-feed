using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Lambda.UsersRead;

public static class LambdaContextPermissionsExtensions
{
    public static bool IsMemberOf(this APIGatewayProxyRequest request, string groupName)
    {
        return request.RequestContext.Authorizer.Claims
                // find claims for groups
            .Where(c => c.Key == "cognito:groups")
            .Select(c => c.Value)
                // split string to get groups
            .SelectMany(v => v.Split(","))
                // check if group is included
            .Any(g => g == groupName);
    }
}