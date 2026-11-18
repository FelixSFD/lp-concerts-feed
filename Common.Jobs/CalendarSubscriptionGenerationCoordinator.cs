using Amazon.S3;
using Amazon.S3.Model;
using Database.Tours.Repositories;
using Database.Users.DataObjects;
using Database.Users.Repositories;
using Ical.Net.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Calendar;
using Service.Calendar.DataStructure;

namespace Common.Jobs;

/// <summary>
/// Coordinates generation and storage of subscription calendar files.
/// </summary>
public sealed class CalendarSubscriptionGenerationCoordinator(
    ICalendarSubscriptionRepository subscriptions,
    IConcertRepository concerts,
    CalendarGenerationService calendarGenerationService,
    IAmazonS3 s3Client,
    IConfiguration configuration,
    ILogger<CalendarSubscriptionGenerationCoordinator> logger)
{
    private readonly string _calendarCacheBucketName = configuration.GetValue<string>("CalendarCache:BucketName")
                                                      ?? throw new InvalidOperationException("CalendarCache:BucketName is missing!");

    /// <summary>
    /// Generates calendar files for all subscriptions that are outdated.
    /// </summary>
    public async Task GenerateOutdatedSubscriptionsAsync(CancellationToken cancellationToken)
    {
        var latestConcertChange = await concerts.GetLatestChangeAsync(cancellationToken);
        var allSubscriptions = await subscriptions.GetAllAsync(cancellationToken);

        foreach (var subscription in allSubscriptions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await GenerateIfOutdatedAsync(subscription, latestConcertChange, cancellationToken);
        }
    }

    /// <summary>
    /// Generates the calendar file of one subscription if it is outdated.
    /// </summary>
    public async Task GenerateSingleSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await subscriptions.GetByPrimaryKeyAsync(subscriptionId, cancellationToken);
        if (subscription is null)
        {
            logger.LogWarning("Skipping generation for missing subscription '{subscriptionId}'.", subscriptionId);
            return;
        }

        var latestConcertChange = await concerts.GetLatestChangeAsync(cancellationToken);
        await GenerateIfOutdatedAsync(subscription, latestConcertChange, cancellationToken);
    }

    private async Task GenerateIfOutdatedAsync(CalendarSubscriptionDo subscription, DateTimeOffset? latestConcertChange,
        CancellationToken cancellationToken)
    {
        if (!NeedsGeneration(subscription.LastGeneratedAt, latestConcertChange))
            return;

        var calendar = await calendarGenerationService.GenerateCalendarAsync(new CalendarGenerationSettingsBo
        {
            IncludeDoorsTime = subscription.IncludeDoorsTime,
            IncludeLpStageTime = subscription.IncludeLpStageTime
        }, cancellationToken);

        var serializedCalendar = new CalendarSerializer().SerializeToString(calendar);
        var key = $"subs/{subscription.Id}.ical";
        await s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _calendarCacheBucketName,
            Key = key,
            ContentBody = serializedCalendar,
            ContentType = "text/calendar"
        }, cancellationToken);

        await subscriptions.MarkGeneratedAsync(subscription.Id, DateTimeOffset.UtcNow, cancellationToken);
        logger.LogInformation("Generated and stored calendar for subscription '{subscriptionId}' as '{key}'.",
            subscription.Id, key);
    }

    private static bool NeedsGeneration(DateTimeOffset? lastGeneratedAt, DateTimeOffset? latestConcertChange)
    {
        if (lastGeneratedAt is null)
            return true;

        if (latestConcertChange is null)
            return false;

        return latestConcertChange > lastGeneratedAt;
    }
}






