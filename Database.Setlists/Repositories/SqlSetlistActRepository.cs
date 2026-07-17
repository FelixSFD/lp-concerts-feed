using Common.Datbase.MySql.Repositories;
using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlSetlistActRepository(SetlistsDbContext dbContext)
    : SqlRepositoryBase<SetlistActDo>(dbContext, dbContext.SetlistActs), ISetlistActRepository
{
    /// <inheritdoc/>
    protected override Task<SetlistActDo> LoadReferences(SetlistActDo dataObject)
    {
        return Task.FromResult(dataObject);
    }

    /// <inheritdoc/>
    public async Task<SetlistActDo?> GetBy(uint setlistId, uint actNumber)
    {
        return await DbSet.FindAsync(setlistId, actNumber);
    }
}