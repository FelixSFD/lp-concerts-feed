using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public class SqlSetlistRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SetlistDo, uint>(dbContext, dbContext.Setlists), ISetlistRepository
{
    /// <inheritdoc/>
    public async Task<SetlistDo?> GetByConcertIdAsync(string concertId)
    {
        return await DbSet
            .AsQueryable()
            .Where(sl => sl.ConcertId == concertId)
            .FirstOrDefaultAsync();
    }
}