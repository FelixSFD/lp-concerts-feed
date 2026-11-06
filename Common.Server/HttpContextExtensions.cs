using Common.Server.Auth;
using Common.Server.ClientIp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Server;

public static class HttpContextExtensions
{
    /// <summary>
    /// Tries to retrieve the client's IP. This is not trivial as multiple headers might contain the value.
    /// And even then, there is still no guarantee that this is the correct IP.
    /// </summary>
    /// <param name="request"></param>
    /// <returns>IP or null if no information was found</returns>
    public static string? GetRequestClientIp(this HttpRequest request)
    {
        var finderService = (IClientIpFinder?) request.HttpContext.RequestServices.GetService(typeof(IClientIpFinder));
        return request.GetRequestClientIp(finderService);
    }
    
    private static string? GetRequestClientIp(this HttpRequest request, IClientIpFinder? clientIpFinder)
    {
        if (clientIpFinder == null)
            throw new ArgumentNullException(nameof(clientIpFinder), "The IClientIpFinder was not registered.");
        
        return clientIpFinder.GetClientIp(request);
    }

    /// <summary>
    /// Disable caching if the request is authenticated. This only applies to external caches!
    /// </summary>
    /// <param name="context">HTTP-context</param>
    public static void DisableCacheIfAuthenticated(this HttpContext context)
    {
        if (!context.IsAuthenticated()) return;
        
        context.Response.Headers.CacheControl = "no-cache";
        var logger = context.RequestServices.GetService(typeof(ILogger<HttpContext>)) as ILogger<HttpContext>;
        Log.DisabledCacheForAuthenticatedRequest(logger);
    }
}