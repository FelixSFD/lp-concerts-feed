using System.Diagnostics;
using Common.Database.MySql.Repositories;
using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public class SqlSongRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SongDo, uint>(dbContext, dbContext.Songs), ISongRepository
{
    private static readonly ActivitySource ActivitySource =
        new("LPshows.Performance");
    
    /// <inheritdoc/>
    protected override async Task<SongDo> LoadReferences(SongDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(s => s.Album)
            .LoadAsync();
        
        return dataObject;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<SongDo> GetSongsByIds(params uint[] songIds)
    {
        var idList = songIds.ToList();
        return DbSet
            .AsQueryable()
            .Where(s => idList.Contains(s.Id))
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<SongDo> GetSongsByTitle(string title)
    {
        return DbSet
            .AsQueryable()
            .Where(s => s.Title == title)
            .ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public async Task<IDictionary<SongDo, uint>> GetSongsPlayedAtConcertsAsync(IEnumerable<string> concertIds,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity();
        var concertIdList = concertIds.ToList();
        activity?.SetTag("numberOfConcerts", concertIdList.Count);

        var query = dbContext
            .Setlists
            .AsQueryable()
            .Where(s => concertIdList.Contains(s.ConcertId))
            .Include(s => s.Entries)
            .SelectMany(s => s.Entries)
            .Where(e => !e.IsPlayedFromRecording)
            .Include(e => e.PlayedSong)
                .ThenInclude(s => s!.Album)
            .Include(e => e.PlayedSongVariant)
                .ThenInclude(v => v!.Song)
                .ThenInclude(s => s.Album);
        
        var entries = await query.ToListAsync(cancellationToken);
        return entries
            .Select(entry => entry.PlayedSong ?? entry.PlayedSongVariant?.Song)
            .Where(song => song is not null)
            .Cast<SongDo>()
            .GroupBy(song => song.Id)
            .ToDictionary(
                group => group.First(),
                group => (uint)group.Count());
    }

    /// <inheritdoc/>
    public async Task<SongConcertPlayData> GetSongPlaysByConcertAsync(IEnumerable<string> concertIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(concertIds);
        cancellationToken.ThrowIfCancellationRequested();
        var ids = concertIds.Distinct().ToArray();
        if (ids.Length == 0)
            return new SongConcertPlayData([], []);

        // Coverage must be queried independently: an empty or recording-only setlist is still present.
        var coveredConcertIds = await dbContext.Setlists
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(setlist => ids.Contains(setlist.ConcertId))
            .Select(setlist => setlist.ConcertId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var entries = await dbContext.SetlistEntries
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(entry => ids.Contains(entry.Setlist.ConcertId) && !entry.IsPlayedFromRecording)
            .Include(entry => entry.Setlist)
            .Include(entry => entry.PlayedSong).ThenInclude(song => song!.Album)
            .Include(entry => entry.PlayedSongVariant).ThenInclude(variant => variant!.Song)
                .ThenInclude(song => song.Album)
            .ToListAsync(cancellationToken);

        var plays = entries
            .Select(entry => new { entry.Setlist.ConcertId, Song = entry.PlayedSong ?? entry.PlayedSongVariant?.Song })
            .Where(entry => entry.Song is not null)
            .GroupBy(entry => (entry.ConcertId, SongId: entry.Song!.Id))
            .Select(group => new SongConcertPlayCount(group.First().Song!, group.Key.ConcertId, checked((uint)group.Count())))
            .ToArray();

        return new SongConcertPlayData(coveredConcertIds, plays);
    }
}