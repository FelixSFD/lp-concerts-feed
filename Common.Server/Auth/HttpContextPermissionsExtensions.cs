using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Common.Server.Auth;

public static class HttpContextPermissionsExtensions
{
    private const string GroupNameAdmin = "Admin";
    
    /// <summary>
    /// Checks if the client is authenticated. To check for the actual permissions, use methods like <see cref="IsMemberOfOrAdmin(Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest,string)"/>
    /// </summary>
    /// <param name="request"></param>
    /// <returns>true, if the client is authenticated</returns>
    public static bool IsAuthenticated(this HttpContext request) 
        => request.GetUserId() != null;
    
    /// <summary>
    /// Tries to find the UserID in the request context
    /// </summary>
    /// <param name="request"></param>
    /// <returns>id of user if found</returns>
    public static string? GetUserId(this HttpContext request) =>
        request.User.Claims
            .Where(kv => kv.Type == ClaimTypes.NameIdentifier) // this contains the user ID
            .Select(kv => kv.Value)
            .FirstOrDefault();

    /// <summary>
    /// Checks in the request if the user is in the admin group
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <returns>true, if current user is member of the group</returns>
    public static bool IsAdmin(this HttpContext request) 
        => IsMemberOf(request, [GroupNameAdmin]);

    /// <summary>
    /// Checks in the request if the user is member of a given group
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <param name="groupName">name of the group</param>
    /// <returns>true, if current user is member of the group</returns>
    public static bool IsMemberOf(this HttpContext request, string groupName) 
        => IsMemberOf(request, [groupName]);


    /// <summary>
    /// Checks in the request if the user is member of a given group or admin
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <param name="groupName">name of the group</param>
    /// <returns>true, if current user is member of the group or in the admin group</returns>
    public static bool IsMemberOfOrAdmin(this HttpContext request, string groupName) 
        => IsMemberOf(request, groupName, GroupNameAdmin);
    
    
    /// <summary>
    /// Checks in the request if the user is member of a given group or admin
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <param name="groupNames">name of the groups; Only one of the groups has to match</param>
    /// <returns>true, if current user is member of the group or in the admin group</returns>
    public static bool IsMemberOfOrAdmin(this HttpContext request, params string[] groupNames) 
        => IsMemberOf(request, groupNames.Append(GroupNameAdmin).ToArray());
    
    
    /// <summary>
    /// Checks in the request if the user is member of a given group
    /// </summary>
    /// <param name="request">Request that contains the claims</param>
    /// <param name="groupNames">name of the groups; Only one of the groups has to match</param>
    /// <returns>true, if current user is member of the group</returns>
    public static bool IsMemberOf(this HttpContext request, params string[] groupNames)
    {
        return request?.User.Claims
            // find claims for groups
            .Where(c => c.Type == "cognito:groups")
            .Select(c => c.Value)
            // split string to get groups
            .SelectMany(v => v.Split(","))
            // check if group is included
            .Any(groupNames.Contains) ?? false;
    }


    /// <summary>
    /// Checks if the user of the request is allowed to add/publish concerts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanAddConcerts(this HttpContext request)
        => request.IsMemberOfOrAdmin("AddConcerts");
    
    
    /// <summary>
    /// Checks if the user of the request is allowed to update concerts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanUpdateConcerts(this HttpContext request)
        => request.IsMemberOfOrAdmin("UpdateConcerts");
    
    
    /// <summary>
    /// Checks if the user of the request is allowed to delete concerts
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanDeleteConcerts(this HttpContext request)
        => request.IsMemberOfOrAdmin("DeleteConcerts");


    /// <summary>
    /// Checks if the user of the request is allowed to manage other users
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanManageUsers(this HttpContext request)
        => request.IsMemberOfOrAdmin("ManageUsers");
    
    /// <summary>
    /// Checks if the user of the request is allowed to manage setlists
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanManageSetlists(this HttpContext request)
        => request.IsMemberOfOrAdmin("ManageSetlists");
    
    /// <summary>
    /// Checks if the user of the request is allowed to delete data like songs or albums
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static bool CanDeleteSongs(this HttpContext request)
        => request.IsMemberOfOrAdmin("DeleteSongs");
}