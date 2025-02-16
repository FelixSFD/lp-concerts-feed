using Ical.Net.DataTypes;
using JetBrains.Annotations;
using LPCalendar.DataStructure;
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
    
    
    /// <summary>
    /// Test-data for <see cref="GetFullEventFor"/>
    /// </summary>
    public static IEnumerable<object[]> GetFullEventForData =>
        new List<object[]>
        {
            new object[]
            {
                new Concert
                {
                    Id = Guid.NewGuid().ToString(),
                    Status = "PUBLISHED",
                    Venue = "Hogwarts Quidditch Stadium",
                    City = "Hogsmeade",
                    Country = "Scotland",
                    TimeZoneId = "Europe/London",
                    LpuEarlyEntryConfirmed = false,
                    PostedStartTime = new DateTimeOffset(2024, 9, 23, 17, 0, 0, TimeSpan.FromHours(1))
                },
                "Linkin Park: Hogwarts Quidditch Stadium",
                "Linkin Park Concert at Hogwarts Quidditch Stadium\nThis show is not part of a tour.",
                "Hogwarts Quidditch Stadium, Hogsmeade, Scotland",
                new CalDateTime(2024, 9, 23, 17, 0, 0, "Europe/London"),
                TimeSpan.FromHours(3)
            },
            new object[]
            {
                new Concert
                {
                    Id = Guid.NewGuid().ToString(),
                    Status = "PUBLISHED",
                    Venue = "Barclays Arena",
                    City = "Hamburg",
                    Country = "Germany",
                    TimeZoneId = "Europe/Berlin",
                    LpuEarlyEntryConfirmed = false,
                    PostedStartTime = new DateTimeOffset(2024, 9, 22, 18, 0, 0, TimeSpan.FromHours(2)),
                    DoorsTime = new DateTimeOffset(2024, 9, 22, 18, 30, 0, TimeSpan.FromHours(2)),
                    MainStageTime= new DateTimeOffset(2024, 9, 22, 20, 40, 0, TimeSpan.FromHours(2))
                },
                "Linkin Park: Barclays Arena",
                "Linkin Park Concert at Barclays Arena\nThis show is not part of a tour.",
                "Barclays Arena, Hamburg, Germany",
                new CalDateTime(2024, 9, 22, 20, 40, 0, "Europe/Berlin"),
                TimeSpan.FromHours(2)
            },
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


    [Theory]
    [MemberData(nameof(GetFullEventForData))]
    public void GetFullEventFor(Concert concert, string expectedTitle, string expectedDescription, string expectedLocation, CalDateTime expectedStart, TimeSpan expectedDuration)
    {
        var fullEvent = CalendarHelper.GetFullEventFor(concert);

        Assert.NotNull(fullEvent);
        Assert.Equal(expectedTitle, fullEvent.Summary);
        Assert.Equal(expectedDescription, fullEvent.Description);
        Assert.Equal(expectedLocation, fullEvent.Location);
        Assert.Equal(expectedDuration, fullEvent.Duration);
        Assert.Equal(expectedStart, fullEvent.Start);
    }
}