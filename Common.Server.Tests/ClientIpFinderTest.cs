using System.Net;
using Common.Server.ClientIp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Common.Server.Tests;

public class ClientIpFinderTest
{
    ClientIpFinder _finder;
    
    public ClientIpFinderTest()
    {
        var logger = Substitute.For<ILogger<ClientIpFinder>>();
        _finder = new ClientIpFinder(logger);
    }
    
    [Theory]
    [InlineData("192.168.1.2", "192.168.1.1", "192.168.1.2")]
    [InlineData("192.168.1.1", "192.168.1.1", null)]
    [InlineData("192.168.1.1", "192.168.1.1", null, "192.168.2.1", "192.168.2.2", "192.168.2.3")]
    [InlineData("192.168.2.1", null, null, "192.168.2.1", "192.168.2.2", "192.168.2.3")]
    [InlineData("10.10.1.1", null, null)]
    public void GetIp(string? expectedIp, string? realIp, string? warpTrustedIp, params string[] forwardedFor)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = new IPAddress([10, 10, 1, 1]);
        var request = httpContext.Request;
        
        if (realIp != null)
            _ = request.Headers.TryAdd("X-Real-IP", realIp);
        
        if (warpTrustedIp != null)
            _ = request.Headers.TryAdd("X-Warp-Trusted", warpTrustedIp);
        
        if (forwardedFor is { Length: > 0 })
            _ = request.Headers.TryAdd("X-Forwarded-For", forwardedFor);
        
        // run the test
        var actualIp = _finder.GetClientIp(request);
        Assert.Equal(expectedIp, actualIp);
    }
}