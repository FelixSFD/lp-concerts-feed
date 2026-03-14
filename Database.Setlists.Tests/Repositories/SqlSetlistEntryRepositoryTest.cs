using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSetlistEntryRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task WithMultipleEntries()
    {
        var repo = new SqlSetlistEntryRepository(DbContext);
        var setlistRepo = new SqlSetlistRepository(DbContext);

        var setlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            Title = "Setlist 1",
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
        
        // test reading via Setlist header
        var setlistDo = await setlistRepo.GetByPrimaryKeyAsync(entry1.SetlistId);
        Assert.NotNull(setlistDo);
        retrievedEntry = setlistDo.Entries.FirstOrDefault();
        Assert.NotNull(retrievedEntry);
        AssertEntriesEqual(entry1, retrievedEntry);
        
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
            Title = "Setlist 1",
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

        var song1 = new SongDo
        {
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        var entry1 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            SetlistId = setlist.Id,
            ActNumber = 1,
            ExtraNotes = "Notes for this entry",
            TitleOverride = "Custom title",
            SongNumber = 1,
            PlayedSong = song1,
            IsWorldPremiere = false,
            IsPlayedFromRecording = false,
            IsRotationSong = true
        };
        
        repo.Add(entry1);
        
        var song2 = new SongDo
        {
            Title = "Lost",
            Isrc = "3452111",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/Lost"
        };
        var songVariant2 = new SongVariantDo
        {
            Song = song2,
            VariantName = "Piano Version",
            Description = "just amazing"
        };
        var entry2Extra = new SetlistEntrySongExtraDo
        {
            Id = Guid.NewGuid().ToString(),
            Song = song1,
            Description = "testing",
            Type = SetlistEntrySongExtraDo.ExtraType.ExtraVerse
        };
        var entry2 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            SetlistId = setlist.Id,
            ActNumber = 1,
            ExtraNotes = "second entry",
            TitleOverride = null,
            SongNumber = 2,
            PlayedSongVariant = songVariant2,
            SongExtras = [entry2Extra],
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

        var retrievedExtra = retrievedEntry.SongExtras.FirstOrDefault();
        Assert.NotNull(retrievedExtra);
        Assert.Equal(entry2Extra.Id, retrievedExtra.Id);
        
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
        
        AssertSongsEqual(expected.PlayedSong, actual.PlayedSong);
        AssertSongVariantsEqual(expected.PlayedSongVariant, actual.PlayedSongVariant);
    }


    private static void AssertActsEqual(SetlistActDo? expected, SetlistActDo? actual)
    {
        Assert.Equal(expected?.ActNumber, actual?.ActNumber);
        Assert.Equal(expected?.SetlistId, actual?.SetlistId);
        Assert.Equal(expected?.Title, actual?.Title);
    }
    
    private static void AssertSongsEqual(SongDo? expected, SongDo? actual)
    {
        Assert.Equal(expected?.Id, actual?.Id);
        Assert.Equal(expected?.Isrc, actual?.Isrc);
        Assert.Equal(expected?.Title, actual?.Title);
        Assert.Equal(expected?.LinkinpediaUrl, actual?.LinkinpediaUrl);
    }
    
    private static void AssertSongVariantsEqual(SongVariantDo? expected, SongVariantDo? actual)
    {
        Assert.Equal(expected?.Id, actual?.Id);
        Assert.Equal(expected?.IsrcOverride, actual?.IsrcOverride);
        Assert.Equal(expected?.Description, actual?.Description);
        Assert.Equal(expected?.VariantName, actual?.VariantName);
    }
}