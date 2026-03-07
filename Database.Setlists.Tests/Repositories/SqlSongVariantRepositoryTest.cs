using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSongVariantRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var repo = new SqlSongVariantRepository(DbContext);
        var song = new SongDo
        {
            Title = "Test Song",
            Isrc = "1337",
            LinkinpediaUrl = "https://lplive.net"
        };

        var variant = new SongVariantDo
        {
            Song = song,
            Description = "Test description",
            IsrcOverride = "9999",
            VariantName = "Piano Version"
        };

        repo.Add(variant);
        await repo.SaveChangesAsync();
        
        var retrievedVariant = await repo.GetByPrimaryKeyAsync(song.Id);
        Assert.NotNull(retrievedVariant);
        Assert.Equal(retrievedVariant.Id, variant.Id);
        Assert.Equal(retrievedVariant.Description, variant.Description);
        Assert.Equal(retrievedVariant.IsrcOverride, variant.IsrcOverride);

        var retrievedSong = retrievedVariant.Song;
        Assert.NotNull(retrievedSong);
        Assert.Equal(retrievedSong.Id, song.Id);
        Assert.Equal(retrievedSong.Title, song.Title);
        Assert.Equal(retrievedSong.Isrc, song.Isrc);
        Assert.Equal(retrievedSong.LinkinpediaUrl, song.LinkinpediaUrl);
        
        repo.Delete(retrievedVariant);
        
        await repo.SaveChangesAsync();
        
        retrievedVariant = await repo.GetByPrimaryKeyAsync(song.Id);
        Assert.Null(retrievedVariant);
    }
}