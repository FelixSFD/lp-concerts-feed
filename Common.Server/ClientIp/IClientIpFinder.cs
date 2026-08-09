using Microsoft.AspNetCore.Http;

namespace Common.Server.ClientIp;

/// <summary>
/// Service that can try to fetch the client IP from a <see cref="HttpRequest"/>
/// </summary>
internal interface IClientIpFinder
{
    /// <summary>
    /// Tries to find the IP of the client from various header fields in the <paramref name="request"/>
    /// </summary>
    /// <param name="request">the HTTP request to check for the IP</param>
    /// <returns>the IP or null, if no IP could be found</returns>
    public string? GetClientIp(HttpRequest request);
}