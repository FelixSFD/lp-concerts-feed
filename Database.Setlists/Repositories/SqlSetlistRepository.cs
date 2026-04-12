using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public class SqlSetlistRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SetlistDo, uint>(dbContext, dbContext.Setlists), ISetlistRepository
{
    /// <inheritdoc/>
    protected override async Task<SetlistDo> LoadReferences(SetlistDo dataObject)
    {
        await Context.Entry(dataObject)
            .Collection(e => e.Entries)
            .Query()
            .Include(e => e.Act)
            .LoadAsync();
        return dataObject;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<SetlistDo> GetByConcertIdAsync(string concertId)
    {
        return DbSet
            .AsQueryable()
            .Where(sl => sl.ConcertId == concertId)
            .Include(sl => sl.Entries)
            .ThenInclude(entry => entry.Act)
            .Include(sl => sl.Entries)
            .ThenInclude(entry => entry.PlayedSong)
            .Include(sl => sl.Entries)
            .ThenInclude(entry => entry.PlayedSongVariant)
            .ThenInclude(variant => variant!.Song)
            .Include(sl => sl.Entries)
            .ThenInclude(entry => entry.PlayedMashup)
            .AsAsyncEnumerable();
    }
}