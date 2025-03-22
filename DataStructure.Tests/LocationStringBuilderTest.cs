using JetBrains.Annotations;
using LPCalendar.DataStructure;
using Xunit;

namespace DataStructure.Tests;

[TestSubject(typeof(LocationStringBuilder))]
public class LocationStringBuilderTest
{

    [Theory]
    [InlineData("Test Stadium, Frankfurt, Hesse, Germany", "Test Stadium", "Frankfurt", "Hesse", "Germany")]
    [InlineData("Test Stadium, Frankfurt, Germany", "Test Stadium", "Frankfurt", null, "Germany")]
    [InlineData("WWK Arena, Augsburg, Germany", "WWK Arena", "Augsburg", null, "Germany")]
    [InlineData("WWK Arena, Augsburg, Germany", "WWK Arena", "Augsburg", "", "Germany")]
    [InlineData("London, United Kingdom", null, "London", null, "United Kingdom")]
    [InlineData("London, United Kingdom", "", "London", null, "United Kingdom")]
    [InlineData("Wembley Stadium", "Wembley Stadium", null, null, null)]
    [InlineData("Wembley Stadium", "Wembley Stadium", "", "", "")]
    public void GetLocationString(string expectedOutput, string? venue = null, string? city = null, string? state = null, string? country = null)
    {
        var output = LocationStringBuilder.GetLocationString(venue, city, state, country);
        
        Assert.Equal(expectedOutput, output);
    }
}