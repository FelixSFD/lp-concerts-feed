using Common.Database.MySql.Repositories;
using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public class SqlSongRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SongDo, uint>(dbContext, dbContext.Songs), ISongRepository
{
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

    public async Task<IDictionary<SongDo, uint>> GetSongsPlayedAtConcertsAsync(IEnumerable<string> concertIds,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext
            .Setlists
            .AsQueryable()
            .Where(s => concertIds.Contains(s.ConcertId))
            .Include(s => s.Entries)
            .SelectMany(s => s.Entries)
            .Include(e => e.PlayedSong)
            .Include(e => e.PlayedSongVariant)
            .Select(e => e);

        var entries = await query.ToListAsync(cancellationToken);
        return entries
            .Select(entry => entry.PlayedSong ?? entry.PlayedSongVariant?.Song)
            .Where(song => song is not null)
            .Cast<SongDo>()
            .GroupBy(song => song)
            .ToDictionary(
                group => group.Key,
                group => (uint)group.Count());
    }
}