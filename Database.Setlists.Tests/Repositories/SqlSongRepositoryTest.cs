using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSongRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var repo = new SqlSongRepository(DbContext);
        
        var album = new AlbumDo
        {
            Title = "Test Album",
            LinkinpediaUrl = "https://lplive.net"
        };

        var song = new SongDo
        {
            Album = album,
            Title = "Test Song",
            Isrc = "1337",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(song);

        await repo.SaveChangesAsync();
        
        var retrievedSong = await repo.GetByPrimaryKeyAsync(song.Id);
        Assert.NotNull(retrievedSong);
        Assert.Equal(retrievedSong.Id, song.Id);
        Assert.Equal(retrievedSong.Title, song.Title);
        Assert.Equal(retrievedSong.Isrc, song.Isrc);
        Assert.Equal(retrievedSong.LinkinpediaUrl, song.LinkinpediaUrl);
        
        var retrievedAlbum = retrievedSong.Album;
        Assert.NotNull(retrievedAlbum);
        Assert.Equal(album.Id, retrievedAlbum.Id);
        Assert.Equal(album.Title, retrievedAlbum.Title);
        Assert.Equal(album.LinkinpediaUrl, retrievedAlbum.LinkinpediaUrl);
        
        repo.Delete(retrievedSong);
        
        await repo.SaveChangesAsync();
        
        retrievedSong = await repo.GetByPrimaryKeyAsync(song.Id);
        Assert.Null(retrievedSong);
    }
}