using Ical.Net.DataTypes;
using JetBrains.Annotations;
using Xunit;

namespace Lambda.CalendarFeed.Tests;

[TestSubject(typeof(CalendarHelper))]
public class CalendarHelperTest
{
    /// <summary>
    /// Test-data for <see cref="DateTimeOffsetConvertToTimezone"/>
    /// </summary>
    public static IEnumerable<object[]> DateTimeOffsetConvertToTimezoneData =>
        new List<object[]>
        {
            new object[] { new DateTimeOffset(2024, 9, 5, 15, 0, 0, TimeSpan.FromHours(-7)), "Europe/Berlin", new DateTimeOffset(2024, 9, 6, 0, 0, 0, TimeSpan.FromHours(2))},
            new object[] { new DateTimeOffset(2024, 9, 22, 20, 13, 37, TimeSpan.FromHours(2)), "Australia/Perth", new DateTimeOffset(2024, 9, 23, 2, 13, 37, TimeSpan.FromHours(8))},
            new object[] { new DateTimeOffset(2025, 1, 13, 12, 00, 45, TimeSpan.FromHours(1)), "Asia/Katmandu", new DateTimeOffset(2025, 1, 13, 16, 45, 45, TimeSpan.FromHours(5.75))},
        };
    
    /// <summary>
    /// Test-data for <see cref="ToCalDateTime"/>
    /// </summary>
    public static IEnumerable<object[]> ToCalDateTimeData =>
        new List<object[]>
        {
            new object[] { new DateTimeOffset(2024, 9, 5, 15, 0, 0, TimeSpan.FromHours(-7)), "Europe/Berlin", new CalDateTime(2024, 9, 6, 0, 0, 0, "Europe/Berlin")},
            new object[] { new DateTimeOffset(2024, 9, 22, 20, 13, 37, TimeSpan.FromHours(2)), "Australia/Perth", new CalDateTime(2024, 9, 23, 2, 13, 37, "Australia/Perth")},
            new object[] { new DateTimeOffset(2025, 1, 13, 12, 00, 45, TimeSpan.FromHours(1)), "Asia/Katmandu", new CalDateTime(2025, 1, 13, 16, 45, 45, "Asia/Katmandu")},
        };


    [Theory]
    [MemberData(nameof(DateTimeOffsetConvertToTimezoneData))]
    public void DateTimeOffsetConvertToTimezone(DateTimeOffset input, string targetTz, DateTimeOffset expectedOutput)
    {
        var converted = input.ConvertToTimezone(targetTz);
        Assert.Equal(expectedOutput, converted);
        Assert.Equal(expectedOutput.Offset, converted.Offset);
    }
    
    
    [Theory]
    [MemberData(nameof(ToCalDateTimeData))]
    public void ToCalDateTime(DateTimeOffset input, string targetTz, CalDateTime expectedOutput)
    {
        var converted = input.ToCalDateTime(targetTz);
        Assert.Equal(expectedOutput, converted);
        Assert.Equal(expectedOutput.TzId, converted.TzId);
        Assert.Equal(expectedOutput.TimeZoneName, converted.TimeZoneName);
    }
}