using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;

namespace Database.Setlists.Tests.Repositories;


public class SqlSongRepositoryTest : SetlistsDbIntegrationTestsBase
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
            AppleMusicId = "1234567890234",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(song);

        await repo.SaveChangesAsync();
        
        var retrievedSong = await repo.GetByPrimaryKeyAsync(song.Id);
        Assert.NotNull(retrievedSong);
        Assert.Equal(retrievedSong.Id, song.Id);
        Assert.Equal(retrievedSong.Title, song.Title);
        Assert.Equal(retrievedSong.Isrc, song.Isrc);
        Assert.Equal(retrievedSong.AppleMusicId, song.AppleMusicId);
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

    [Fact]
    public async Task TestWithoutAlbum()
    {
        var repo = new SqlSongRepository(DbContext);
        
        var song = new SongDo
        {
            Title = "Song without album",
            Isrc = "1234",
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
        Assert.Null(retrievedAlbum);
    }
    
    [Fact]
    public async Task GetSongsByTitle()
    {
        var repo = new SqlSongRepository(DbContext);
        
        var song1 = new SongDo
        {
            Title = "Song without album",
            Isrc = "1234",
            LinkinpediaUrl = "https://lplive.net"
        };
        var song2 = new SongDo
        {
            Title = "Song 2",
            Isrc = "56352",
            LinkinpediaUrl = "https://lplive.net"
        };
        
        repo.Add(song1);
        repo.Add(song2);
        await repo.SaveChangesAsync();
        
        var retrievedSongs = await repo.GetSongsByTitle(song2.Title).ToArrayAsync();
        Assert.Single(retrievedSongs);
        
        var retrievedSong = retrievedSongs[0];
        Assert.NotNull(retrievedSong);
        Assert.Equal(song2.Id, retrievedSong.Id);
        Assert.Equal(song2.Title, retrievedSong.Title);
        Assert.Equal(song2.Isrc, retrievedSong.Isrc);
        Assert.Equal(song2.LinkinpediaUrl, retrievedSong.LinkinpediaUrl);
        
        var retrievedAlbum = retrievedSong.Album;
        Assert.Null(retrievedAlbum);
    }

    [Fact]
    public async Task GetSongsByIds_ReturnsMatchingSongs()
    {
        var repo = new SqlSongRepository(DbContext);

        var song1 = new SongDo
        {
            Title = "Song one",
            Isrc = "1111",
            LinkinpediaUrl = "https://lplive.net"
        };
        var song2 = new SongDo
        {
            Title = "Song two",
            Isrc = "2222",
            LinkinpediaUrl = "https://lplive.net"
        };
        var song3 = new SongDo
        {
            Title = "Song three",
            Isrc = "3333",
            LinkinpediaUrl = "https://lplive.net"
        };

        repo.Add(song1);
        repo.Add(song2);
        repo.Add(song3);
        await repo.SaveChangesAsync();

        var retrievedSongs = await repo.GetSongsByIds(song1.Id, song3.Id).ToArrayAsync();

        Assert.Equal(2, retrievedSongs.Length);
        Assert.Contains(retrievedSongs, s => s.Id == song1.Id);
        Assert.Contains(retrievedSongs, s => s.Id == song3.Id);
        Assert.DoesNotContain(retrievedSongs, s => s.Id == song2.Id);
    }

    [Fact]
    public async Task GetSongsByIds_ReturnsEmptyWhenNoIdsMatch()
    {
        var repo = new SqlSongRepository(DbContext);

        var song = new SongDo
        {
            Title = "Song",
            Isrc = "9876",
            LinkinpediaUrl = "https://lplive.net"
        };

        repo.Add(song);
        await repo.SaveChangesAsync();

        var retrievedSongs = await repo.GetSongsByIds(uint.MaxValue).ToArrayAsync();

        Assert.Empty(retrievedSongs);
    }

    [Fact]
    public async Task GetSongsByTitle_ReturnsAllMatchingSongs()
    {
        var repo = new SqlSongRepository(DbContext);

        var matchingSong1 = new SongDo
        {
            Title = "Duplicate title",
            Isrc = "1234",
            LinkinpediaUrl = "https://lplive.net"
        };
        var matchingSong2 = new SongDo
        {
            Title = "Duplicate title",
            Isrc = "5678",
            LinkinpediaUrl = "https://lplive.net"
        };
        var otherSong = new SongDo
        {
            Title = "Different title",
            Isrc = "9012",
            LinkinpediaUrl = "https://lplive.net"
        };

        repo.Add(matchingSong1);
        repo.Add(matchingSong2);
        repo.Add(otherSong);
        await repo.SaveChangesAsync();

        var retrievedSongs = await repo.GetSongsByTitle("Duplicate title").ToArrayAsync();

        Assert.Equal(2, retrievedSongs.Length);
        Assert.Contains(retrievedSongs, s => s.Id == matchingSong1.Id);
        Assert.Contains(retrievedSongs, s => s.Id == matchingSong2.Id);
        Assert.DoesNotContain(retrievedSongs, s => s.Id == otherSong.Id);
    }

    [Fact]
    public async Task GetSongsPlayedAtConcertsAsync_CountsSongsAcrossConcerts()
    {
        var repo = new SqlSongRepository(DbContext);
        var setlistRepo = new SqlSetlistRepository(DbContext);
        var entriesRepo = new SqlSetlistEntryRepository(DbContext);

        var song1 = new SongDo
        {
            Title = "Song one",
            Isrc = "1111",
            LinkinpediaUrl = "https://lplive.net"
        };
        var song2 = new SongDo
        {
            Title = "Song two",
            Isrc = "2222",
            LinkinpediaUrl = "https://lplive.net"
        };
        var songVariant = new SongVariantDo
        {
            Song = song2,
            SongId = song2.Id,
            VariantName = "Live version",
            Description = "Live version",
            IsrcOverride = "3333"
        };
        var song3 = new SongDo
        {
            Title = "Song three",
            Isrc = "3333",
            LinkinpediaUrl = "https://lplive.net"
        };

        repo.Add(song1);
        repo.Add(song2);
        repo.Add(song3);
        await repo.SaveChangesAsync();

        songVariant.SongId = song2.Id;

        var concertId1 = Guid.NewGuid().ToString();
        var concertId2 = Guid.NewGuid().ToString();
        var otherConcertId = Guid.NewGuid().ToString();

        var setlist1 = new SetlistDo
        {
            ConcertId = concertId1,
            ConcertTitle = "Concert 1",
            ConcertType = "Linkin Park",
            ConcertDate = DateTime.Today,
            SetName = "Set A"
        };
        var setlist2 = new SetlistDo
        {
            ConcertId = concertId2,
            ConcertTitle = "Concert 2",
            ConcertType = "Linkin Park",
            ConcertDate = DateTime.Today.AddDays(1),
            SetName = "Set B"
        };
        var setlist3 = new SetlistDo
        {
            ConcertId = otherConcertId,
            ConcertTitle = "Concert 3",
            ConcertType = "Linkin Park",
            ConcertDate = DateTime.Today.AddDays(2),
            SetName = "Set C"
        };

        setlistRepo.Add(setlist1);
        setlistRepo.Add(setlist2);
        setlistRepo.Add(setlist3);
        await setlistRepo.SaveChangesAsync();

        var entry1 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist1,
            SongNumber = 1,
            PlayedSong = song1,
            IsRotationSong = false,
            IsPlayedFromRecording = false,
            IsWorldPremiere = false,
            IsLivePremiere = false
        };
        var entry2 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist1,
            SongNumber = 2,
            PlayedSong = song1,
            IsRotationSong = false,
            IsPlayedFromRecording = false,
            IsWorldPremiere = false,
            IsLivePremiere = false
        };
        var entry3 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist2,
            SongNumber = 1,
            PlayedSongVariant = songVariant,
            IsRotationSong = false,
            IsPlayedFromRecording = false,
            IsWorldPremiere = false,
            IsLivePremiere = false
        };
        var entry4 = new SetlistEntryDo
        {
            Id = Guid.NewGuid().ToString(),
            Setlist = setlist3,
            SongNumber = 1,
            PlayedSong = song1,
            IsRotationSong = false,
            IsPlayedFromRecording = false,
            IsWorldPremiere = false,
            IsLivePremiere = false
        };

        entriesRepo.Add(entry1);
        entriesRepo.Add(entry2);
        entriesRepo.Add(entry3);
        entriesRepo.Add(entry4);
        await entriesRepo.SaveChangesAsync();

        var statistics = await repo.GetSongsPlayedAtConcertsAsync([concertId1, concertId2]);

        Assert.Equal(2, statistics.Count);
        Assert.Equal(2u, statistics[song1]);
        Assert.Equal(1u, statistics[song2]);
        Assert.DoesNotContain(statistics, kv => kv.Key.Id == song3.Id);
    }

    [Fact]
    public async Task GetSongsPlayedAtConcertsAsync_ReturnsEmptyDictionaryWhenNoConcertMatches()
    {
        var repo = new SqlSongRepository(DbContext);

        var song = new SongDo
        {
            Title = "Song",
            Isrc = "9876",
            LinkinpediaUrl = "https://lplive.net"
        };

        repo.Add(song);
        await repo.SaveChangesAsync();

        var statistics = await repo.GetSongsPlayedAtConcertsAsync(new[] { Guid.NewGuid().ToString() });

        Assert.Empty(statistics);
    }
}