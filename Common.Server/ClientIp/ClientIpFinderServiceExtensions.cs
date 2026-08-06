using Microsoft.Extensions.DependencyInjection;

namespace Common.Server.ClientIp;

public static class ClientIpFinderServiceExtensions
{
    /// <summary>
    /// Configures the <see cref="IClientIpFinder"/> service
    /// </summary>
    /// <param name="services"></param>
    public static void UseClientIpFinder(this IServiceCollection services)
    {
        services.AddSingleton<IClientIpFinder, ClientIpFinder>();
    }
}