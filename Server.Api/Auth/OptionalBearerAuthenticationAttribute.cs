using Common.Server.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Server.Api.Auth;

/// <summary>Allows anonymous requests but rejects invalid credentials and incomplete identities.</summary>
public sealed class OptionalBearerAuthenticationAttribute : Attribute, IAsyncAuthorizationFilter
{
    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        var result = await http.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
        if (result.Failure is not null ||
            (http.Request.Headers.ContainsKey("Authorization") && !result.Succeeded) ||
            (result.Succeeded && string.IsNullOrEmpty(http.GetUserId())))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
