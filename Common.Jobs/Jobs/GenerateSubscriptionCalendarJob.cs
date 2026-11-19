using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Common.Jobs.Jobs;

[QuartzJob(Name = CalendarSubscriptionJobConstants.GenerateSingleJobName, Group = CalendarSubscriptionJobConstants.GroupName)]
[DisallowConcurrentExecution]
public sealed class GenerateSubscriptionCalendarJob(
    CalendarSubscriptionGenerationCoordinator generationCoordinator,
    ILogger<GenerateSubscriptionCalendarJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Log.StartJob(logger, nameof(GenerateSubscriptionCalendarJob));
        var stopwatch = Stopwatch.StartNew();

        var subscriptionId = context.MergedJobDataMap.GetString(CalendarSubscriptionJobConstants.SubscriptionIdJobDataKey);
        if (string.IsNullOrWhiteSpace(subscriptionId))
        {
            Log.JobMissingRequiredData(logger, nameof(GenerateSubscriptionCalendarJob),
                CalendarSubscriptionJobConstants.SubscriptionIdJobDataKey);
            stopwatch.Stop();
            Log.FinishedJob(logger, nameof(GenerateSubscriptionCalendarJob), stopwatch.Elapsed.TotalSeconds,
                context.NextFireTimeUtc);
            return;
        }

        await generationCoordinator.GenerateSingleSubscriptionAsync(subscriptionId, cancellationToken);

        stopwatch.Stop();
        Log.FinishedJob(logger, nameof(GenerateSubscriptionCalendarJob), stopwatch.Elapsed.TotalSeconds,
            context.NextFireTimeUtc);
    }
}

