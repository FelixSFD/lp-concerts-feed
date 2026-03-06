using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlAlbumRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var repo = new SqlAlbumRepository(DbContext);
        
        var album = new AlbumDo
        {
            Title = "Test Album",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(album);

        await repo.SaveChangesAsync();
        
        var retrievedAlbum = await repo.GetByIdAsync(album.Id);
        Assert.NotNull(retrievedAlbum);
        Assert.Equal(album.Id, retrievedAlbum.Id);
        Assert.Equal(album.Title, retrievedAlbum.Title);
        Assert.Equal(album.LinkinpediaUrl, retrievedAlbum.LinkinpediaUrl);
        
        repo.Delete(retrievedAlbum);
        
        await repo.SaveChangesAsync();
        
        retrievedAlbum = await repo.GetByIdAsync(album.Id);
        Assert.Null(retrievedAlbum);
    }
}