using Database.Tours;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Server.Api.HealthChecks;

/// <summary>
/// Health check to verify the DB connection
/// </summary>
public class DbConnectedHealthCheck(ToursDbContext toursDbContext, ILogger<DbConnectedHealthCheck> logger) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            await toursDbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Health check failed!");
            return new HealthCheckResult(
                context.Registration.FailureStatus, $"Connection to database not possible! {e.Message}");
        }
        
        return HealthCheckResult.Healthy();
    }
}