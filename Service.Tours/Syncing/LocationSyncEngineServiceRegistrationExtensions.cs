using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.DependencyInjection;

namespace Service.Tours.Syncing;

public static class LocationSyncEngineServiceRegistrationExtensions
{
    /// <summary>
    /// Add sync engines for locations (venues, cities, countries).
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLocationSyncEngines(this IServiceCollection services)
    {
        return services
            .AddScoped<ISyncEngine<VenueWithDetailsBo, uint>, VenueSyncEngine>()
            .AddScoped<ISyncEngine<CityWithCountryBo, CityIdentifierBo>, CitySyncEngine>()
            .AddScoped<ISyncEngine<CountryBo, string>, CountrySyncEngine>();
    }
}
