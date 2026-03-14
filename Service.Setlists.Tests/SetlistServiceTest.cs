using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Database.Concerts;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using LPCalendar.DataStructure.Setlists.Parameters;
using NSubstitute;
using Service.Setlists.Exceptions;

namespace Service.Setlists.Tests;

public class SetlistServiceTest
{
    private readonly ISetlistRepository _setlistRepository;
    private readonly ISetlistEntryRepository _setlistEntryRepository;
    private readonly IConcertRepository _concertRepository;
    private readonly ISongRepository _songRepository;
    private readonly ISongVariantRepository _songVariantRepository;
    private readonly ISongMashupRepository _songMashupRepository;
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
        _songMashupRepository = Substitute.For<ISongMashupRepository>();
        _actRepository = Substitute.For<ISetlistActRepository>();
        
        _setlistService = new SetlistService(_setlistRepository, _setlistEntryRepository, _concertRepository, _songRepository, _songVariantRepository, _songMashupRepository, _actRepository, _logger);
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
    public async Task AddSongMashupToSetlistAsync_MashupNotFound()
    {
        // prepare mocks and test data
        var setlist1 = new SetlistDo
        {
            Id = 1,
            ConcertId = Guid.NewGuid().ToString(),
            LinkinpediaUrl = "https://lplive.net"
        };

        var request = new AddSongMashupToSetlistRequestDto
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
            SongMashupParameters = new SongMashupParametersDto
            {
                SongMashupId = 1234
            }
        };
        
        _setlistRepository
            .GetByPrimaryKeyAsync(setlist1.Id)
            .Returns(setlist1);
        
        _songMashupRepository
            .GetByPrimaryKeyAsync(request.SongMashupParameters.SongMashupId)
            .Returns((SongMashupDo?)null);
        
        // call the service
        await Assert.ThrowsAsync<SongMashupNotFoundException>(() => _setlistService.AddSongMashupToSetlistAsync(request, setlist1.Id));
        
        // verify mock calls
        await _setlistRepository
            .Received(1)
            .GetByPrimaryKeyAsync(setlist1.Id);
        
        await _songMashupRepository
            .Received(1)
            .GetByPrimaryKeyAsync(request.SongMashupParameters.SongMashupId);
        
        _setlistEntryRepository
            .DidNotReceive()
            .Add(Arg.Any<SetlistEntryDo>());

        await _setlistEntryRepository
            .DidNotReceive()
            .SaveChangesAsync();
        _songMashupRepository
            .DidNotReceive()
            .Add(Arg.Any<SongMashupDo>());
    }
    
    [Fact]
    public async Task AddSongMashupToSetlistAsync_ExistingMashup()
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
        
        var song2 = new SongDo
        {
            Id = 1,
            Title = "Lost",
            Isrc = "123",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/Lost"
        };

        var mashup = new SongMashupDo
        {
            Id = 1,
            Title = "QWERTY/Lost",
            Songs =  new List<SongDo> { song1, song2 }
        };

        var request = new AddSongMashupToSetlistRequestDto
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
            SongMashupParameters = new SongMashupParametersDto
            {
                SongMashupId = mashup.Id
            }
        };
        
        _setlistRepository
            .GetByPrimaryKeyAsync(setlist1.Id)
            .Returns(setlist1);
        
        _songMashupRepository
            .GetByPrimaryKeyAsync(mashup.Id)
            .Returns(mashup);
        
        _setlistEntryRepository.Add(Arg.Is<SetlistEntryDo>(entry => entry.SetlistId == setlist1.Id && entry.SongNumber == request.EntryParameters.SongNumber));
        
        // call the service
        await _setlistService.AddSongMashupToSetlistAsync(request, setlist1.Id);
        
        // verify mock calls
        await _setlistRepository
            .Received(1)
            .GetByPrimaryKeyAsync(setlist1.Id);
        
        await _songMashupRepository
            .Received(1)
            .GetByPrimaryKeyAsync(mashup.Id);
        
        _setlistEntryRepository
            .Received(1)
            .Add(Arg.Is<SetlistEntryDo>(entry => entry.SetlistId == setlist1.Id && entry.SongNumber == request.EntryParameters.SongNumber));

        await _setlistEntryRepository.Received(1).SaveChangesAsync();
        _songMashupRepository
            .DidNotReceive()
            .Add(Arg.Any<SongMashupDo>());
    }
    
    
    [Fact]
    public async Task GetSetlistHeaders()
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

        List<SetlistDo> setlists = [setlist];
        
        // setup mocks
        _setlistRepository
            .QueryAsync(CancellationToken.None)
            .Returns(setlists.ToAsyncEnumerable());
        
        // call the service
        var result = await _setlistService.GetSetlistHeaders(CancellationToken.None).FirstOrDefaultAsync();
        
        // verify result DTO
        Assert.NotNull(result);
        Assert.Equal(setlist.Id, result.Id);
        Assert.Equal(setlist.LinkinpediaUrl, result.LinkinpediaUrl);
        Assert.Equal(setlist.ConcertId, result.ConcertId);
        
        // verify mock calls
        _setlistRepository
            .Received(1)
            .QueryAsync(CancellationToken.None);
        await _setlistRepository
            .DidNotReceive()
            .GetByPrimaryKeyAsync(setlist.Id);
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
    
    
    [Fact]
    public async Task GetSetlistsForConcert()
    {
        var concertId = Guid.NewGuid().ToString();
        
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
        
        var song3 = new SongDo
        {
            Id = 1,
            Title = "Bleed It Out",
            Isrc = "1234123"
        };
        
        var act1 = new SetlistActDo
        {
            ActNumber = 1,
            Title = "Inception Intro A",
        };
        
        var act2 = new SetlistActDo
        {
            ActNumber = 2,
            Title = "Encore",
        };
        
        var entry0PreShowSong = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            SongNumber = 0,
            SortNumber = 10,
            PlayedSong = null,
            TitleOverride = "Some pre-show song",
            ExtraNotes = "played during countdown",
            IsPlayedFromRecording = true,
            IsWorldPremiere = false,
            IsRotationSong = false
        };

        var entry1 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            ActNumber = act1.ActNumber,
            Act = act1,
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
            ActNumber = act1.ActNumber,
            Act = act1,
            SongNumber = 2,
            SortNumber = 20,
            PlayedSong = song2,
            IsPlayedFromRecording = false,
            IsWorldPremiere = false,
            IsRotationSong = false
        };
        
        var entry3 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            ActNumber = act2.ActNumber,
            Act = act2,
            SongNumber = 3,
            SortNumber = 30,
            PlayedSong = song3,
            IsPlayedFromRecording = false,
            IsWorldPremiere = false,
            IsRotationSong = false
        };
        
        var setlist = new SetlistDo
        {
            Id = 1,
            ConcertId = Guid.NewGuid().ToString(),
            LinkinpediaUrl = "https://lplive.net",
            Entries = [entry0PreShowSong, entry2, entry1, entry3]
        };
        
        List<SetlistDo> concertSetlists = [setlist];
        
        // setup mocks
        _setlistRepository
            .GetByConcertIdAsync(concertId)
            .Returns(concertSetlists.ToAsyncEnumerable());
        
        // call the service
        var results = await _setlistService.GetSetlistsForConcert(concertId);
        var result = results.FirstOrDefault();
        
        // verify result DTO
        Assert.NotNull(result);
        Assert.Equal(setlist.Id, result.Id);
        Assert.Equal(setlist.LinkinpediaUrl, result.LinkinpediaUrl);
        Assert.Equal(setlist.ConcertId, result.ConcertId);
        
        // check if acts are returned
        var foundActs = result.Acts;
        Assert.NotNull(foundActs);
        Assert.Equal(2, foundActs.Count);
        
        var foundAct1 = result.Acts[0];
        Assert.NotNull(foundAct1);
        Assert.Equal(act1.ActNumber, foundAct1.ActNumber);
        Assert.Equal(act1.Title, foundAct1.Title);
        
        var foundAct2 = result.Acts[1];
        Assert.NotNull(foundAct2);
        Assert.Equal(act2.ActNumber, foundAct2.ActNumber);
        Assert.Equal(act2.Title, foundAct2.Title);

        // check entries
        Assert.Equal(4, result.Entries.Count);
        var returnedEntry0 = result.Entries[0];
        Assert.Null(returnedEntry0.PlayedSong);
        Assert.Equal(entry0PreShowSong.IsRotationSong, returnedEntry0.IsRotationSong);
        Assert.Equal(entry0PreShowSong.IsWorldPremiere, returnedEntry0.IsWorldPremiere);
        Assert.Equal(entry0PreShowSong.IsPlayedFromRecording, returnedEntry0.IsPlayedFromRecording);
        Assert.Equal(entry0PreShowSong.ActNumber, returnedEntry0.ActNumber);
        
        var returnedEntry1 = result.Entries[1];
        Assert.Equal(entry1.PlayedSong.Title, returnedEntry1.PlayedSong?.Title);
        Assert.Equal(entry1.PlayedSong.Isrc, returnedEntry1.PlayedSong?.Isrc);
        Assert.Equal(entry1.IsRotationSong, returnedEntry1.IsRotationSong);
        Assert.Equal(entry1.IsWorldPremiere, returnedEntry1.IsWorldPremiere);
        Assert.Equal(entry1.IsPlayedFromRecording, returnedEntry1.IsPlayedFromRecording);
        Assert.Equal(entry1.ActNumber, returnedEntry1.ActNumber);
        
        var returnedEntry2 = result.Entries[2];
        Assert.Equal(entry2.PlayedSong.Title, returnedEntry2.PlayedSong?.Title);
        Assert.Equal(entry2.PlayedSong.Isrc, returnedEntry2.PlayedSong?.Isrc);
        Assert.Equal(entry2.IsRotationSong, returnedEntry2.IsRotationSong);
        Assert.Equal(entry2.IsWorldPremiere, returnedEntry2.IsWorldPremiere);
        Assert.Equal(entry2.IsPlayedFromRecording, returnedEntry2.IsPlayedFromRecording);
        Assert.Equal(entry2.ActNumber, returnedEntry2.ActNumber);
        
        var returnedEntry3 = result.Entries[3];
        Assert.Equal(entry3.PlayedSong.Title, returnedEntry3.PlayedSong?.Title);
        Assert.Equal(entry3.PlayedSong.Isrc, returnedEntry3.PlayedSong?.Isrc);
        Assert.Equal(entry3.IsRotationSong, returnedEntry3.IsRotationSong);
        Assert.Equal(entry3.IsWorldPremiere, returnedEntry3.IsWorldPremiere);
        Assert.Equal(entry3.IsPlayedFromRecording, returnedEntry3.IsPlayedFromRecording);
        Assert.Equal(entry3.ActNumber, returnedEntry3.ActNumber);
        
        // verify mock calls
        _setlistRepository
            .Received(1)
            .GetByConcertIdAsync(concertId);
    }
}