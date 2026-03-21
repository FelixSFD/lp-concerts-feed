using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSetlistRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task UpdateSetlist()
    {
        var repo = new SqlSetlistRepository(DbContext);

        var setlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            ConcertTitle = "Setlist 1",
            SetName = "Set A2",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(setlist);

        await repo.SaveChangesAsync();
        
        setlist.LinkinpediaUrl = "https://linkinpedia.com";
        setlist.ConcertTitle = "Concert 1";
        setlist.SetName = "Set A2.1";
        
        repo.Update(setlist);
        await repo.SaveChangesAsync();
        
        var retrievedSetlist = await repo.GetByConcertIdAsync(setlist.ConcertId).FirstOrDefaultAsync();
        Assert.NotNull(retrievedSetlist);
        Assert.Equal(setlist.Id, retrievedSetlist.Id);
        Assert.Equal(setlist.ConcertId, retrievedSetlist.ConcertId);
        Assert.Equal(setlist.LinkinpediaUrl, retrievedSetlist.LinkinpediaUrl);
        Assert.Equal(setlist.ConcertTitle, retrievedSetlist.ConcertTitle);
        Assert.Equal(setlist.SetName, retrievedSetlist.SetName);
        Assert.NotNull(retrievedSetlist.Entries);
        Assert.Empty(retrievedSetlist.Entries);
    }
    
    
    [Fact]
    public async Task GetByIdAsync_HeaderOnly()
    {
        var repo = new SqlSetlistRepository(DbContext);

        var setlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            ConcertTitle = "Setlist 1",
            SetName = "Set A2",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(setlist);

        await repo.SaveChangesAsync();
        
        var retrievedSetlist = await repo.GetByPrimaryKeyAsync(setlist.Id);
        Assert.NotNull(retrievedSetlist);
        Assert.NotNull(retrievedSetlist.Entries);
        Assert.Empty(retrievedSetlist.Entries);
        Assert.Equal(setlist.Id, retrievedSetlist.Id);
        Assert.Equal(setlist.ConcertId, retrievedSetlist.ConcertId);
        Assert.Equal(setlist.LinkinpediaUrl, retrievedSetlist.LinkinpediaUrl);
        Assert.Equal(setlist.ConcertTitle, retrievedSetlist.ConcertTitle);
        Assert.Equal(setlist.SetName, retrievedSetlist.SetName);
        
        repo.Delete(retrievedSetlist);
        
        await repo.SaveChangesAsync();
        
        retrievedSetlist = await repo.GetByPrimaryKeyAsync(setlist.Id);
        Assert.Null(retrievedSetlist);
    }
    
    
    [Fact]
    public async Task GetByIdAsync_WithEntries()
    {
        var repo = new SqlSetlistRepository(DbContext);
        var entriesRepo = new SqlSetlistEntryRepository(DbContext);

        var setlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            ConcertTitle = "Setlist 1",
            SetName = "Set A2",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(setlist);

        await repo.SaveChangesAsync();

        var act = new SetlistActDo
        {
            ActNumber = 1,
            SetlistId = setlist.Id,
            Setlist = setlist,
            Title = "Intro"
        };
        
        var song1 = new SongDo
        {
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        var song2 = new SongDo
        {
            Title = "Lost",
            Isrc = "1234",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/Lost"
        };
        var song2Piano = new SongVariantDo
        {
            Song = song2,
            SongId = song2.Id,
            VariantName = "Piano Version",
            Description = "just beautiful",
            IsrcOverride = null
        };
        
        var entryCustomTitle = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist,
            Act = act,
            ExtraNotes = "Notes for this entry",
            TitleOverride = "Custom title",
            SongNumber = 1,
            PlayedSong = song1,
            IsWorldPremiere = false,
            IsPlayedFromRecording = false,
            IsRotationSong = true
        };
        entriesRepo.Add(entryCustomTitle);
        
        var entrySong = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist,
            ExtraNotes = "entry with default song title",
            SongNumber = 2,
            PlayedSong = song2,
            IsWorldPremiere = false,
            IsPlayedFromRecording = false,
            IsRotationSong = true
        };
        entriesRepo.Add(entrySong);
        
        var entrySongPiano = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist,
            ExtraNotes = "entry with default song variant name",
            SongNumber = 3,
            PlayedSongVariant = song2Piano,
            IsWorldPremiere = false,
            IsPlayedFromRecording = false,
            IsRotationSong = true
        };
        entriesRepo.Add(entrySongPiano);

        await repo.SaveChangesAsync();
        
        // retrieve the setlist and validate the result
        var retrievedSetlist = await repo.GetByPrimaryKeyAsync(setlist.Id);
        Assert.NotNull(retrievedSetlist);
        Assert.Equal(setlist.Id, retrievedSetlist.Id);
        Assert.Equal(setlist.ConcertId, retrievedSetlist.ConcertId);
        Assert.Equal(setlist.LinkinpediaUrl, retrievedSetlist.LinkinpediaUrl);
        Assert.Equal(setlist.ConcertTitle, retrievedSetlist.ConcertTitle);
        Assert.Equal(setlist.SetName, retrievedSetlist.SetName);
        
        Assert.NotNull(retrievedSetlist.Entries);
        var entriesList = retrievedSetlist.Entries
            .OrderBy(e => e.SortNumber)
            .ThenBy(e => e.SongNumber)
            .ToList();
        Assert.Equal(3, entriesList.Count);
        AssertSetlistEntry(entryCustomTitle, entriesList[0]);
        AssertSetlistEntry(entrySong, entriesList[1]);
        AssertSetlistEntry(entrySongPiano, entriesList[2]);
        
        // clean up
        repo.Delete(retrievedSetlist);
        
        await repo.SaveChangesAsync();
        
        retrievedSetlist = await repo.GetByPrimaryKeyAsync(setlist.Id);
        Assert.Null(retrievedSetlist);
    }


    private static void AssertSetlistEntry(SetlistEntryDo expected, SetlistEntryDo actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.PlayedSong?.Title, actual.PlayedSong?.Title);
        Assert.Equal(expected.PlayedSong?.LinkinpediaUrl, actual.PlayedSong?.LinkinpediaUrl);
        Assert.Equal(expected.PlayedSong?.Isrc, actual.PlayedSong?.Isrc);
        Assert.Equal(expected.ExtraNotes, actual.ExtraNotes);
        Assert.Equal(expected.TitleOverride, actual.TitleOverride);
        Assert.Equal(expected.SongNumber, actual.SongNumber);
        Assert.Equal(expected.IsWorldPremiere, actual.IsWorldPremiere);
        Assert.Equal(expected.IsPlayedFromRecording, actual.IsPlayedFromRecording);
        Assert.Equal(expected.IsRotationSong, actual.IsRotationSong);
        
        Assert.Equal(expected.PlayedSongVariant?.Song.Id, actual.PlayedSongVariant?.Song.Id);
        Assert.Equal(expected.PlayedSongVariant?.VariantName, actual.PlayedSongVariant?.VariantName);
        Assert.Equal(expected.PlayedSongVariant?.Description, actual.PlayedSongVariant?.Description);
        Assert.Equal(expected.PlayedSongVariant?.IsrcOverride, actual.PlayedSongVariant?.IsrcOverride);
        
        // check act
        Assert.Equal(expected.Act?.ActNumber, actual.Act?.ActNumber);
        Assert.Equal(expected.Act?.SetlistId, actual.Act?.SetlistId);
        Assert.Equal(expected.Act?.Title, actual.Act?.Title);
    }


    [Fact]
    public async Task GetByConcertIdAsync()
    {
        var repo = new SqlSetlistRepository(DbContext);
        var entriesRepo = new SqlSetlistEntryRepository(DbContext);

        var setlist1 = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            ConcertTitle = "Setlist 1",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        var setlist2 = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            ConcertTitle = "Setlist 2",
            SetName = "Set B1",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(setlist1);
        repo.Add(setlist2);
        
        var song1 = new SongDo
        {
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        var entry1 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist1,
            ExtraNotes = "Notes for this entry",
            TitleOverride = "Custom title",
            SongNumber = 1,
            PlayedSong = song1,
            IsWorldPremiere = false,
            IsPlayedFromRecording = false,
            IsRotationSong = true
        };
        entriesRepo.Add(entry1);

        await repo.SaveChangesAsync();
        
        var retrievedSetlist = await repo.GetByConcertIdAsync(setlist1.ConcertId).FirstOrDefaultAsync();
        Assert.NotNull(retrievedSetlist);
        Assert.Equal(setlist1.Id, retrievedSetlist.Id);
        Assert.Equal(setlist1.ConcertId, retrievedSetlist.ConcertId);
        Assert.Equal(setlist1.LinkinpediaUrl, retrievedSetlist.LinkinpediaUrl);
        Assert.Equal(setlist1.ConcertTitle, retrievedSetlist.ConcertTitle);
        Assert.Equal(setlist1.SetName, retrievedSetlist.SetName);
        Assert.NotNull(retrievedSetlist.Entries);
        Assert.Single(retrievedSetlist.Entries);

        var retrievedSong = retrievedSetlist.Entries.FirstOrDefault()?.PlayedSong;
        Assert.NotNull(retrievedSong);
        Assert.Equal(song1.Title, retrievedSong.Title);
        
        retrievedSetlist = await repo.GetByConcertIdAsync(setlist2.ConcertId).FirstOrDefaultAsync();
        Assert.NotNull(retrievedSetlist);
        Assert.NotNull(retrievedSetlist.Entries);
        Assert.Empty(retrievedSetlist.Entries);
        Assert.Equal(setlist2.Id, retrievedSetlist.Id);
        Assert.Equal(setlist2.ConcertId, retrievedSetlist.ConcertId);
        Assert.Equal(setlist2.LinkinpediaUrl, retrievedSetlist.LinkinpediaUrl);
        Assert.Equal(setlist2.ConcertTitle, retrievedSetlist.ConcertTitle);
        Assert.Equal(setlist2.SetName, retrievedSetlist.SetName);
    }
    
    [Fact]
    public async Task GetByConcertIdAsync_WithAct()
    {
        var repo = new SqlSetlistRepository(DbContext);
        var entriesRepo = new SqlSetlistEntryRepository(DbContext);
        var actRepo = new SqlSetlistActRepository(DbContext);
        
        var setlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            ConcertTitle = "Setlist 1",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(setlist);
        
        var song1 = new SongDo
        {
            Title = "QWERTY",
            Isrc = "5355646",
            LinkinpediaUrl = "https://linkinpedia.com/wiki/QWERTY"
        };
        var entry1 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist,
            ExtraNotes = "Notes for this entry",
            TitleOverride = "Custom title",
            SongNumber = 1,
            PlayedSong = song1,
            IsWorldPremiere = false,
            IsPlayedFromRecording = false,
            IsRotationSong = true
        };

        var act1 = new SetlistActDo
        {
            SetlistId = setlist.Id,
            ActNumber = 1,
            Title = "Act 1 Title",
            Setlist = setlist
        };

        entry1.ActNumber = act1.ActNumber;
        
        actRepo.Add(act1);
        entriesRepo.Add(entry1);

        await repo.SaveChangesAsync();
        
        var retrievedSetlist = await repo.GetByConcertIdAsync(setlist.ConcertId).FirstOrDefaultAsync();
        Assert.NotNull(retrievedSetlist);
        Assert.Equal(setlist.Id, retrievedSetlist.Id);
        Assert.Equal(setlist.ConcertId, retrievedSetlist.ConcertId);
        Assert.Equal(setlist.LinkinpediaUrl, retrievedSetlist.LinkinpediaUrl);
        Assert.Equal(setlist.ConcertTitle, retrievedSetlist.ConcertTitle);
        Assert.Equal(setlist.SetName, retrievedSetlist.SetName);
        Assert.NotNull(retrievedSetlist.Entries);
        Assert.Single(retrievedSetlist.Entries);
        
        var retrievedAct = retrievedSetlist.Entries.FirstOrDefault()?.Act;
        Assert.NotNull(retrievedAct);
        Assert.Equal(act1.Title, retrievedAct.Title);
        Assert.Equal(act1.SetlistId, retrievedAct.SetlistId);
        Assert.Equal(act1.ActNumber, retrievedAct.ActNumber);

        var retrievedSong = retrievedSetlist.Entries.FirstOrDefault()?.PlayedSong;
        Assert.NotNull(retrievedSong);
        Assert.Equal(song1.Title, retrievedSong.Title);
    }
}