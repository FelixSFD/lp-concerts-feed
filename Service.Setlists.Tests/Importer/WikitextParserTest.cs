using Service.Setlists.Importer;
using Service.Setlists.Importer.DataStructure;

namespace Service.Setlists.Tests.Importer;

public class WikitextParserTest
{
    [Fact]
    public async Task GetEntriesAsync()
    {
        var setlistSource = await File.ReadAllTextAsync("TestData/Wiki/setlist_source_Live_20240905.txt");

        WikiSetlistEntry[] expectedEntries = [
            new ActWikiSetlistEntry
            {
                Name = "Inception Intro A",
                ActNumber = 1,
                Note = "w/ \"[[Castle Of Glass]]\" Vocals"
            },
            new ActWikiSetlistEntry
            {
                Name = "Creation Intro A",
                ActNumber = 2,
                Note = "w/ \"Castle Of Glass\" Vocals"
            },
            new ActWikiSetlistEntry
            {
                Name = "Resolution Intro AA",
                ActNumber = 5,
                Note = "w/ \"Castle Of Glass\" Vocals"
            }
        ];
        
        var parser = new WikitextParser();
        var entries = await parser.GetEntriesAsync(setlistSource);
        AssertSameContent(expectedEntries, entries);
    }

    private void AssertSameContent(WikiSetlistEntry[] expected, WikiSetlistEntry[] actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            AssertSameContent(expected[i], actual[i]);
        }
    }

    private void AssertSameContent(WikiSetlistEntry expected, WikiSetlistEntry actual)
    {
        var expectedType = expected.GetType();
        Assert.IsType(expectedType, actual);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Note, actual.Note);

        if (expectedType == typeof(ActWikiSetlistEntry))
        {
            Assert.Equal(((ActWikiSetlistEntry)expected).ActNumber, ((ActWikiSetlistEntry)actual).ActNumber);
        }
        else if (expectedType == typeof(SongWikiSetlistEntry))
        {
            Assert.Equal(((SongWikiSetlistEntry)expected).SongNumber, ((SongWikiSetlistEntry)actual).SongNumber);
        }
        else
        {
            Assert.Fail($"Unexpected type: {expectedType}");
        }
    }
    
    [Fact]
    public async Task ExtractSetlistSource()
    {
        var pageSource = await File.ReadAllTextAsync("TestData/Wiki/page_source_Live_20240905.txt");
        var expectedSetlistSource = await File.ReadAllTextAsync("TestData/Wiki/setlist_source_Live_20240905.txt");
        var parser = new WikitextParser();

        var setlistSource = await parser.ExtractSetlistSource(pageSource);
        Assert.NotNull(setlistSource);
        Assert.Equal(expectedSetlistSource, setlistSource);
    }
}