using Common.Jobs.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
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
                options.DefaultTimeZone = TimeZoneInfo.Utc;
            });
            
            q.UsePersistentStore(store =>
            {
                store.UseMySql(connectionString);
                store.UseClustering();
                store.ProvisionSchema();
            });

            // add the jobs from attributes
            q.AddDeclaredJobs();
        });
        
        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        
        services.AddQuartzDashboard(options =>
        {
            options.DashboardPath = "/quartz";
            options.ReadOnly = true;
        });
        
        return services;
    }

    public static async Task<IApplicationBuilder> RegisterScheduledJobs(this IApplicationBuilder app)
    {
        return app;
    }
}