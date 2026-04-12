using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using NSubstitute;
using Service.Setlists.Exceptions;

namespace Service.Setlists.Tests;

public class AlbumServiceTest
{
    private readonly IAlbumRepository _albumRepository;
    private readonly ILambdaLogger _logger = new TestLambdaLogger();
    private readonly AlbumService _albumService;
    
    public AlbumServiceTest()
    {
        _albumRepository = Substitute.For<IAlbumRepository>();
        _albumService = new AlbumService(_albumRepository, _logger);
    }
    
    [Fact]
    public async Task CreateAlbumAsync()
    {
        // call the service
        var request = new CreateAlbumRequestDto
        {
            Title = "From Zero",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/From_Zero"
        };
        
        var song = await _albumService.CreateAlbumAsync(request);
        Assert.NotNull(song);
        Assert.Equal(request.Title, song.Title);
        Assert.Equal(request.LinkinpediaUrl, song.LinkinpediaUrl);
        
        // verify mock calls
        _albumRepository
            .Received(1)
            .Add(Arg.Is<AlbumDo>(m => m.Id == 0 && m.Title == request.Title && m.LinkinpediaUrl == request.LinkinpediaUrl));
        
        await _albumRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    [Fact]
    public async Task UpdateAlbumAsync()
    {
        // prepare mocks
        var mockAlbum = new AlbumDo
        {
            Id = 1,
            Title = "From Zero",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/From_Zero"
        };
        
        _albumRepository
            .GetByPrimaryKeyAsync(mockAlbum.Id)
            .Returns(mockAlbum);
        
        // call the service
        var request = new UpdateAlbumRequestDto
        {
            Title = "Mmm... cookies",
            LinkinpediaUrl = null
        };
        
        var album = await _albumService.UpdateAlbumAsync(mockAlbum.Id, request);
        Assert.NotNull(album);
        Assert.Equal(request.Title, album.Title);
        Assert.Equal(request.LinkinpediaUrl, album.LinkinpediaUrl);
        
        // verify mock calls
        _albumRepository
            .Received(1)
            .Update(Arg.Is<AlbumDo>(m => m.Id == mockAlbum.Id && m.Title == request.Title && m.LinkinpediaUrl == request.LinkinpediaUrl));
        
        await _albumRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    [Fact]
    public async Task GetAlbumByIdAsync()
    {
        // prepare mocks and test data
        var album = new AlbumDo
        {
            Id = 123,
            Title = "A Thousand Suns",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/A_Thousand_Suns"
        };
        
        _albumRepository
            .GetByPrimaryKeyAsync(album.Id)
            .Returns(album);

        // call the service
        var result = await _albumService.GetAlbumById(album.Id);
        Assert.NotNull(result);
        Assert.Equal(album.Title, result.Title);
        Assert.Equal(album.Id, result.Id);
        
        // verify mock calls
        await _albumRepository
            .Received(1)
            .GetByPrimaryKeyAsync(album.Id);
    }
    
    [Fact]
    public async Task GetAlbumByIdAsync_NotFound()
    {
        const uint albumId = 123u;
        
        // prepare mocks and test data
        _albumRepository
            .GetByPrimaryKeyAsync(albumId)
            .Returns((AlbumDo?)null);

        // call the service
        var exception = await Assert.ThrowsAsync<AlbumNotFoundException>(() => _albumService.GetAlbumById(albumId));
        Assert.Equal(albumId, exception.AlbumId);
        
        // verify mock calls
        await _albumRepository
            .Received(1)
            .GetByPrimaryKeyAsync(albumId);
    }
    
    [Fact]
    public async Task GetAllAlbumsAsync()
    {
        // prepare mocks and test data
        var album = new AlbumDo
        {
            Id = 1,
            Title = "Hybrid Theory",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/Hybrid_Theroy"
        };
        
        var album2 = new AlbumDo
        {
            Id = 2,
            Title = "Mmm... Cookies",
        };

        List<AlbumDo> mockAlbums = [album, album2];
        
        _albumRepository
            .QueryAsync(CancellationToken.None)
            .Returns(mockAlbums.ToAsyncEnumerable());

        // call the service
        var allAlbums = await _albumService.GetAllAlbumsAsync(CancellationToken.None).ToListAsync();
        Assert.NotNull(allAlbums);
        Assert.Equal(mockAlbums.Count, allAlbums.Count);
        
        // Results are ordered by title. That's why song2 is returned first
        var result1 = allAlbums[1];
        Assert.NotNull(result1);
        Assert.Equal(album2.Title, result1.Title);
        Assert.Equal(album2.Id, result1.Id);
        
        var result2 = allAlbums[0];
        Assert.NotNull(result2);
        Assert.Equal(album.Title, result2.Title);
        Assert.Equal(album.Id, result2.Id);
        
        // verify mock calls
        _albumRepository
            .Received(1)
            .QueryAsync(CancellationToken.None);
    }
    
    [Fact]
    public async Task DeleteSongWithIdAsync()
    {
        // prepare mocks and test data
        var album = new AlbumDo
        {
            Id = 8,
            Title = "Mmm... cookies"
        };
        
        _albumRepository
            .GetByPrimaryKeyAsync(album.Id)
            .Returns(album);
        _albumRepository
            .Delete(Arg.Is<AlbumDo>(m => m.Id == album.Id));
        
        // call the service
        await _albumService.DeleteAlbumWithIdAsync(album.Id);
        
        // verify mock calls
        await _albumRepository
            .Received(1)
            .GetByPrimaryKeyAsync(album.Id);
        
        _albumRepository
            .Received(1)
            .Delete(Arg.Is<AlbumDo>(m => m.Id == album.Id));

        await _albumRepository
            .Received(1)
            .SaveChangesAsync();
    }
}