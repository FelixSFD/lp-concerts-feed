using Common.Database;
using Common.Database.DataObjects;
using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

public interface ICityRepository : IRepositoryBase<CityDo>, ITimestampedRepository<CityDo>, ISoftDeletableRepository<CityDo>
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
    
    /// <summary>
    /// Returns a filtered list of cities with pagination and sorting
    /// </summary>
    /// <param name="token">Token to cancel the operation</param>
    /// <param name="filter">Filter to apply to the query</param>
    /// <param name="orderBy">sorting</param>
    /// <param name="paginationParams">pagination options</param>
    /// <param name="includeDeleted">true, if deleted cities should be returned, too</param>
    /// <returns></returns>
    Task<PaginatedQueryResult<CityDo>> GetCitiesAsync(CancellationToken token, Filters.CityFilter? filter = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false);
}