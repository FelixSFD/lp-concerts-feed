using Common.Server.ClientIp;
using Microsoft.AspNetCore.Http;

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
}