using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlCityRepository(ToursDbContext dbContext) : SqlRepositoryBase<CityDo>(dbContext, dbContext.Cities), ICityRepository
{
    public async Task<CityDo?> GetByPrimaryKeyAsync(string countryCode, uint cityId)
    {
        return await DbSet.FindAsync(countryCode, cityId);
    }
}