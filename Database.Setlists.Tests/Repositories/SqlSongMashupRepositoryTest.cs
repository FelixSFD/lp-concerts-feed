using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSongMashupRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task TestWithTwoSongs()
    {
        var songRepo = new SqlSongRepository(DbContext);
        var repo = new SqlSongMashupRepository(DbContext);
        
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
        
        var song2 = new SongDo
        {
            Album = album,
            Title = "Test Song 2",
            Isrc = "1234",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        songRepo.Add(song2);

        await songRepo.SaveChangesAsync();

        var mashup = new SongMashupDo
        {
            Title = "Test Mashup",
            LinkinpediaUrl = "https://lplive.net",
            Songs = [song, song2]
        };
        
        repo.Add(mashup);
        await repo.SaveChangesAsync();
        
        var retrievedMashup = await repo.GetByPrimaryKeyAsync(mashup.Id);
        Assert.NotNull(retrievedMashup);
        Assert.Equal(retrievedMashup.Id, mashup.Id);
        Assert.Equal(retrievedMashup.Title, mashup.Title);
        Assert.Equal(retrievedMashup.LinkinpediaUrl, mashup.LinkinpediaUrl);
        
        Assert.Equal(2, retrievedMashup.Songs.Count);

        repo.Delete(mashup);
        
        await repo.SaveChangesAsync();
        
        retrievedMashup = await repo.GetByPrimaryKeyAsync(mashup.Id);
        Assert.Null(retrievedMashup);
    }
}