using Amazon.Lambda.TestUtilities;
using Common.WikiMedia.DTOs;
using Common.WikiMedia.Repositories;
using LPCalendar.DataStructure.Setlists.Import;
using NSubstitute;
using Service.Setlists.Importer;
using Service.Setlists.Importer.DataStructure;

namespace Service.Setlists.Tests;


public class LinkinpediaImportServiceTest
{
    private readonly IWikiMediaRepository _wikiMediaRepository;
    private readonly IWikitextParser _wikitextParser;
    private readonly LinkinpediaImportService _sut;

    public LinkinpediaImportServiceTest()
    {
        _wikiMediaRepository = Substitute.For<IWikiMediaRepository>();
        _wikitextParser = Substitute.For<IWikitextParser>();
        
        _sut = new LinkinpediaImportService(_wikiMediaRepository, _wikitextParser, new TestLambdaLogger());
    }
    
    [Fact]
    public async Task GetImportPlanForSetlistFromPageAsync()
    {
        var pageId = "Test_Page";
        
        var expectedResult = new ImportSetlistPreviewDto
        {
            ConcertId = "1234",
            Entries = [
                new ImportSetlistEntryPreviewDto
                {
                    Title = "Song 1",
                    ExtraNotes = "some notes"
                },
                new ImportSetlistEntryPreviewDto
                {
                    Title = "Song 2",
                    ExtraNotes = null
                },
                new ImportSetlistEntryPreviewDto
                {
                    Title = "yet another song",
                    ExtraNotes = null
                },
            ]
        };
        
        // prepare mocks
        var mockWikiPage = new WikiPageDto
        {
            Id = 123,
            Title = "Sample Page",
            Source = "mock source"
        };

        WikiSetlistEntry[] mockWikiSetlistEntries = [
            new SongWikiSetlistEntry
            {
                Name = "Song 1",
                Note = "some notes"
            },
            new SongWikiSetlistEntry
            {
                Name = "Song 2",
                Note = null
            },
            new SongWikiSetlistEntry
            {
                Name = "yet another song",
                Note = null
            },
        ];
        
        _wikiMediaRepository
            .GetWikiPageAsync(pageId)
            .Returns(mockWikiPage);

        _wikitextParser
            .ExtractSetlistSource(mockWikiPage.Source)
            .Returns("mock setlist");
        
        _wikitextParser
            .GetEntries("mock setlist")
            .Returns(mockWikiSetlistEntries);
        
        // run the test
        var result = await _sut.GetImportPlanForSetlistFromPageAsync(pageId);
        Assert.NotNull(result);
        
        // validate method calls
        await _wikiMediaRepository
            .Received(1)
            .GetWikiPageAsync(pageId);

        AssertEqual(expectedResult, result);
    }


    private void AssertEqual(ImportSetlistPreviewDto expectedResult,
        ImportSetlistPreviewDto actualResult)
    {
        //Assert.Equal(expectedResult.ConcertId, actualResult.ConcertId);
        
        Assert.Equal(expectedResult.Entries.Count, actualResult.Entries.Count);
        for (var i = 0; i < expectedResult.Entries.Count; i++)
        {
            var expectedEntry = expectedResult.Entries[i];
            var actualEntry = actualResult.Entries[i];
            AssertEqual(expectedEntry, actualEntry);
        }
    }
    
    private void AssertEqual(ImportSetlistEntryPreviewDto expectedEntry,
        ImportSetlistEntryPreviewDto actualEntry)
    {
        Assert.Equal(expectedEntry.Title, actualEntry.Title);
        Assert.Equal(expectedEntry.ExtraNotes, actualEntry.ExtraNotes);
        Assert.Equal(expectedEntry.FoundSongId, actualEntry.FoundSongId);
        Assert.Equal(expectedEntry.FoundSongVariantId, actualEntry.FoundSongVariantId);
        Assert.Equal(expectedEntry.FoundMashupId, actualEntry.FoundMashupId);
    }
}