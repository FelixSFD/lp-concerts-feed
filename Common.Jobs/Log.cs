using Microsoft.Extensions.Logging;

namespace Common.Jobs;

public static partial class Log
{
    [LoggerMessage(
        EventId = 90001,
        Level = LogLevel.Information,
        Message = "Running job '{job}'..."
    )]
    public static partial void StartJob(ILogger logger, string job);
    
    [LoggerMessage(
        EventId = 90002,
        Level = LogLevel.Information,
        Message = "Finished job '{job}' in {seconds} seconds. Next execution: {nextExecution}"
    )]
    public static partial void FinishedJob(ILogger logger, string job, double seconds, DateTimeOffset? nextExecution);
    
    [LoggerMessage(
        EventId = 90003,
        Level = LogLevel.Error,
        Message = "Failed to generate profile banner for user: {username} (ID: {userId})"
    )]
    public static partial void FailedToGenerateProfileBannerFor(ILogger logger, string username, string userId, Exception exception);
    
    [LoggerMessage(
        EventId = 90004,
        Level = LogLevel.Debug,
        Message = "Fetching stats for user '{username}' (ID: {userId})..."
    )]
    public static partial void FetchingStatsFor(ILogger logger, string username, string userId);
    
    [LoggerMessage(
        EventId = 90005,
        Level = LogLevel.Debug,
        Message = "Fetched stats for user '{username}' (ID: {userId})."
    )]
    public static partial void FetchedStatsFor(ILogger logger, string username, string userId);
    
    [LoggerMessage(
        EventId = 90006,
        Level = LogLevel.Debug,
        Message = "Successfully generated profile banner for user: {username} (ID: {userId})"
    )]
    public static partial void GeneratedProfileBannerFor(ILogger logger, string username, string userId);
    
    [LoggerMessage(
        EventId = 90007,
        Level = LogLevel.Debug,
        Message = "Successfully stored profile banner for user: {username} (ID: {userId}); Size: {bytes} bytes"
    )]
    public static partial void StoredProfileBannerFor(ILogger logger, string username, string userId, int bytes);

    [LoggerMessage(
        EventId = 90008,
        Level = LogLevel.Warning,
        Message = "Skipping generation for missing subscription '{subscriptionId}'."
    )]
    public static partial void SkippingGenerationForMissingSubscription(ILogger logger, string subscriptionId);

    [LoggerMessage(
        EventId = 90009,
        Level = LogLevel.Information,
        Message = "Generated and stored calendar for subscription '{subscriptionId}' as '{key}'."
    )]
    public static partial void GeneratedAndStoredSubscriptionCalendar(ILogger logger, string subscriptionId, string key);

    [LoggerMessage(
        EventId = 90010,
        Level = LogLevel.Warning,
        Message = "Job '{job}' is missing required job data '{jobDataKey}'."
    )]
    public static partial void JobMissingRequiredData(ILogger logger, string job, string jobDataKey);
    
    [LoggerMessage(
        EventId = 90011,
        Level = LogLevel.Information,
        Message = "Forcefully generating all subscriptions regardless of their last update time..."
    )]
    public static partial void ForceGenerateAllSubscriptionCalendars(ILogger logger);
}