using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.SharePics.Gotenberg;
using Service.SharePics.Storage;

namespace Service.SharePics;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharePicService(this IServiceCollection services, string gotenbergBaseUrl, string templateBaseDir)
    {
        return services.AddSharePicService(new Uri(gotenbergBaseUrl), templateBaseDir);
    }
    
    public static IServiceCollection AddSharePicService(this IServiceCollection services, Uri gotenbergBaseUri, string templateBaseDir)
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
        services.AddScoped<SharePicService>(builder =>
        {
            var gotenbergClient = builder.GetRequiredService<IGotenbergClient>();
            var logger = builder.GetRequiredService<ILogger<SharePicService>>();
            return new SharePicService(gotenbergClient, templateBaseDir, logger);
        });
        return services;
    }

    public static IServiceCollection AddSharePicStoreS3(this IServiceCollection services, Action<S3SharePicStoreOptions> optionsBuilder)
    {
        var opt = new S3SharePicStoreOptions
        {
            BucketName = "",
            KeyPrefix = "share/"
        };

        optionsBuilder(opt);
        services.AddScoped<ISharePicStore, S3SharePicStore>(provider => new S3SharePicStore(provider.GetRequiredService<IAmazonS3>(), opt,
            provider.GetRequiredService<ILogger<S3SharePicStore>>()));

        return services;
    }
}