using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using NSubstitute;
using Service.Setlists.Exceptions;

namespace Service.Setlists.Tests;

public class SongServiceTest
{
    private readonly ISongRepository _songRepository;
    private readonly ISongVariantRepository _songVariantRepository;
    private readonly ISongMashupRepository _songMashupRepository;
    private readonly ILambdaLogger _logger = new TestLambdaLogger();
    private readonly SongService _songService;

    public SongServiceTest()
    {
        _songRepository = Substitute.For<ISongRepository>();
        _songVariantRepository = Substitute.For<ISongVariantRepository>();
        _songMashupRepository = Substitute.For<ISongMashupRepository>();
        
        _songService = new SongService(_songRepository, _songVariantRepository, _songMashupRepository, _logger);
    }
    
    
    [Fact]
    public async Task GetAllSongsAsync()
    {
        // prepare mocks and test data
        var song = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        var song2 = new SongDo
        {
            Id = 3121,
            Title = "Lost",
            Isrc = "1",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/Lost"
        };

        List<SongDo> mockSongs = [song, song2];
        
        _songRepository
            .QueryAsync(CancellationToken.None)
            .Returns(mockSongs.ToAsyncEnumerable());

        // call the service
        var allSongs = await _songService.GetAllSongsAsync(CancellationToken.None).ToListAsync();
        Assert.NotNull(allSongs);
        Assert.Equal(mockSongs.Count, allSongs.Count);
        
        // Results are ordered by title. That's why song2 is returned first
        var result1 = allSongs[0];
        Assert.NotNull(result1);
        Assert.Equal(song2.Title, result1.Title);
        Assert.Equal(song2.Isrc, result1.Isrc);
        Assert.Equal(song2.Id, result1.Id);
        
        var result2 = allSongs[1];
        Assert.NotNull(result2);
        Assert.Equal(song.Title, result2.Title);
        Assert.Equal(song.Isrc, result2.Isrc);
        Assert.Equal(song.Id, result2.Id);
        
        // verify mock calls
        _songRepository
            .Received(1)
            .QueryAsync(CancellationToken.None);
    }

    
    [Fact]
    public async Task GetSongByIdAsync()
    {
        // prepare mocks and test data
        var song = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        _songRepository
            .GetByPrimaryKeyAsync(song.Id)
            .Returns(song);

        // call the service
        var result = await _songService.GetSongById(song.Id);
        Assert.NotNull(result);
        Assert.Equal(song.Title, result.Title);
        Assert.Equal(song.Isrc, result.Isrc);
        Assert.Equal(song.Id, result.Id);
        
        // verify mock calls
        await _songRepository
            .Received(1)
            .GetByPrimaryKeyAsync(song.Id);
    }
    
    
    [Fact]
    public async Task GetSongByIdAsync_NotFound()
    {
        // prepare mocks and test data
        var song = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        _songRepository
            .GetByPrimaryKeyAsync(song.Id)
            .Returns(song);

        // call the service
        var songNotFoundException = await Assert.ThrowsAsync<SongNotFoundException>(() => _songService.GetSongById(999));
        Assert.NotNull(songNotFoundException);
        Assert.Equal(999u, songNotFoundException.SongId);

        // verify mock calls
        await _songRepository
            .Received(1)
            .GetByPrimaryKeyAsync(999);
    }
    
    
    [Fact]
    public async Task DeleteSongWithIdAsync()
    {
        // prepare mocks and test data
        var song = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        _songRepository
            .GetByPrimaryKeyAsync(song.Id)
            .Returns(song);
        _songRepository
            .Delete(Arg.Is<SongDo>(m => m.Id == song.Id));
        
        // call the service
        await _songService.DeleteSongWithIdAsync(song.Id);
        
        // verify mock calls
        await _songRepository
            .Received(1)
            .GetByPrimaryKeyAsync(song.Id);
        
        _songRepository
            .Received(1)
            .Delete(Arg.Is<SongDo>(m => m.Id == song.Id));

        await _songRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    
    [Fact]
    public async Task GetSongVariantByIdAsync()
    {
        // prepare mocks and test data
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };

        var variant = new SongVariantDo
        {
            Song = song1,
            SongId = song1.Id,
            VariantName = "Live with Em",
            Id = 123,
            Description = "Something new"
        };
        
        _songVariantRepository
            .GetByPrimaryKeyAsync(variant.Id)
            .Returns(variant);

        // call the service
        var result = await _songService.GetSongVariantById(variant.Id);
        Assert.NotNull(result);
        Assert.Equal(variant.Id, result.Id);
        Assert.Equal(variant.SongId, result.SongId);
        Assert.Equal(variant.Description, result.Description);
        Assert.Equal(variant.IsrcOverride, result.IsrcOverride);
        Assert.Equal(variant.VariantName, result.VariantName);
        
        // verify mock calls
        await _songVariantRepository
            .Received(1)
            .GetByPrimaryKeyAsync(variant.Id);
    }
    
    
    [Fact]
    public async Task GetSongVariantByIdAsync_NotFound()
    {
        // prepare mocks and test data
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };

        var variant = new SongVariantDo
        {
            Song = song1,
            SongId = song1.Id,
            VariantName = "Live with Em",
            Id = 123,
            Description = "Something new"
        };
        
        _songVariantRepository
            .GetByPrimaryKeyAsync(variant.Id)
            .Returns(variant);

        // call the service
        var songVariantNotFoundException = await Assert.ThrowsAsync<SongVariantNotFoundException>(() => _songService.GetSongVariantById(999));
        Assert.NotNull(songVariantNotFoundException);
        Assert.Equal(999u, songVariantNotFoundException.SongVariantId);

        // verify mock calls
        await _songVariantRepository
            .Received(1)
            .GetByPrimaryKeyAsync(999);
    }
    
    
    [Fact]
    public async Task GetVariantsOfSongAsync()
    {
        // prepare mocks and test data
        var song = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };

        var variant1 = new SongVariantDo
        {
            Song = song,
            SongId = song.Id,
            VariantName = "Live with Em",
            Id = 123,
            Description = "Something new"
        };
        
        var variant2 = new SongVariantDo
        {
            Song = song,
            SongId = song.Id,
            VariantName = "Live with Em (Piano Version)",
            Id = 124
        };
        
        _songVariantRepository
            .GetVariantsOfSongAsync(song.Id)
            .Returns([variant1, variant2]);

        // call the service
        var results = await _songService.GetVariantsOfSong(song.Id);
        Assert.NotNull(results);
        Assert.Equal(2, results.Count);
        
        var result1 = results[0];
        Assert.NotNull(result1);
        Assert.Equal(variant1.Id, result1.Id);
        Assert.Equal(variant1.VariantName, result1.VariantName);
        Assert.Equal(variant1.Description, result1.Description);
        Assert.Equal(variant1.IsrcOverride, result1.IsrcOverride);
        Assert.Equal(variant1.SongId, result1.SongId);
        
        var result2 = results[1];
        Assert.NotNull(result2);
        Assert.Equal(variant2.Id, result2.Id);
        Assert.Equal(variant2.VariantName, result2.VariantName);
        Assert.Equal(variant2.Description, result2.Description);
        Assert.Equal(variant2.IsrcOverride, result2.IsrcOverride);
        Assert.Equal(variant2.SongId, result2.SongId);

        // verify mock calls
        await _songVariantRepository
            .Received(1)
            .GetVariantsOfSongAsync(song.Id);
    }


    [Fact]
    public async Task CreateMashupOfSongsAsync()
    {
        // prepare mocks and test data
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        var song2 = new SongDo
        {
            Id = 1111,
            Title = "One More Light",
            Isrc = "123234"
        };

        SongDo[] mockSongs = [song1, song2];
        _songRepository.GetSongsByIds(song1.Id, song2.Id).Returns(mockSongs.ToAsyncEnumerable());
        
        // call the service
        var request = new CreateSongMashupRequestDto
        {
            Title = "Weird mashup",
            LinkinpediaUrl = "https://lplive.net",
            SongIds = [song1.Id, song2.Id]
        };
        
        var mashup = await _songService.CreateMashupOfSongsAsync(request);
        Assert.NotNull(mashup);
        Assert.Equal(request.Title, mashup.Title);
        Assert.Equal(request.LinkinpediaUrl, mashup.LinkinpediaUrl);
        Assert.Equal(2, mashup.Songs.Count);

        var addedSong1 = mashup.Songs[0];
        Assert.NotNull(addedSong1);
        Assert.Equal(song1.Id, addedSong1.Id);
        Assert.Equal(song1.Title, addedSong1.Title);
        Assert.Equal(song1.Isrc, addedSong1.Isrc);
        
        var addedSong2 = mashup.Songs[1];
        Assert.NotNull(addedSong2);
        Assert.Equal(song2.Id, addedSong2.Id);
        Assert.Equal(song2.Title, addedSong2.Title);
        Assert.Equal(song2.Isrc, addedSong2.Isrc);
        
        // verify mock calls
        _songRepository
            .Received(1)
            .GetSongsByIds(song1.Id, song2.Id);
        
        _songMashupRepository
            .Received(1)
            .Add(Arg.Is<SongMashupDo>(m => m.Id == 0 && m.Title == request.Title && m.LinkinpediaUrl == request.LinkinpediaUrl));
    }
    
    
    [Fact]
    public async Task CreateMashupOfSongsAsync_NotEnoughSongs()
    {
        // prepare mocks and test data
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };

        SongDo[] mockSongs = [song1];
        _songRepository.GetSongsByIds(song1.Id).Returns(mockSongs.ToAsyncEnumerable());
        
        // call the service
        var request = new CreateSongMashupRequestDto
        {
            Title = "no mashup",
            SongIds = [song1.Id]
        };
        
        var exception = await Assert.ThrowsAsync<InvalidMashupException>(async () => await _songService.CreateMashupOfSongsAsync(request));
        Assert.NotNull(exception);
        
        // verify mock calls
        _songRepository
            .DidNotReceive()
            .GetSongsByIds(Arg.Any<uint[]>());
        
        _songMashupRepository
            .DidNotReceive()
            .Add(Arg.Any<SongMashupDo>());
    }
    
    
    [Fact]
    public async Task GetAllSongMashupsAsync()
    {
        // prepare mocks and test data
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        var song2 = new SongDo
        {
            Id = 1111,
            Title = "One More Light",
            Isrc = "123234"
        };

        var mashup = new SongMashupDo
        {
            Id = 1,
            Title = "Weird mashup",
            LinkinpediaUrl = "https://lplive.net",
            Songs = [song1, song2]
        };

        List<SongMashupDo> mashups = [mashup];
        _songMashupRepository.QueryAsync(CancellationToken.None).Returns(mashups.ToAsyncEnumerable());
        
        // call the service
        var foundMashup = await _songService.GetAllSongMashupsAsync(CancellationToken.None).FirstOrDefaultAsync();
        Assert.NotNull(foundMashup);
        Assert.Equal(mashup.Title, foundMashup.Title);
        Assert.Equal(mashup.LinkinpediaUrl, foundMashup.LinkinpediaUrl);
        Assert.Equal(2, foundMashup.Songs.Count);

        var addedSong1 = foundMashup.Songs[0];
        Assert.NotNull(addedSong1);
        Assert.Equal(song1.Id, addedSong1.Id);
        Assert.Equal(song1.Title, addedSong1.Title);
        Assert.Equal(song1.Isrc, addedSong1.Isrc);
        
        var addedSong2 = foundMashup.Songs[1];
        Assert.NotNull(addedSong2);
        Assert.Equal(song2.Id, addedSong2.Id);
        Assert.Equal(song2.Title, addedSong2.Title);
        Assert.Equal(song2.Isrc, addedSong2.Isrc);
        
        // verify mock calls
        _songMashupRepository
            .Received(1)
            .QueryAsync(CancellationToken.None);
    }
    
    [Fact]
    public async Task GetMashupByIdAsync()
    {
        // prepare mocks and test data
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        var song2 = new SongDo
        {
            Id = 1111,
            Title = "One More Light",
            Isrc = "123234"
        };

        var mashup = new SongMashupDo
        {
            Id = 1,
            Title = "Weird mashup",
            LinkinpediaUrl = "https://lplive.net",
            Songs = [song1, song2]
        };

        _songMashupRepository.GetByPrimaryKeyAsync(mashup.Id).Returns(mashup);
        
        // call the service
        var foundMashup = await _songService.GetMashupByIdAsync(mashup.Id);
        Assert.NotNull(foundMashup);
        Assert.Equal(mashup.Title, foundMashup.Title);
        Assert.Equal(mashup.LinkinpediaUrl, foundMashup.LinkinpediaUrl);
        Assert.Equal(2, foundMashup.Songs.Count);

        var addedSong1 = foundMashup.Songs[0];
        Assert.NotNull(addedSong1);
        Assert.Equal(song1.Id, addedSong1.Id);
        Assert.Equal(song1.Title, addedSong1.Title);
        Assert.Equal(song1.Isrc, addedSong1.Isrc);
        
        var addedSong2 = foundMashup.Songs[1];
        Assert.NotNull(addedSong2);
        Assert.Equal(song2.Id, addedSong2.Id);
        Assert.Equal(song2.Title, addedSong2.Title);
        Assert.Equal(song2.Isrc, addedSong2.Isrc);
        
        // verify mock calls
        await _songMashupRepository
            .Received(1)
            .GetByPrimaryKeyAsync(mashup.Id);
        
        _songMashupRepository
            .DidNotReceive()
            .QueryAsync(Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task DeleteMashupWithIdAsync()
    {
        // prepare mocks and test data
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        var song2 = new SongDo
        {
            Id = 1111,
            Title = "One More Light",
            Isrc = "123234"
        };

        var mashup = new SongMashupDo
        {
            Id = 1,
            Title = "Weird mashup",
            LinkinpediaUrl = "https://lplive.net",
            Songs = [song1, song2]
        };
        
        _songMashupRepository
            .GetByPrimaryKeyAsync(mashup.Id)
            .Returns(mashup);
        _songMashupRepository
            .Delete(Arg.Is<SongMashupDo>(m => m.Id == mashup.Id));
        
        // call the service
        await _songService.DeleteMashupWithIdAsync(mashup.Id);
        
        // verify mock calls
        await _songMashupRepository
            .Received(1)
            .GetByPrimaryKeyAsync(mashup.Id);
        
        _songMashupRepository
            .Received(1)
            .Delete(Arg.Is<SongMashupDo>(m => m.Id == mashup.Id));

        await _songMashupRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    
    [Fact]
    public async Task UpdateMashupAsync()
    {
        // prepare mocks and test data
        var song1 = new SongDo
        {
            Id = 1234,
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        
        var song2 = new SongDo
        {
            Id = 1111,
            Title = "One More Light",
            Isrc = "123234"
        };
        
        var song3 = new SongDo
        {
            Id = 1,
            Title = "Lost",
            Isrc = "1"
        };

        var mashup = new SongMashupDo
        {
            Id = 1,
            Title = "Weird mashup",
            LinkinpediaUrl = "https://lplive.net",
            Songs = [song1, song2]
        };

        var request = new UpdateSongMashupRequestDto
        {
            Title = "new title",
            LinkinpediaUrl = "https://linkinpedia.com/",
            SongIds = [song1.Id, song3.Id]
        };

        List<SongDo> mockSongs = [song1, song3];

        _songRepository
            .GetSongsByIds(song1.Id, song3.Id)
            .Returns(mockSongs.ToAsyncEnumerable());
        _songMashupRepository
            .GetByPrimaryKeyAsync(mashup.Id)
            .Returns(mashup);
        _songMashupRepository
            .Update(Arg.Is<SongMashupDo>(m => m.Id == mashup.Id
                                              && m.Title == request.Title
                                              && m.LinkinpediaUrl == request.LinkinpediaUrl
                                              && m.Songs.Any(s => s.Id == song1.Id)
                                              && m.Songs.Any(s => s.Id == song3.Id)
                                              && m.Songs.All(s => s.Id != song2.Id)
            ));
        
        // call the service
        var result = await _songService.UpdateMashupAsync(mashup.Id, request);
        
        // verify results
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(result.LinkinpediaUrl, request.LinkinpediaUrl);
        Assert.Equal(result.Songs.Count, mockSongs.Count);
        
        // verify mock calls
        await _songMashupRepository
            .Received(1)
            .GetByPrimaryKeyAsync(mashup.Id);

        _songRepository
            .Received(1)
            .GetSongsByIds(song1.Id, song3.Id);
        
        _songMashupRepository
            .Received(1)
            .Update(Arg.Is<SongMashupDo>(m => m.Id == mashup.Id
                                              && m.Title == request.Title
                                              && m.LinkinpediaUrl == request.LinkinpediaUrl
                                              && m.Songs.Any(s => s.Id == song1.Id)
                                              && m.Songs.Any(s => s.Id == song3.Id)
                                              && m.Songs.All(s => s.Id != song2.Id)
            ));

        await _songMashupRepository
            .Received(1)
            .SaveChangesAsync();
    }
}