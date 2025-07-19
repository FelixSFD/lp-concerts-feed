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
        return IsMemberOf(request, [groupName]);
    }
    
    
    /// <summary>
    /// Checks in the request if the user is member of a given group
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <param name="groupNames">name of the groups; Only one of the groups has to match</param>
    /// <returns>true, if current user is member of the group</returns>
    public static bool IsMemberOf(this APIGatewayProxyRequest request, params string[] groupNames)
    {
        return request.RequestContext.Authorizer.Claims
            // find claims for groups
            .Where(c => c.Key == "cognito:groups")
            .Select(c => c.Value)
            // split string to get groups
            .SelectMany(v => v.Split(","))
            // check if group is included
            .Any(groupNames.Contains);
    }


    /// <summary>
    /// Checks if the user of the request is allowed to manage other users
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanManageUsers(this APIGatewayProxyRequest request)
        => request.IsMemberOf("ManageUsers", "Admin");
}