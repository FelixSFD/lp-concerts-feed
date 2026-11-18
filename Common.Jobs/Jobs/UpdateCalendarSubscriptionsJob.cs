using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Common.Jobs.Jobs;

[QuartzJob(Name = CalendarSubscriptionJobConstants.UpdateAllJobName, Group = CalendarSubscriptionJobConstants.GroupName)]
[CronTrigger("0 0 * ? * *", Name = "UpdateCalendarSubscriptions-hourly")]
[DisallowConcurrentExecution]
public sealed class UpdateCalendarSubscriptionsJob(
    CalendarSubscriptionGenerationCoordinator generationCoordinator,
    ILogger<UpdateCalendarSubscriptionsJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Log.StartJob(logger, nameof(UpdateCalendarSubscriptionsJob));
        var stopwatch = Stopwatch.StartNew();

        await generationCoordinator.GenerateOutdatedSubscriptionsAsync(cancellationToken);

        stopwatch.Stop();
        Log.FinishedJob(logger, nameof(UpdateCalendarSubscriptionsJob), stopwatch.Elapsed.TotalSeconds,
            context.NextFireTimeUtc);
    }
}

