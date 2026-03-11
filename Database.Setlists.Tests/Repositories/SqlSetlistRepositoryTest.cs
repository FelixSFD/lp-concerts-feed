using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSetlistRepositoryTest : DbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var repo = new SqlSetlistRepository(DbContext);

        var setlist = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
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
        
        repo.Delete(retrievedSetlist);
        
        await repo.SaveChangesAsync();
        
        retrievedSetlist = await repo.GetByPrimaryKeyAsync(setlist.Id);
        Assert.Null(retrievedSetlist);
    }


    [Fact]
    public async Task GetByConcertIdAsync()
    {
        var repo = new SqlSetlistRepository(DbContext);
        var entriesRepo = new SqlSetlistEntryRepository(DbContext);

        var setlist1 = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
            LinkinpediaUrl = "https://lplive.net"
        };
        
        var setlist2 = new SetlistDo
        {
            ConcertId = Guid.NewGuid().ToString(),
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
    }
}