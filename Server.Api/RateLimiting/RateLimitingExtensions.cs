using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Server.Api.RateLimiting;

/// <summary>
/// Extensions to configure rate limiting
/// </summary>
internal static class RateLimitingExtensions
{
    /// <summary>
    /// Configure the rate limiting policies for this application. Rate limiting needs to be enabled 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRateLimitPolicies(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(RateLimitingPolicyNames.FixedLowLimit, opt =>
            {
                opt.PermitLimit = 10;
                opt.Window = TimeSpan.FromSeconds(10);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 5;
            });
        });
        
        return services;
    }
}