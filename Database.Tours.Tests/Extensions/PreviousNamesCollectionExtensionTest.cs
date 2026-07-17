using Database.Tours.DataObjects;
using Database.Tours.Extensions;

namespace Database.Tours.Tests.Extensions;

public class PreviousNamesCollectionExtensionTest
{
    [Theory]
    [InlineData(1, "Testing", "2024-05-31", "2024-05-31T13:37:00.000Z")]
    [InlineData(2, "Some Venue Name", "2045-12-31", "2054-08-11T10:11:00.000Z")]
    public void GetValidNameEntryAt_OnlyOneEntry(uint id, string name, string fromStr, string testTimeStr)
    {
        var testDateTime = DateTimeOffset.Parse(testTimeStr);
        var from = DateOnly.ParseExact(fromStr, "yyyy-MM-dd");
        PreviousVenueNameDo[] names = [
            new()
            {
                Id = id,
                Name = name,
                From = from
            }
        ];

        var currentName = names.GetValidNameEntryAt(testDateTime);
        Assert.Equal(name, currentName.Name);
        Assert.Equal(from, currentName.From);
        Assert.Null(currentName.To);
        Assert.Equal(id, currentName.Id);
    }
    
    [Theory]
    [InlineData(1, "Testing", "2024-05-31", "2024-05-31T13:37:00.000Z", "Testing", 1, "2024-05-31", null)]
    [InlineData(2, "Some Venue Name", "2045-12-31", "2054-08-11T10:11:00.000Z", "Some Venue Name", 2, "2045-12-31", null)]
    [InlineData(2, "Some Venue Name", "2045-12-31", "2045-12-31T00:00:00.000Z", "Some Venue Name", 2, "2045-12-31", null)]
    [InlineData(2, "Some Venue Name", "2045-12-31", "2045-12-31T00:00:00.000+01:00", "Old Venue Name", 99999, "0001-01-01", "2045-12-30")]
    public void GetValidNameEntryAt_TwoEntries(uint id, string name, string fromStr, string testTimeStr, string expectedName, uint expectedId, string expectedFromStr, string? expectedToStr)
    {
        var testDateTime = DateTimeOffset.Parse(testTimeStr);
        var from = DateOnly.ParseExact(fromStr, "yyyy-MM-dd");
        var expectedFrom = DateOnly.ParseExact(expectedFromStr, "yyyy-MM-dd");
        DateOnly? expectedTo = expectedToStr != null ? DateOnly.ParseExact(expectedToStr, "yyyy-MM-dd") : null;
        PreviousVenueNameDo[] names = [
            new()
            {
                Id = 99999,
                Name = "Old Venue Name",
                From = DateOnly.MinValue,
                To = from.AddDays(-1)
            },
            new()
            {
                Id = id,
                Name = name,
                From = from
            }
        ];

        var currentName = names.GetValidNameEntryAt(testDateTime);
        Assert.Equal(expectedName, currentName.Name);
        Assert.Equal(expectedFrom, currentName.From);
        Assert.Equal(expectedTo, currentName.To);
        Assert.Equal(expectedId, currentName.Id);
    }
}