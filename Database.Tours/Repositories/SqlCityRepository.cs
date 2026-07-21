using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Repositories;

public class SqlCityRepository(ToursDbContext dbContext) : SqlRepositoryBase<CityDo>(dbContext, dbContext.Cities), ICityRepository
{
    protected override async Task<CityDo> LoadReferences(CityDo dataObject)
    {
        await Context.Entry(dataObject)
            .Reference(v => v.Country)
            .LoadAsync();
        await Context.Entry(dataObject)
            .Reference(v => v.State)
            .LoadAsync();
        
        return dataObject;
    }
    
    public async Task<CityDo?> GetByPrimaryKeyAsync(string countryCode, uint cityId)
    {
        return await DbSet
            .Include(c => c.Country)
            .Include(c => c.State)
            .FirstOrDefaultAsync(c => c.CountryCode == countryCode && c.Id == cityId);
    }
    
    public async Task<CityDo?> GetByPrimaryKeyWithoutReferencesAsync(string countryCode, uint cityId)
    {
        return await DbSet.FindAsync(cityId, countryCode);
    }
}