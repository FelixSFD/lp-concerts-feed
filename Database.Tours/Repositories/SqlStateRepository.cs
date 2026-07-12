using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Repositories;

public class SqlStateRepository(ToursDbContext dbContext) : SqlRepositoryBase<StateDo>(dbContext, dbContext.States), IStateRepository
{
    /// <inheritdoc/>
    public async Task<StateDo?> GetByPrimaryKeyAsync(string countryCode, string stateCode)
    {
        return await DbSet
            .Include(state => state.Country)
            .FirstOrDefaultAsync(s => s.CountryCode == countryCode && s.Code == stateCode);
    }
    
    /// <inheritdoc/>
    public async Task<StateDo?> GetByPrimaryKeyWithoutCountryAsync(string countryCode, string stateCode)
    {
        return await DbSet.FindAsync(countryCode, stateCode);
    }
}