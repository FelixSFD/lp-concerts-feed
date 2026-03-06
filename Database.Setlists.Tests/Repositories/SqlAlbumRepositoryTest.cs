using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlAlbumRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task SaveAlbum()
    {
        var repo = new SqlAlbumRepository(DbContext);
        
        var album = new AlbumDo
        {
            Title = "Test Album",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(album);

        await repo.SaveChangesAsync();
        
        Assert.Equal(1, DbContext.Albums.Count());
        Assert.NotEqual(0u, album.Id);
        Assert.Equal("Test Album", album.Title);
        Assert.Equal("https://lplive.net", album.LinkinpediaUrl);
    }
}