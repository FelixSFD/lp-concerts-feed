namespace Common.Utils.Tests;

public class DateTimeExtensionsTest
{
    [Fact]
    public void ToDateTimeOffset_FromUtcDateTime_ConvertsToCorrectOffset()
    {
        var utcDateTime = new DateTime(2026, 6, 30, 18, 0, 0, DateTimeKind.Utc);
        var berlinTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        var result = utcDateTime.ToDateTimeOffset(berlinTz);

        Assert.Equal(TimeSpan.FromHours(2), result.Offset);
        Assert.Equal(20, result.Hour);
        Assert.Equal(18, result.UtcDateTime.Hour);
    }

    [Fact]
    public void ToDateTimeOffset_ByTimeZoneId_ConvertsCorrectly()
    {
        var utcDateTime = new DateTime(2026, 1, 15, 18, 0, 0, DateTimeKind.Utc);

        var result = utcDateTime.ToDateTimeOffset("Europe/Berlin");

        // In January (standard time), Berlin is UTC+1
        Assert.Equal(TimeSpan.FromHours(1), result.Offset);
        Assert.Equal(19, result.Hour);
        Assert.Equal(18, result.UtcDateTime.Hour);
    }
    
    [Fact]
    public void ToDateTimeOffset_FromLocalDateTime_ConvertsToCorrectOffset()
    {
        var localDateTime = new DateTime(2026, 6, 30, 20, 0, 0, DateTimeKind.Local);
        var nyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
        
        var result = localDateTime.ToDateTimeOffset(nyTimeZone);

        var expectedUtcDateTime = localDateTime.ToUniversalTime();
        var expectedOffset = nyTimeZone.GetUtcOffset(localDateTime);
        var expectedHour = expectedUtcDateTime.Add(expectedOffset).Hour;

        Assert.Equal(expectedOffset, result.Offset);
        Assert.Equal(expectedHour, result.Hour);
        Assert.Equal(expectedUtcDateTime.Hour, result.UtcDateTime.Hour);
    }
}
