using Service.Setlists.Importer;
using Service.Setlists.Importer.DataStructure;

namespace Service.Setlists.Tests.Importer;

public class WikitextParserTest
{
    [Fact]
    public async Task GetEntries()
    {
        var setlistSource = await File.ReadAllTextAsync("TestData/Wiki/setlist_source_Live_20240905_short.txt");

        WikiSetlistEntry[] expectedEntries = [
            new ActWikiSetlistEntry
            {
                Name = "Inception Intro A",
                ActNumber = 1,
                Note = "w/ \"[[Castle Of Glass]]\" Vocals"
            },
            new SongWikiSetlistEntry
            {
                Name = "The Emptiness Machine",
                Note = "Live Debut",
                SongNumber = 1,
                ActNumber = 1,
            },
            new ActWikiSetlistEntry
            {
                Name = "Creation Intro A",
                ActNumber = 2,
                Note = "w/ \"Castle Of Glass\" Vocals"
            },
            new SongWikiSetlistEntry
            {
                Name = "The Catalyst",
                Note = "Shortened (No Third Chorus/Breakdown); First Time w/ Emily, Colin and Alex",
                SongNumber = 5,
                ActNumber = 2,
            },
            new SongWikiSetlistEntry
            {
                Name = "Waiting For The End",
                Note = "2024 Intro; First Time w/ Emily, Colin and Alex",
                SongNumber = 6,
                ActNumber = 2,
            },
            new SongWikiSetlistEntry
            {
                Name = "Numb",
                Note = "'Numb/Encore' Intro; First Time w/ Emily, Colin and Alex",
                SongNumber = 7,
                ActNumber = 2,
            },
            new ActWikiSetlistEntry
            {
                Name = "Resolution Intro A",
                ActNumber = 5,
                Note = "w/ \"Castle Of Glass\" Vocals"
            },
            new SongWikiSetlistEntry
            {
                Name = "Bleed It Out",
                Note = "Ext. Bridge w/ \"[[A Place For My Head]]\" Verse 1; Ext. Outro; First Time w/ Emily, Colin and Alex",
                SongNumber = 14,
                ActNumber = 5,
            },
        ];
        
        var parser = new WikitextParser();
        var entries = parser.GetEntries(setlistSource);
        AssertSameContent(expectedEntries, entries);
    }
    
    [Fact]
    public async Task GetEntries_WithIntroAndOutro()
    {
        var setlistSource = await File.ReadAllTextAsync("TestData/Wiki/setlist_source_Live_20260315_short.txt");

        WikiSetlistEntry[] expectedEntries = [
            new SongWikiSetlistEntry
            {
                Name = "It's Goin' Down",
                Note = "Pre-show w/ From Zero countdown; The X-Ecutioners (feat. Mike and Joe) song",
                SongNumber = 0,
                IsPlayedFromRecording = true,
            },
            new ActWikiSetlistEntry
            {
                Name = "Inception Intro A",
                ActNumber = 1,
                Note = "w/ \\\"[[Castle Of Glass]]\\\" Vocals"
            },
            new SongWikiSetlistEntry
            {
                Name = "Somewhere I Belong",
                Note = "Short Intro w/ Scratch",
                SongNumber = 1,
                ActNumber = 1,
            },
            new SongWikiSetlistEntry
            {
                Name = "Lying From You",
                SongNumber = 2,
                ActNumber = 1,
            },
            new SongWikiSetlistEntry
            {
                Name = "I Will Always Love You",
                Note = "Whitney Houston song; Emily sang during Joe's Happy Birthday moment",
                SongNumber = 15,
                ActNumber = 1,
            },
            new SongWikiSetlistEntry
            {
                Name = "Lost",
                Note = "Stripped (Intro/1st Verse/Chorus) into Full Band; Shortened Bridge",
                SongNumber = 17,
                ActNumber = 1,
            },
            new ActWikiSetlistEntry
            {
                Name = "Kintsugi Transition",
                ActNumber = 0,
            },
            new SongWikiSetlistEntry
            {
                Name = "Overflow",
                Note = "Ext. Synth Intro w/ [[Link:\\\"Pushing Me Away\\\"|\\\"Pushing Me Away\\\"]]",
                SongNumber = 20,
                ActNumber = 0,
            },
        ];
        
        var parser = new WikitextParser();
        var entries = parser.GetEntries(setlistSource);
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
        else if (expected is SongWikiSetlistEntry expectedSongEntry)
        {
            Assert.Equal(expectedSongEntry.SongNumber, ((SongWikiSetlistEntry)actual).SongNumber);
            Assert.Equal(expectedSongEntry.ActNumber, ((SongWikiSetlistEntry)actual).ActNumber);
            Assert.Equal(expectedSongEntry.IsPlayedFromRecording, ((SongWikiSetlistEntry)actual).IsPlayedFromRecording);
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

        var setlistSource = parser.ExtractSetlistSource(pageSource);
        Assert.NotNull(setlistSource);
        Assert.Equal(expectedSetlistSource, setlistSource);
    }
    
    [Fact]
    public async Task ExtractSetlistSource_PageHasRehearsals()
    {
        var pageSource = await File.ReadAllTextAsync("TestData/Wiki/page_source_Live_20250618.txt");
        var expectedSetlistSource = await File.ReadAllTextAsync("TestData/Wiki/setlist_source_Live_20250618.txt");
        var parser = new WikitextParser();

        var setlistSource = parser.ExtractSetlistSource(pageSource);
        Assert.NotNull(setlistSource);
        Assert.Equal(expectedSetlistSource, setlistSource);
    }
}