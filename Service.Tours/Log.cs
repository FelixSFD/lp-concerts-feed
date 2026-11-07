using Microsoft.Extensions.Logging;

namespace Service.Tours;

public static partial class Log
{
    [LoggerMessage(10000, LogLevel.Debug, "Start syncing concerts with last sync date: {lastSync}")]
    public static partial void StartConcertSync(ILogger logger, DateTimeOffset? lastSync);
    
    [LoggerMessage(10001, LogLevel.Information, "Finished syncing concerts with last sync date: {lastSync}; Updated: {updated}; Deleted: {deleted}; Duration: {duration}")]
    public static partial void FinishedConcertSync(ILogger logger, DateTimeOffset? lastSync, int updated, int deleted, TimeSpan duration);
    
    [LoggerMessage(10002, LogLevel.Information, "Finished syncing concerts with last sync date: {lastSync}; Updated: {updated}; Deleted: {deleted}; Duration: not measured")]
    public static partial void FinishedConcertSync(ILogger logger, DateTimeOffset? lastSync, int updated, int deleted);
}