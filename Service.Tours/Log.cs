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
    
    [LoggerMessage(10010, LogLevel.Debug, "Start syncing venues with last sync date: {lastSync}")]
    public static partial void StartVenueSync(ILogger logger, DateTimeOffset? lastSync);
    
    [LoggerMessage(10011, LogLevel.Information, "Finished syncing venues with last sync date: {lastSync}; Updated: {updated}; Deleted: {deleted}; Duration: {duration}")]
    public static partial void FinishedVenueSync(ILogger logger, DateTimeOffset? lastSync, int updated, int deleted, TimeSpan duration);
    
    [LoggerMessage(10012, LogLevel.Information, "Finished syncing venues with last sync date: {lastSync}; Updated: {updated}; Deleted: {deleted}; Duration: not measured")]
    public static partial void FinishedVenueSync(ILogger logger, DateTimeOffset? lastSync, int updated, int deleted);

    [LoggerMessage(10020, LogLevel.Debug, "Start syncing cities with last sync date: {lastSync}")]
    public static partial void StartCitySync(ILogger logger, DateTimeOffset? lastSync);
    
    [LoggerMessage(10021, LogLevel.Information, "Finished syncing cities with last sync date: {lastSync}; Updated: {updated}; Deleted: {deleted}; Duration: {duration}")]
    public static partial void FinishedCitySync(ILogger logger, DateTimeOffset? lastSync, int updated, int deleted, TimeSpan duration);
    
    [LoggerMessage(10022, LogLevel.Information, "Finished syncing cities with last sync date: {lastSync}; Updated: {updated}; Deleted: {deleted}; Duration: not measured")]
    public static partial void FinishedCitySync(ILogger logger, DateTimeOffset? lastSync, int updated, int deleted);

    [LoggerMessage(10030, LogLevel.Debug, "Start syncing countries with last sync date: {lastSync}")]
    public static partial void StartCountrySync(ILogger logger, DateTimeOffset? lastSync);
    
    [LoggerMessage(10031, LogLevel.Information, "Finished syncing countries with last sync date: {lastSync}; Updated: {updated}; Deleted: {deleted}; Duration: {duration}")]
    public static partial void FinishedCountrySync(ILogger logger, DateTimeOffset? lastSync, int updated, int deleted, TimeSpan duration);
    
    [LoggerMessage(10032, LogLevel.Information, "Finished syncing countries with last sync date: {lastSync}; Updated: {updated}; Deleted: {deleted}; Duration: not measured")]
    public static partial void FinishedCountrySync(ILogger logger, DateTimeOffset? lastSync, int updated, int deleted);
    
    [LoggerMessage(10040, LogLevel.Debug, "Generating AttendedConcerts stats for user: {userId}")]
    public static partial void StartGeneratingAttendedConcertsStats(ILogger logger, string userId);
    
    [LoggerMessage(10041, LogLevel.Debug, "Generated AttendedConcerts stats for user: {userId}; Concerts: {numberOfConcerts}; Venues: {numberOfVenues}; Cities: {numberOfCities}; Countries: {numberOfCountries}")]
    public static partial void FinishedGeneratingAttendedConcertsStats(ILogger logger, string userId, int numberOfConcerts, int numberOfVenues, int numberOfCities, int numberOfCountries);
    
    [LoggerMessage(10042, LogLevel.Debug, "Loaded concerts the user '{userId}' has attended so far")]
    public static partial void LoadedConcertsUserAttended(ILogger logger, string userId);
}