using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.DependencyInjection;

namespace Service.Tours.Syncing;

public static class ConcertSyncEngineServiceRegistrationExtensions
{
    /// <summary>
    /// Add a <see cref="ISyncEngine{ConcertDetailsBo, string}"/> to sync concerts.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddConcertSyncEngine(this IServiceCollection services)
    {
        return services.AddScoped<ISyncEngine<ConcertDetailsBo, string>, ConcertSyncEngine>();
    }
}