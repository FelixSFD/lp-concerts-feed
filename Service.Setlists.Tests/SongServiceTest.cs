using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using NSubstitute;
using Service.Setlists.Exceptions;

namespace Service.Setlists.Tests;

public class SongServiceTest
{
    private readonly ISongRepository _songRepository;
    private readonly ISongVariantRepository _songVariantRepository;
    private readonly ILambdaLogger _logger = new TestLambdaLogger();
    private readonly SongService _songService;

    public SongServiceTest()
    {
        _songRepository = Substitute.For<ISongRepository>();
        _songVariantRepository = Substitute.For<ISongVariantRepository>();
        
        _songService = new SongService(_songRepository, _songVariantRepository, _logger);
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
}