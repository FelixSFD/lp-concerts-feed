using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlStateRepository(ToursDbContext dbContext) : SqlRepositoryBase<StateDo>(dbContext, dbContext.States), IStateRepository
{
    public async Task<StateDo?> GetByPrimaryKeyAsync(string countryCode, string stateCode)
    {
        return await DbSet.FindAsync(countryCode, stateCode);
    }
}