using Microsoft.AspNetCore.Authorization;

namespace Server.Api.Auth;

/// <summary>
/// Configures authorization based on a list of <see cref="RoleNames"/>
/// </summary>
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Configures authorization based on a list of role names and the admins
    /// </summary>
    /// <param name="roleNames">Names of the roles that can access the route/controller</param>
    public AuthorizeRolesAttribute(params string[] roleNames) : this(string.Join(",", roleNames))
    {
    }
    
    private AuthorizeRolesAttribute(string roles)
    {
        Roles = GetRolesString(roles);
    }

    /// <summary>
    /// Configures authorization for the <see cref="GetBaseRoles"/> and admins
    /// </summary>
    public AuthorizeRolesAttribute()
    {
        Roles = GetRolesString();
    }
    
    protected virtual string[] GetBaseRoles()
    {
        return Array.Empty<string>();
    }
    
    private string GetRolesString(string? additionalRoles = null)
    {
        List<string> roles =
        [
            .. GetBaseRoles(),
            RoleNames.Admin
        ];

        if (!string.IsNullOrEmpty(additionalRoles))
        {
            var additionalRolesArr = additionalRoles.Trim().Split(",");
            roles.AddRange(additionalRolesArr);
        }

        return string.Join(",", roles.Distinct().OrderBy(role => role));
    }
}
