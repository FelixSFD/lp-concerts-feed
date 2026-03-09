using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Database.Concerts;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Lambda.SetlistsWrite.Services;
using LPCalendar.DataStructure.Setlists;
using LPCalendar.DataStructure.Setlists.Parameters;
using NSubstitute;
using Xunit;

namespace Lambda.SetlistsWrite.Tests.Services;

public class SetlistServiceTest
{
    private readonly ISetlistRepository _setlistRepository;
    private readonly ISetlistEntryRepository _setlistEntryRepository;
    private readonly IConcertRepository _concertRepository;
    private readonly ISongRepository _songRepository;
    private readonly ISongVariantRepository _songVariantRepository;
    private readonly ISetlistActRepository _actRepository;
    private readonly ILambdaLogger _logger = new TestLambdaLogger();
    private readonly SetlistService _setlistService;

    public SetlistServiceTest()
    {
        _setlistRepository = Substitute.For<ISetlistRepository>();
        _setlistEntryRepository = Substitute.For<ISetlistEntryRepository>();
        _concertRepository = Substitute.For<IConcertRepository>();
        _songRepository = Substitute.For<ISongRepository>();
        _songVariantRepository = Substitute.For<ISongVariantRepository>();
        _actRepository = Substitute.For<ISetlistActRepository>();
        
        _setlistService = new SetlistService(_setlistRepository, _setlistEntryRepository, _concertRepository, _songRepository, _songVariantRepository, _actRepository, _logger);
    }

    [Theory]
    [InlineData("testconcert123", "https://lplive.net")]
    [InlineData("1234567890", null)]
    public async Task CreateSetlistAsync(string concertId, string? linkinpediaUrl)
    {
        // setup mock
        _setlistRepository.Add(Arg.Is<SetlistDo>(setlist => setlist.ConcertId == concertId && setlist.LinkinpediaUrl == linkinpediaUrl && setlist.Id == 0));
        
        // call the service
        var createRequest = new CreateSetlistRequestDto
        {
            ConcertId = concertId,
            LinkinpediaUrl = linkinpediaUrl
        };
        var response = await _setlistService.CreateSetlistAsync(createRequest);
        
        // validate if repo was called correctly
        _setlistRepository
            .Received(1)
            .Add(Arg.Is<SetlistDo>(setlist => setlist.ConcertId == concertId && setlist.LinkinpediaUrl == linkinpediaUrl && setlist.Id == 0));
        
        // validate result
        Assert.NotNull(response);
        Assert.Equal(createRequest.ConcertId, response.ConcertId);
        Assert.Equal(createRequest.LinkinpediaUrl, response.LinkinpediaUrl);
    }


    [Fact]
    public async Task AddSongToSetlistAsync_NewSong()
    {
        // prepare mocks and test data
        var setlist1 = new SetlistDo
        {
            Id = 1,
            ConcertId = Guid.NewGuid().ToString(),
            LinkinpediaUrl = "https://lplive.net"
        };

        var request = new AddSongToSetlistRequestDto
        {
            Act = null,
            EntryParameters = new SetlistEntryParametersDto
            {
                SongNumber = 1,
                SortNumber = 10,
                TitleOverride = null,
                ExtraNotes = "something special",
                IsPlayedFromRecording = false,
                IsWorldPremiere = true,
                IsRotationSong = false
            },
            SongParameters = new SongParametersDto
            {
                SongTitle = "New Song",
                Isrc = "new1234"
            }
        };
        
        _setlistRepository
            .GetByPrimaryKeyAsync(setlist1.Id)
            .Returns(setlist1);
        
        _songRepository
            .Add(Arg.Is<SongDo>(song => song.Title == request.SongParameters.SongTitle && song.Isrc == request.SongParameters.Isrc && song.Id == 0));
        
        _setlistEntryRepository.Add(Arg.Is<SetlistEntryDo>(entry => entry.SetlistId == setlist1.Id && entry.SongNumber == request.EntryParameters.SongNumber));
        
        // call the service
        await _setlistService.AddSongToSetlistAsync(request, setlist1.Id);
        
        // verify mock calls
        await _setlistRepository
            .Received(1)
            .GetByPrimaryKeyAsync(setlist1.Id);
        
        _setlistEntryRepository
            .Received(1)
            .Add(Arg.Is<SetlistEntryDo>(entry => entry.SetlistId == setlist1.Id && entry.SongNumber == request.EntryParameters.SongNumber));
        
        // as this is a new song without an ID, there should be no call to the song repo
        await _songRepository.DidNotReceive().GetByPrimaryKeyAsync(Arg.Any<uint>());

        await _setlistEntryRepository.Received(1).SaveChangesAsync();
    }
    
    [Fact]
    public async Task AddSongToSetlistAsync_ExistingSong()
    {
        // prepare mocks and test data
        var setlist1 = new SetlistDo
        {
            Id = 1,
            ConcertId = Guid.NewGuid().ToString(),
            LinkinpediaUrl = "https://lplive.net"
        };
        
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };

        var request = new AddSongToSetlistRequestDto
        {
            Act = null,
            EntryParameters = new SetlistEntryParametersDto
            {
                SongNumber = 1,
                SortNumber = 10,
                TitleOverride = null,
                ExtraNotes = "something special",
                IsPlayedFromRecording = false,
                IsWorldPremiere = true,
                IsRotationSong = false
            },
            SongParameters = new SongParametersDto
            {
                SongId = song1.Id
            }
        };
        
        _setlistRepository
            .GetByPrimaryKeyAsync(setlist1.Id)
            .Returns(setlist1);
        
        _songRepository
            .GetByPrimaryKeyAsync(song1.Id)
            .Returns(song1);
        
        _setlistEntryRepository.Add(Arg.Is<SetlistEntryDo>(entry => entry.SetlistId == setlist1.Id && entry.SongNumber == request.EntryParameters.SongNumber));
        
        // call the service
        await _setlistService.AddSongToSetlistAsync(request, setlist1.Id);
        
        // verify mock calls
        await _setlistRepository
            .Received(1)
            .GetByPrimaryKeyAsync(setlist1.Id);
        
        await _songRepository
            .Received(1)
            .GetByPrimaryKeyAsync(song1.Id);
        
        _setlistEntryRepository
            .Received(1)
            .Add(Arg.Is<SetlistEntryDo>(entry => entry.SetlistId == setlist1.Id && entry.SongNumber == request.EntryParameters.SongNumber));

        await _setlistEntryRepository.Received(1).SaveChangesAsync();
        _songRepository
            .DidNotReceive()
            .Add(Arg.Any<SongDo>());
    }


    [Fact]
    public async Task GetCompleteSetlistAsync()
    {
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        var song2 = new SongDo
        {
            Id = 1,
            Title = "Lost",
            Isrc = "1",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/Lost"
        };

        var entry1 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            SongNumber = 1,
            SortNumber = 10,
            PlayedSong = song1,
            TitleOverride = null,
            ExtraNotes = "FINALLY!!!",
            IsPlayedFromRecording = false,
            IsWorldPremiere = false,
            IsRotationSong = false
        };
        
        var entry2 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            SongNumber = 2,
            SortNumber = 20,
            PlayedSong = song2,
            IsPlayedFromRecording = false,
            IsWorldPremiere = false,
            IsRotationSong = false
        };
        
        var setlist = new SetlistDo
        {
            Id = 1,
            ConcertId = Guid.NewGuid().ToString(),
            LinkinpediaUrl = "https://lplive.net",
            Entries = [entry2, entry1]
        };
        
        // setup mocks
        _setlistRepository
            .GetByPrimaryKeyAsync(setlist.Id)
            .Returns(setlist);
        
        // call the service
        var result = await _setlistService.GetCompleteSetlist(setlist.Id);
        
        // verify result DTO
        Assert.NotNull(result);
        Assert.Equal(setlist.Id, result.Id);
        Assert.Equal(setlist.LinkinpediaUrl, result.LinkinpediaUrl);
        Assert.Equal(setlist.Entries.Count, result.Entries.Count);
        Assert.Equal(setlist.ConcertId, result.ConcertId);

        var returnedEntry1 = result.Entries[0];
        Assert.Equal(entry1.PlayedSong.Title, returnedEntry1.PlayedSong?.Title);
        Assert.Equal(entry1.PlayedSong.Isrc, returnedEntry1.PlayedSong?.Isrc);
        Assert.Equal(entry1.IsRotationSong, returnedEntry1.IsRotationSong);
        Assert.Equal(entry1.IsWorldPremiere, returnedEntry1.IsWorldPremiere);
        Assert.Equal(entry1.IsPlayedFromRecording, returnedEntry1.IsPlayedFromRecording);
        
        var returnedEntry2 = result.Entries[1];
        Assert.Equal(entry2.PlayedSong.Title, returnedEntry2.PlayedSong?.Title);
        Assert.Equal(entry2.PlayedSong.Isrc, returnedEntry2.PlayedSong?.Isrc);
        Assert.Equal(entry2.IsRotationSong, returnedEntry2.IsRotationSong);
        Assert.Equal(entry2.IsWorldPremiere, returnedEntry2.IsWorldPremiere);
        Assert.Equal(entry2.IsPlayedFromRecording, returnedEntry2.IsPlayedFromRecording);
        
        // verify mock calls
        await _setlistRepository
            .Received(1)
            .GetByPrimaryKeyAsync(setlist.Id);
    }
}