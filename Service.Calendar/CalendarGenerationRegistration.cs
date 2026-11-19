using System.Configuration;
using Database.Tours.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Service.Calendar;

/// <summary>Registers calendar generation using the shared frontend configuration.</summary>
public static class CalendarGenerationRegistration
{
    /// <summary>Registers calendar generation with the optional frontend base URL.</summary>
    /// <param name="services">Services to register the calendar generator with.</param>
    /// <param name="configuration">Configuration containing Frontend:BaseUrl.</param>
    /// <returns>The service collection.</returns>
    /// <exception cref="ConfigurationErrorsException">The frontend URL is invalid.</exception>
    public static IServiceCollection AddCalendarGeneration(this IServiceCollection services, Func<IServiceProvider, CalendarGenerationRegistrationOptions, CalendarGenerationRegistrationOptions> configureOptions)
    {
        var options = new CalendarGenerationRegistrationOptions
        {
            FrontendBaseUrl = string.Empty,
        };
        
        var serviceProvider = services.BuildServiceProvider();
        options = configureOptions(serviceProvider, options);

        if (!string.IsNullOrWhiteSpace(options.FrontendBaseUrl))
        {
            if (!Uri.TryCreate(options.FrontendBaseUrl, UriKind.Absolute, out var frontendBaseUri) ||
                (frontendBaseUri.Scheme != Uri.UriSchemeHttp && frontendBaseUri.Scheme != Uri.UriSchemeHttps) ||
                !string.IsNullOrEmpty(frontendBaseUri.Query) || !string.IsNullOrEmpty(frontendBaseUri.Fragment))
                throw new ConfigurationErrorsException("Frontend:BaseUrl must be an absolute HTTP(S) URL without a query or fragment.");

            services.AddScoped(provider => new CalendarGenerationService(provider.GetRequiredService<IConcertRepository>(), frontendBaseUri));
        }
        else
        {
            services.AddScoped(provider => new CalendarGenerationService(provider.GetRequiredService<IConcertRepository>()));
        }

        return services;
    }
}