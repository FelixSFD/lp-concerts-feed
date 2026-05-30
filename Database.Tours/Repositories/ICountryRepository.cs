using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public interface ICountryRepository : ISingleKeyRepositoryBase<CountryDo, string>
{
}