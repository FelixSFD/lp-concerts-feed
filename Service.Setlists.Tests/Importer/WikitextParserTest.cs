using Service.Setlists.Importer;

namespace Service.Setlists.Tests.Importer;

public class WikitextParserTest
{
    [Fact]
    public async Task GetEntriesAsync()
    {
        var pageSource = await File.ReadAllTextAsync("TestData/Wiki/page_source_Live_20240905.txt");
        var parser = new WikitextParser();

        await parser.GetEntriesAsync(pageSource);
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