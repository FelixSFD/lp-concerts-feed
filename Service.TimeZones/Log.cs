using Microsoft.Extensions.Logging;

namespace Service.TimeZones;

public static partial class Log
{
    [LoggerMessage(LogLevel.Debug, "Received response from TimeZoneDb API for coordinates: lat={Lat}, lon={Lon}. Response content: {responseContent}")]
    public static partial void ReceivedResponseFromTimeZoneDbApi(ILogger logger, double lat, double lon, string responseContent);
    
    [LoggerMessage(LogLevel.Error, "Time zone API call failed for coordinates: lat={Lat}, lon={Lon}. Message: {Message}")]
    public static partial void TimeZoneApiCallFailed(ILogger logger, double lat, double lon, string message);
}