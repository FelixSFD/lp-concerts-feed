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
            Songs = []
        };
        mashup.Songs.Add(new SongInMashupDo
        {
            SongId = song.Id,
            Song = song,
            Mashup = mashup
        });
        mashup.Songs.Add(new SongInMashupDo
        {
            SongId = song2.Id,
            Song = song2,
            Mashup = mashup
        });
        
        repo.Add(mashup);
        await repo.SaveChangesAsync();
        
        var retrievedMashup = await repo.GetByPrimaryKeyAsync(mashup.Id);
        Assert.NotNull(retrievedMashup);
        Assert.Equal(retrievedMashup.Id, mashup.Id);
        Assert.Equal(retrievedMashup.Title, mashup.Title);
        Assert.Equal(retrievedMashup.LinkinpediaUrl, mashup.LinkinpediaUrl);
        
        Assert.Equal(2, retrievedMashup.Songs.Count);
        
        var includedSongs = retrievedMashup.Songs.Select(s => s.Song).ToList();
        var includedSong1 = includedSongs[0];
        var includedSong2 = includedSongs[1];
        Assert.Equal("Test Song", includedSong1.Title);
        Assert.Equal("Test Song 2", includedSong2.Title);

        repo.Delete(mashup);
        
        await repo.SaveChangesAsync();
        
        retrievedMashup = await repo.GetByPrimaryKeyAsync(mashup.Id);
        Assert.Null(retrievedMashup);
    }
}