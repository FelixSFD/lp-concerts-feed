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
        Assert.Equal(1u, album.Id);
    }
}