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
        => IsMemberOf(request, [groupName]);


    /// <summary>
    /// Checks in the request if the user is member of a given group or admin
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <param name="groupName">name of the group</param>
    /// <returns>true, if current user is member of the group or in the admin group</returns>
    public static bool IsMemberOfOrAdmin(this APIGatewayProxyRequest request, string groupName) 
        => IsMemberOf(request, groupName, "Admin");
    
    
    /// <summary>
    /// Checks in the request if the user is member of a given group or admin
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <param name="groupNames">name of the groups; Only one of the groups has to match</param>
    /// <returns>true, if current user is member of the group or in the admin group</returns>
    public static bool IsMemberOfOrAdmin(this APIGatewayProxyRequest request, params string[] groupNames) 
        => IsMemberOf(request, groupNames.Append("Admin").ToArray());
    
    
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
    /// Checks if the user of the request is allowed to add/publish concerts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanAddConcerts(this APIGatewayProxyRequest request)
        => request.IsMemberOfOrAdmin("AddConcerts");
    
    
    /// <summary>
    /// Checks if the user of the request is allowed to update concerts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanUpdateConcerts(this APIGatewayProxyRequest request)
        => request.IsMemberOfOrAdmin("UpdateConcerts");
    
    
    /// <summary>
    /// Checks if the user of the request is allowed to delete concerts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanDeleteConcerts(this APIGatewayProxyRequest request)
        => request.IsMemberOfOrAdmin("DeleteConcerts");


    /// <summary>
    /// Checks if the user of the request is allowed to manage other users
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanManageUsers(this APIGatewayProxyRequest request)
        => request.IsMemberOfOrAdmin("ManageUsers");
}