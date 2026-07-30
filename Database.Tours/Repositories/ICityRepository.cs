using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public interface ICityRepository : IRepositoryBase<CityDo>
{
    /// <summary>
    /// Returns a city by its primary key
    /// </summary>
    /// <param name="countryCode">ISO code of the country this state is a part of</param>
    /// <param name="cityId">ID of this city</param>
    /// <returns>City or null if no city matching the keys was found</returns>
    public Task<CityDo?> GetByPrimaryKeyAsync(string countryCode, uint cityId);
}