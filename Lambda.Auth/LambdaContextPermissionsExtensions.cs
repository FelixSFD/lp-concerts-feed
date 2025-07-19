using Amazon.Lambda.APIGatewayEvents;

namespace Lambda.Auth;

public static class ApiGatewayProxyRequestPermissionsExtensions
{
    /// <summary>
    /// Checks in the request if the user is member of a given group
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <param name="groupName">name of the group</param>
    /// <returns>true, if current user is member of the group</returns>
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