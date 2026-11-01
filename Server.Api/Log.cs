namespace Server.Api;

public static partial class Log
{
    [LoggerMessage(LogLevel.Debug, "Generating Apple Music developer token...")]
    public static partial void GeneratingAppleMusicDeveloperToken(ILogger logger);
    
    [LoggerMessage(LogLevel.Debug, "Generated Apple Music Developer Token: {token}")]
    public static partial void AppleMusicDeveloperTokenCreated(ILogger logger, string token);
}