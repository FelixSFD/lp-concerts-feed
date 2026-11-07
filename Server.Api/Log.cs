namespace Server.Api;

internal static partial class Log
{
    [LoggerMessage(LogLevel.Debug, "Generating Apple Music developer token...")]
    public static partial void GeneratingAppleMusicDeveloperToken(ILogger logger);
    
    [LoggerMessage(LogLevel.Debug, "Generated Apple Music Developer Token: {token}")]
    public static partial void AppleMusicDeveloperTokenCreated(ILogger logger, string token);
    
    [LoggerMessage(10103, LogLevel.Debug, "User 'me' was requested. Resolved to user ID: {userId}")]
    public static partial void ResolvedParameterMeToUserId(ILogger logger, string userId);
    
    [LoggerMessage(10104, LogLevel.Warning, "User 'me' was requested, but the request was not authenticated!")]
    public static partial void ResolvedParameterMeToUserIdNotAuthenticated(ILogger logger);
    
    [LoggerMessage(10105, LogLevel.Trace, "Added output-cache tag for user: {userId}")]
    public static partial void AddedOutputCacheTagForUser(ILogger logger, string userId);
    
    [LoggerMessage(10106, LogLevel.Debug, "Evicted statistics cache for user: {userId}")]
    public static partial void EvictedStatisticsCacheForUser(ILogger logger, string userId);
    
    [LoggerMessage(10107, LogLevel.Warning, "Failed to evict statistics cache for user: {userId}")]
    public static partial void FailedToEvictStatisticsCacheForUser(ILogger logger, string userId, Exception exception);
}