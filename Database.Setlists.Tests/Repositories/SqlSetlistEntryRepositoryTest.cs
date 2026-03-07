using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSetlistEntryRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var repo = new SqlSetlistEntryRepository(DbContext);

        var setlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            LinkinpediaUrl = "https://lplive.net"
        };

        var entry1 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist,
            ExtraNotes = "Notes for this entry",
            TitleOverride = "Custom title",
            SongNumber = 1,
            IsWorldPremiere = false,
            IsPlayedFromRecording = false,
            IsRotationSong = true
        };
        
        repo.Add(entry1);
        
        var entry2 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist,
            ExtraNotes = "second entry",
            TitleOverride = null,
            SongNumber = 2,
            IsWorldPremiere = true,
            IsPlayedFromRecording = false,
            IsRotationSong = false
        };
        
        repo.Add(entry2);

        await repo.SaveChangesAsync();
        
        var retrievedEntry = await repo.GetByPrimaryKeyAsync(entry1.Id);
        Assert.NotNull(retrievedEntry);
        AssertEntriesEqual(entry1, retrievedEntry);
        
        retrievedEntry = await repo.GetByPrimaryKeyAsync(entry2.Id);
        Assert.NotNull(retrievedEntry);
        AssertEntriesEqual(entry2, retrievedEntry);
        
        repo.Delete(entry1);
        
        await repo.SaveChangesAsync();
        
        retrievedEntry = await repo.GetByPrimaryKeyAsync(entry1.Id);
        Assert.Null(retrievedEntry);
    }


    private static void AssertEntriesEqual(SetlistEntryDo expected, SetlistEntryDo actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.ExtraNotes, actual.ExtraNotes);
        Assert.Equal(expected.TitleOverride, actual.TitleOverride);
        Assert.Equal(expected.SongNumber, actual.SongNumber);
        Assert.Equal(expected.IsWorldPremiere, actual.IsWorldPremiere);
        Assert.Equal(expected.IsPlayedFromRecording, actual.IsPlayedFromRecording);
        Assert.Equal(expected.IsRotationSong, actual.IsRotationSong);
    }
}