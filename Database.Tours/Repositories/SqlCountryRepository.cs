using Common.Datbase.MySql.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public class SqlCountryRepository(ToursDbContext dbContext) : SingleKeySqlRepositoryBase<CountryDo, string>(dbContext, dbContext.Countries), ICountryRepository
{
    protected override Task<CountryDo> LoadReferences(CountryDo dataObject)
    {
        return Task.FromResult(dataObject);
    }
}