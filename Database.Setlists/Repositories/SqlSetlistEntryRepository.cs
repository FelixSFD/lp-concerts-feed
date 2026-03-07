using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

public class SqlSetlistEntryRepository(SetlistsDbContext dbContext)
    : SingleKeySqlRepositoryBase<SetlistEntryDo, string>(dbContext, dbContext.SetlistEntries), ISetlistEntryRepository
{
}