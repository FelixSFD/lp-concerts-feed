using Service.Tours.Importer;
using Service.Tours.Importer.DataStructure;

namespace Service.Tours.Tests.Importer;

public class TourdataWikitextParserTest
{
    private const string SampleTourdateSource = """
        {{Tourdate
        | ShowType = concert
        | Last show = 2004.08.30
        | Artist = Linkin Park
        | Next show = 2004.09.03
        | Year = 2004
        | Month = September
        | Day = 01
        | Country = United States
        | City = Phoenix, AZ
        | Venue = Cricket Pavilion
        | VenueID = Cricket Wireless Pavilion
        | Venue Type = Amphitheatre
        | Venue Website = http://www.cricket-pavilion.com/
        | Tour = Projekt Revolution 2004
        | Stage = Main Stage
        | Other artists = Korn, Snoop Dogg, The Used, Less Than Jake
        }}

        == Setlist ==

        {{Setlist
        | Set Intro = Gacela Intro
        | Song1 = Don't Stay
        }}
        """;

    private const string SampleBerlinTourdateSource = """
        {{Tourdate
        | Last show = 2025.06.16
        | Next show = 2025.06.20
        | Artist = Linkin Park
        | ShowType = concert
        | Year = 2025
        | Month = June
        | Day = 18
        | Country = Germany
        | City = Berlin
        | Venue = Olympiastadion
        | Venue Type = Stadium
        | Tour = From Zero World Tour
        | Support = Architects
        | Support2 = grandson
        | Setlist = B6
        }}
        """;

    [Fact]
    public void ExtractTourdateSource_WhenTourdatePresent_ReturnsTourdateBlock()
    {
        var parser = new TourdataWikitextParser();
        var extracted = parser.ExtractTourdateSource(SampleTourdateSource);

        Assert.NotNull(extracted);
        Assert.StartsWith("{{Tourdate", extracted);
        Assert.EndsWith("}}", extracted);
        Assert.Contains("Cricket Pavilion", extracted);
        Assert.DoesNotContain("== Setlist ==", extracted);
    }

    [Fact]
    public void ExtractTourdateSource_WhenNoTourdatePresent_ReturnsNull()
    {
        var parser = new TourdataWikitextParser();
        var extracted = parser.ExtractTourdateSource("== Setlist ==\n{{Setlist\n| Song1 = In The End\n}}");

        Assert.Null(extracted);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExtractTourdateSource_WhenEmptyOrNull_ReturnsNull(string? source)
    {
        var parser = new TourdataWikitextParser();
        var extracted = parser.ExtractTourdateSource(source!);

        Assert.Null(extracted);
    }

    [Fact]
    public void GetEntry_ParsesSampleConcertCorrectly()
    {
        var parser = new TourdataWikitextParser();
        var entry = parser.GetTourdateInformation(SampleTourdateSource);

        Assert.NotNull(entry);
        Assert.Equal("concert", entry.ShowType);
        Assert.Equal("Linkin Park", entry.Artist);
        Assert.Equal("2004.08.30", entry.LastShow);
        Assert.Equal("2004.09.03", entry.NextShow);
        Assert.Equal((uint)2004, entry.Year);
        Assert.Equal("September", entry.Month);
        Assert.Equal((uint)1, entry.Day);
        Assert.Equal(new DateOnly(2004, 9, 1), entry.Date);
        Assert.Equal("United States", entry.Country);
        Assert.Equal("Phoenix, AZ", entry.City);
        Assert.Equal("Cricket Pavilion", entry.Venue);
        Assert.Equal("Cricket Wireless Pavilion", entry.VenueId);
        Assert.Equal("Amphitheatre", entry.VenueType);
        Assert.Equal("http://www.cricket-pavilion.com/", entry.VenueWebsite);
        Assert.Equal("Projekt Revolution 2004", entry.Tour);
        Assert.Equal("Main Stage", entry.Stage);
        Assert.Equal("Korn, Snoop Dogg, The Used, Less Than Jake", entry.OtherArtists);
        Assert.Equal(16, entry.RawProperties.Count);
    }

    [Fact]
    public void GetEntry_ParsesSupportActsAndSetlistCorrectly()
    {
        var parser = new TourdataWikitextParser();
        var entry = parser.GetTourdateInformation(SampleBerlinTourdateSource);

        Assert.NotNull(entry);
        Assert.Equal("concert", entry.ShowType);
        Assert.Equal("Linkin Park", entry.Artist);
        Assert.Equal("2025.06.16", entry.LastShow);
        Assert.Equal("2025.06.20", entry.NextShow);
        Assert.Equal((uint)2025, entry.Year);
        Assert.Equal("June", entry.Month);
        Assert.Equal((uint)18, entry.Day);
        Assert.Equal(new DateOnly(2025, 6, 18), entry.Date);
        Assert.Equal("Germany", entry.Country);
        Assert.Equal("Berlin", entry.City);
        Assert.Equal("Olympiastadion", entry.Venue);
        Assert.Equal("Stadium", entry.VenueType);
        Assert.Equal("From Zero World Tour", entry.Tour);
        Assert.Equal("Architects", entry.Support);
        Assert.Equal("grandson", entry.Support2);
        Assert.Equal(["Architects", "grandson"], entry.SupportArtists);
        Assert.Equal("B6", entry.Setlist);
    }

    [Theory]
    [InlineData("September", 9)]
    [InlineData("Sep", 9)]
    [InlineData("Sept", 9)]
    [InlineData("09", 9)]
    [InlineData("9", 9)]
    [InlineData("January", 1)]
    [InlineData("12", 12)]
    public void GetEntry_ParsesVariousMonthFormats(string monthStr, int expectedMonth)
    {
        var parser = new TourdataWikitextParser();
        var source = $"{{\n| Year = 2024\n| Month = {monthStr}\n| Day = 05\n}}";

        var entry = parser.GetTourdateInformation(source);

        Assert.NotNull(entry);
        Assert.Equal(new DateOnly(2024, expectedMonth, 5), entry.Date);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid wikitext")]
    public void GetEntry_WhenInvalidOrEmpty_ReturnsNull(string? source)
    {
        var parser = new TourdataWikitextParser();
        var entry = parser.GetTourdateInformation(source!);

        Assert.Null(entry);
    }
}
