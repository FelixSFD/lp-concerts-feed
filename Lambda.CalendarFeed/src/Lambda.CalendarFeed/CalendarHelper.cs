using Ical.Net.DataTypes;

namespace Lambda.CalendarFeed;

/// <summary>
/// Helper functions and extensions to work with Calendar entries
/// </summary>
public static class CalendarHelper
{
    public static CalDateTime ToCalDateTime(this DateTimeOffset dateTimeOffset, string timezoneId)
    {
        return new CalDateTime(dateTimeOffset.ConvertToTimezone(timezoneId).DateTime, timezoneId);
    }
    
    
    public static DateTimeOffset ConvertToTimezone(this DateTimeOffset dateTimeOffset, string timezoneId)
    {
        var targetTz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
        return TimeZoneInfo.ConvertTime(dateTimeOffset, targetTz);
    }
}