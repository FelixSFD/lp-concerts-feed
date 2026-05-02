using Amazon.Lambda.TestUtilities;
using Common.WikiMedia.DTOs;
using Common.WikiMedia.Repositories;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using LPCalendar.DataStructure.Setlists.Import;
using NSubstitute;
using Service.Setlists.Importer;
using Service.Setlists.Importer.DataStructure;

namespace Service.Setlists.Tests;


public class LinkinpediaImportServiceTest
{
    private readonly IWikiMediaRepository _wikiMediaRepository;
    private readonly IWikitextParser _wikitextParser;
    private readonly ISongRepository _songRepository;
    private readonly ISongVariantRepository _songVariantRepository;
    private readonly ISongMashupRepository _songMashupRepository;
    private readonly LinkinpediaImportService _sut;

    public LinkinpediaImportServiceTest()
    {
        _wikiMediaRepository = Substitute.For<IWikiMediaRepository>();
        _wikitextParser = Substitute.For<IWikitextParser>();
        _songRepository = Substitute.For<ISongRepository>();
        _songVariantRepository = Substitute.For<ISongVariantRepository>();
        _songMashupRepository = Substitute.For<ISongMashupRepository>();
        
        _sut = new LinkinpediaImportService(_wikiMediaRepository, _wikitextParser, _songRepository, _songVariantRepository, _songMashupRepository, new TestLambdaLogger());
    }
    
    [Fact]
    public async Task GetImportPlanForSetlistFromPageAsync()
    {
        var pageId = "Test_Page";
        
        var song1 = new SongDo
        {
            Id = 1,
            Title = "Song 1",
            Isrc = "1234"
        };
        
        var song3 = new SongDo
        {
            Id = 3,
            Title = "yet another song",
            Isrc = "33331"
        };
        var song3SameName = new SongDo
        {
            Id = 4,
            Title = "yet another song",
            Isrc = "33332"
        };
        
        var song4 = new SongDo
        {
            Id = 5,
            Title = "Song 4",
            Isrc = "44"
        };

        var song4Variant1 = new SongVariantDo
        {
            Id = 123,
            VariantName = "Piano Version",
            SongId = song4.Id
        };
        var song4Variant2 = new SongVariantDo
        {
            Id = 432,
            VariantName = "Nu-Metal Version",
            SongId = song4.Id
        };

        List<SongDo> mockSongsSong1 = [song1];
        List<SongDo> mockSongsSong3 = [song3, song3SameName];
        List<SongDo> mockSongsSong4 = [song4];
        List<SongVariantDo> mockVariantsSong4 = [song4Variant1, song4Variant2];
        List<SongVariantDo> emptyVariants = [];
        
        var expectedResult = new ImportSetlistPreviewDto
        {
            ConcertId = "1234",
            Acts = [
                new ImportSetlistActPreviewDto
                {
                    Name = "Inception Intro A",
                    ActNumber = 1,
                    ExtraNotes = "COG vocals",
                },
                new ImportSetlistActPreviewDto
                {
                    Name = "Resolution Intro A",
                    ActNumber = 2,
                    ExtraNotes = "COG vocals 2",
                },
            ],
            Entries = [
                new ImportSetlistEntryPreviewDto
                {
                    SongNumber = 1,
                    ActNumber = 1,
                    Title = "Song 1",
                    ExtraNotes = "some notes",
                    FoundSongId = song1.Id,
                },
                new ImportSetlistEntryPreviewDto
                {
                    SongNumber = 2,
                    ActNumber = 1,
                    Title = "Song 2",
                    ExtraNotes = null,
                },
                new ImportSetlistEntryPreviewDto
                {
                    SongNumber = 3,
                    ActNumber = 2,
                    Title = "yet another song",
                    ExtraNotes = null,
                    FoundSongIds = mockSongsSong3.Select(s => s.Id).ToArray()
                },
                new ImportSetlistEntryPreviewDto
                {
                    SongNumber = 4,
                    ActNumber = 2,
                    Title = "Song 4",
                    FoundSongId = song4.Id,
                    PossibleSongVariants = [
                        new SongVariantDto
                        {
                            Id = 123,
                            VariantName = "Piano Version",
                            SongId = song4.Id
                        },
                        new SongVariantDto
                        {
                            Id = 432,
                            VariantName = "Nu-Metal Version",
                            SongId = song4.Id
                        }
                    ]
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
            new ActWikiSetlistEntry
            {
                Name = "Inception Intro A",
                ActNumber = 1,
                Note = "COG vocals",
            },
            new SongWikiSetlistEntry
            {
                SongNumber = 1,
                ActNumber = 1,
                Name = "Song 1",
                Note = "some notes"
            },
            new SongWikiSetlistEntry
            {
                SongNumber = 2,
                ActNumber = 1,
                Name = "Song 2",
                Note = null
            },
            new ActWikiSetlistEntry
            {
                Name = "Resolution Intro A",
                ActNumber = 2,
                Note = "COG vocals 2",
            },
            new SongWikiSetlistEntry
            {
                SongNumber = 3,
                ActNumber = 2,
                Name = "yet another song",
                Note = null
            },
            new SongWikiSetlistEntry
            {
                SongNumber = 4,
                ActNumber = 2,
                Name = "Song 4",
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

        _songRepository
            .GetSongsByTitle(song1.Title)
            .Returns(mockSongsSong1.ToAsyncEnumerable());
        
        _songRepository
            .GetSongsByTitle("Song 2")
            .Returns(AsyncEnumerable.Empty<SongDo>());
        
        _songRepository
            .GetSongsByTitle(song3.Title)
            .Returns(mockSongsSong3.ToAsyncEnumerable());
        
        _songRepository
            .GetSongsByTitle(song4.Title)
            .Returns(mockSongsSong4.ToAsyncEnumerable());
        
        // first setup with the fallback, after that, override for those args where we want to return data
        _songVariantRepository.GetVariantsOfSongAsync(Arg.Any<uint>())
            .Returns(emptyVariants);
        _songVariantRepository.GetVariantsOfSongAsync(song4.Id)
            .Returns(mockVariantsSong4);
        
        // run the test
        var result = await _sut.GetImportPlanForSetlistFromPageAsync(pageId);
        Assert.NotNull(result);
        
        AssertEqual(expectedResult, result);
        
        // validate method calls
        await _wikiMediaRepository
            .Received(1)
            .GetWikiPageAsync(pageId);
        
        _songRepository
            .Received(1)
            .GetSongsByTitle(song1.Title);

        _songRepository
            .Received(1)
            .GetSongsByTitle("Song 2");

        _songRepository
            .Received(1)
            .GetSongsByTitle(song3.Title);
        
        _songRepository
            .Received(1)
            .GetSongsByTitle(song4.Title);
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
        
        Assert.Equal(expectedResult.Acts.Count, actualResult.Acts.Count);
        for (var i = 0; i < expectedResult.Acts.Count; i++)
        {
            var expectedAct = expectedResult.Acts[i];
            var actualAct = actualResult.Acts[i];
            AssertEqual(expectedAct, actualAct);
        }
    }
    
    private void AssertEqual(ImportSetlistActPreviewDto expectedAct,
        ImportSetlistActPreviewDto actualAct)
    {
        Assert.Equal(expectedAct.Name, actualAct.Name);
        Assert.Equal(expectedAct.ActNumber, actualAct.ActNumber);
        Assert.Equal(expectedAct.ExtraNotes, actualAct.ExtraNotes);
    }
    
    private void AssertEqual(ImportSetlistEntryPreviewDto expectedEntry,
        ImportSetlistEntryPreviewDto actualEntry)
    {
        Assert.Equal(expectedEntry.Title, actualEntry.Title);
        Assert.Equal(expectedEntry.SongNumber, actualEntry.SongNumber);
        Assert.Equal(expectedEntry.ActNumber, actualEntry.ActNumber);
        Assert.Equal(expectedEntry.ExtraNotes, actualEntry.ExtraNotes);
        Assert.Equal(expectedEntry.FoundSongId, actualEntry.FoundSongId);
        Assert.Equal(expectedEntry.FoundSongIds, actualEntry.FoundSongIds);
        Assert.Equal(expectedEntry.FoundSongVariantId, actualEntry.FoundSongVariantId);
        Assert.Equal(expectedEntry.FoundMashupId, actualEntry.FoundMashupId);

        if (expectedEntry.FoundSongId != null && expectedEntry.FoundSongId > 0)
        {
            AssertEqual(expectedEntry.PossibleSongVariants, actualEntry.PossibleSongVariants);
        }
    }
    
    
    private static void AssertEqual(SongVariantDto[] expectedVariants, SongVariantDto[] actualVariants)
    {
        Assert.Equal(expectedVariants.Length, actualVariants.Length);
        for (var i = 0; i < expectedVariants.Length; i++)
        {
            AssertEqual(expectedVariants[i], actualVariants[i]);
        }
    }


    private static void AssertEqual(SongVariantDto expectedVariant, SongVariantDto actualVariant)
    {
        Assert.Equal(expectedVariant.Id, actualVariant.Id);
        Assert.Equal(expectedVariant.SongId, actualVariant.SongId);
        Assert.Equal(expectedVariant.VariantName, actualVariant.VariantName);
        Assert.Equal(expectedVariant.Description, actualVariant.Description);
        Assert.Equal(expectedVariant.AppleMusicIdOverride, actualVariant.AppleMusicIdOverride);
        Assert.Equal(expectedVariant.IsrcOverride, actualVariant.IsrcOverride);
    }
}