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
    private readonly string _calendarCacheBucketName = configuration.GetValue<string>("CalendarSubscriptions:BucketName")
                                                      ?? throw new InvalidOperationException("CalendarSubscriptions:BucketName is missing!");

    /// <summary>
    /// Generates calendar files for all subscriptions that are outdated.
    /// </summary>
    public async Task GenerateOutdatedSubscriptionsAsync(CancellationToken cancellationToken) 
        => await GenerateOutdatedSubscriptionsAsync(false, cancellationToken);

    /// <summary>
    /// Generates calendar files for all subscriptions that are outdated.
    /// </summary>
    /// <param name="forceAll">If true, all subscriptions will be regenerated regardless of their last update time.</param>
    /// <param name="cancellationToken">Token used to cancel retrieval and generation.</param>
    public async Task GenerateOutdatedSubscriptionsAsync(bool forceAll, CancellationToken cancellationToken)
    {
        DateTimeOffset? latestConcertChange;
        if (forceAll)
        {
            latestConcertChange = DateTimeOffset.UtcNow.AddDays(1); // Force all subscriptions to be regenerated
        }
        else
        {
            latestConcertChange = await concerts.GetLatestChangeAsync(cancellationToken);
        }

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
            Log.SkippingGenerationForMissingSubscription(logger, subscriptionId);
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
        var key = $"{subscription.Id}/subscription.ical";
        await s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _calendarCacheBucketName,
            Key = key,
            ContentBody = serializedCalendar,
            ContentType = "text/calendar",
            Headers =
            {
                CacheControl = "public, max-age=14400",
                ContentDisposition = "inline; filename=LinkinPark_concerts.ical"
            },
        }, cancellationToken);

        await subscriptions.MarkGeneratedAsync(subscription.Id, DateTimeOffset.UtcNow, cancellationToken);
        Log.GeneratedAndStoredSubscriptionCalendar(logger, subscription.Id, key);
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






