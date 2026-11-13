using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Common.Jobs;

public static class QuartzServiceRegistrationExtensions
{
    /// <summary>
    /// Configures the Quartz services with MySQL storage and adds the Quartz server to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionString">Connection string to the DB that stores the job data</param>
    /// <returns></returns>
    public static IServiceCollection AddJobServices(this IServiceCollection services, string connectionString)
    {
        services.AddQuartz(q =>
        {
            q.ConfigureScheduler(options =>
            {
                options.InstanceName = "LPshows";
                options.GenerateInstanceId = true;
            });
            
            q.UsePersistentStore(store =>
            {
                store.UseMySqlConnector(connectionString).ConfigureStore(options =>
                {
                    options.TablePrefix = "QRTZ_";
                });

                store.UseClustering();
            });
            
            services.AddQuartzDashboard(options =>
            {
                options.DashboardPath = "/quartz";
                options.ReadOnly = true;
            });
        });
        return services;
    }
}