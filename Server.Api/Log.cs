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
}