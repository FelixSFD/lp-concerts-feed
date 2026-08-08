using Common.Database;
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
    
    /// <summary>
    /// Returns a (filtered) list of concerts
    /// </summary>
    /// <param name="token">Token to delete the request</param>
    /// <param name="countryCode">Filter by country code</param>
    /// <param name="paginationParams">Parameters for paging</param>
    /// <param name="orderBy">list of <see cref="SortDescriptor"/>s to control the order of the returned items</param>
    /// <returns>List of concerts</returns>
    IAsyncEnumerable<CityDo> GetCities(CancellationToken token, string? countryCode = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null);
}