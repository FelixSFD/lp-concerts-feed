using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

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
        return await DbSet.FindAsync(cityId, countryCode);
    }
}