using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.SharePics.Gotenberg;

namespace Service.SharePics;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharePicService(this IServiceCollection services, string gotenbergBaseUrl)
    {
        return services.AddSharePicService(new Uri(gotenbergBaseUrl));
    }
    
    public static IServiceCollection AddSharePicService(this IServiceCollection services, Uri gotenbergBaseUri)
    {
        services.AddScoped<IGotenbergClient, GotenbergClient>(builder =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = gotenbergBaseUri
            };
            var logger = builder.GetRequiredService<ILogger<GotenbergClient>>();
            return new GotenbergClient(httpClient, logger);
        });
        services.AddScoped<SharePicService>();
        return services;
    }
}