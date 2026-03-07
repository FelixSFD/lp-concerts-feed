using Database.Setlists.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.Repositories;

public class SqlSetlistActRepository(SetlistsDbContext dbContext)
    : SqlRepositoryBase<SetlistActDo>(dbContext, dbContext.SetlistActs), ISetlistActRepository
{
    /// <inheritdoc/>
    public async Task<SetlistActDo?> GetBy(uint setlistId, uint actNumber)
    {
        return await DbSet.FindAsync(setlistId, actNumber);
    }
}