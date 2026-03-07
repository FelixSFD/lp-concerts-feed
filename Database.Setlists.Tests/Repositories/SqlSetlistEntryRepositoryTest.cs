using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSetlistEntryRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task WithMultipleEntries()
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
    
    
    [Fact]
    public async Task WithMultipleActs()
    {
        var repo = new SqlSetlistEntryRepository(DbContext);
        var setlistRepo = new SqlSetlistRepository(DbContext);
        var actRepo = new SqlSetlistActRepository(DbContext);

        var setlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            LinkinpediaUrl = "https://lplive.net"
        };
        
        setlistRepo.Add(setlist);
        await setlistRepo.SaveChangesAsync();

        var act1 = new SetlistActDo
        {
            SetlistId = setlist.Id,
            ActNumber = 1,
            Title = "Act 1: Intro"
        };
        actRepo.Add(act1);
        
        var act2 = new SetlistActDo
        {
            SetlistId = setlist.Id,
            ActNumber = 2,
            Title = "Encore"
        };
        actRepo.Add(act2);
        
        await actRepo.SaveChangesAsync();

        var entry1 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            SetlistId = setlist.Id,
            ActNumber = 1,
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
            SetlistId = setlist.Id,
            ActNumber = 1,
            ExtraNotes = "second entry",
            TitleOverride = null,
            SongNumber = 2,
            IsWorldPremiere = true,
            IsPlayedFromRecording = false,
            IsRotationSong = false
        };
        
        repo.Add(entry2);
        
        var entry3 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            SetlistId = setlist.Id,
            ActNumber = 2,
            TitleOverride = null,
            SongNumber = 3,
            IsWorldPremiere = false,
            IsPlayedFromRecording = false,
            IsRotationSong = false
        };
        
        repo.Add(entry3);

        await repo.SaveChangesAsync();
        
        var retrievedEntry = await repo.GetByPrimaryKeyAsync(entry1.Id);
        Assert.NotNull(retrievedEntry);
        AssertEntriesEqual(entry1, retrievedEntry);
        
        retrievedEntry = await repo.GetByPrimaryKeyAsync(entry2.Id);
        Assert.NotNull(retrievedEntry);
        AssertEntriesEqual(entry2, retrievedEntry);
        
        retrievedEntry = await repo.GetByPrimaryKeyAsync(entry3.Id);
        Assert.NotNull(retrievedEntry);
        AssertEntriesEqual(entry3, retrievedEntry);
        
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

        var expectedAct = expected.Act;
        var actualAct = actual.Act;
        AssertActsEqual(expectedAct, actualAct);
    }


    private static void AssertActsEqual(SetlistActDo? expected, SetlistActDo? actual)
    {
        Assert.Equal(expected?.ActNumber, actual?.ActNumber);
        Assert.Equal(expected?.SetlistId, actual?.SetlistId);
        Assert.Equal(expected?.Title, actual?.Title);
    }
}