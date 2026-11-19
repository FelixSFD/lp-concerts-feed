using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Common.Jobs.Jobs;

[QuartzJob(Name = CalendarSubscriptionJobConstants.UpdateAllJobName, Group = CalendarSubscriptionJobConstants.GroupName, Description = "Updates all calendar subscriptions that are outdated. Use the JobData forceAll=true to force all subscriptions to be regenerated.")]
[CronTrigger("0 0 * ? * *", Name = "UpdateCalendarSubscriptions-hourly", Description = "Update calendar subscriptions hourly")]
[DisallowConcurrentExecution]
public sealed class UpdateCalendarSubscriptionsJob(
    CalendarSubscriptionGenerationCoordinator generationCoordinator,
    ILogger<UpdateCalendarSubscriptionsJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Log.StartJob(logger, nameof(UpdateCalendarSubscriptionsJob));
        var stopwatch = Stopwatch.StartNew();

        var force = context.MergedJobDataMap.GetValueOrDefault(CalendarSubscriptionJobConstants.ForceAllJobDataKey, false);
        if (force)
        {
            Log.ForceGenerateAllSubscriptionCalendars(logger);
        }
        await generationCoordinator.GenerateOutdatedSubscriptionsAsync(force, cancellationToken);

        stopwatch.Stop();
        Log.FinishedJob(logger, nameof(UpdateCalendarSubscriptionsJob), stopwatch.Elapsed.TotalSeconds,
            context.NextFireTimeUtc);
    }
}

