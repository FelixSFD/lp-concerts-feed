using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using LPCalendar.DataStructure.Setlists;
using NSubstitute;

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
}