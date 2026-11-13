using Microsoft.Extensions.Logging;
using Quartz;

namespace Common.Jobs.Jobs;

[QuartzJob(Name = "UpdateProfileBanners", Group = "LPshows")]
[CronTrigger("0 0/5 * * * ?")]
public sealed class UpdateProfileBannersJob(ILogger<UpdateProfileBannersJob> logger) : IJob
{
    public ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing UpdateProfileBannersJob");
        return ValueTask.CompletedTask;
    }
}