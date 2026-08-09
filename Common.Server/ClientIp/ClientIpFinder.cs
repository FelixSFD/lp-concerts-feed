using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Server.ClientIp;

public class ClientIpFinder(ILogger<ClientIpFinder> logger) : IClientIpFinder
{
    /// <inheritdoc/>
    public string? GetClientIp(HttpRequest request)
    {
        logger.LogDebug("Trying to get client ip from the request information...");
        logger.LogTrace("Checking X-Warp-Trusted...");
        var ip = request.Headers["X-Warp-Trusted"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ip))
        {
            logger.LogDebug("X-Warp-Trusted found: {foundId}", ip);
            return ip;
        }
        
        logger.LogTrace("Checking X-Real-IP...");
        ip = request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ip))
        {
            logger.LogDebug("X-Real-IP found: {ip}", ip);
            return ip;
        }
        
        logger.LogTrace("Checking X-Forwarded-For...");
        ip = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(ip))
        {
            logger.LogDebug("X-Forwarded-For starts with IP: {ip}", ip);
            return ip;
        }
        
        logger.LogTrace("Checking connection client...");
        ip = request.HttpContext.Connection.RemoteIpAddress.ToString();
        if (!string.IsNullOrEmpty(ip))
        {
            logger.LogDebug("Connection has IP: {ip}; Note that this can be a reverse proxy in many cases", ip);
            return ip;
        }

        return null;
    }
}