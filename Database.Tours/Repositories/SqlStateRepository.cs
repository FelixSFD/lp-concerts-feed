using Common.Database.MySql.Repositories;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Repositories;

public class SqlStateRepository(ToursDbContext dbContext) : SqlRepositoryBase<StateDo>(dbContext, dbContext.States), IStateRepository
{
    protected override async Task<StateDo> LoadReferences(StateDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(v => v.Country)
            .LoadAsync();
        
        return dataObject;
    }
    
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