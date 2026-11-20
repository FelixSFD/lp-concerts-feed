using Microsoft.Extensions.DependencyInjection;

namespace Service.TimeZones;

/// <summary>
/// Options to configure the <see cref="TimeZoneService"/>
/// </summary>
public class TimeZoneServiceConfigurationOptions
{
    /// <summary>
    /// Base URL for the API that provides time zone information.
    /// </summary>
    public string?TimeZoneApiBaseUrl { get; set; }
}


public static class TimeZoneServiceConfigurationExtensions
{
    /// <summary>
    /// Adds the <see cref="TimeZoneService"/> to the service collection with the specified configuration options.
    /// </summary>
    /// <param name="services">The service collection to which the service will be added.</param>
    /// <param name="configureOptions">An action to configure the <see cref="TimeZoneServiceConfigurationOptions"/>.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddTimeZoneService(this IServiceCollection services, Func<IServiceProvider, TimeZoneServiceConfigurationOptions, TimeZoneServiceConfigurationOptions> configureOptions)
    {
        services.AddSingleton<ITimeZoneService>(serviceProvider =>
        {
            var options = new TimeZoneServiceConfigurationOptions();
            options = configureOptions(serviceProvider, options);
            
            var httpClient = serviceProvider.GetRequiredService<HttpClient>();
            var baseUrl = options.TimeZoneApiBaseUrl ?? throw new ArgumentException(
                $"{nameof(options.TimeZoneApiBaseUrl)} must be provided");
            return new TimeZoneService(baseUrl, httpClient);
        });

        return services;
    }
}