using Common.Database;
using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

/// <summary>
/// Repository to manage concerts
/// </summary>
public interface IConcertRepository : ISingleKeyRepositoryBase<ConcertDo, string>, IRepositoryBase<ConcertDo>
{
    /// <summary>
    /// Returns a (filtered) list of concerts
    /// </summary>
    /// <param name="token">Token to delete the request</param>
    /// <param name="countryCode">Filter by country code</param>
    /// <param name="paginationParams">Parameters for paging</param>
    /// <param name="orderBy">list of <see cref="SortDescriptor"/>s to control the order of the returned items</param>
    /// <param name="includeDeleted">true, if concerts that are marked as "deleted" should be returned, too. (default: false)</param>
    /// <returns>List of concerts</returns>
    IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, string? countryCode = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false);
}