using Common.Database;
using Common.Database.DataObjects;
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
    [Obsolete("Use ConcertFilter overload instead")]
    IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, string? countryCode = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false);
    
    /// <summary>
    /// Returns a (filtered) list of concerts
    /// </summary>
    /// <param name="token">Token to delete the request</param>
    /// <param name="filter">Filter for the list of concerts</param>
    /// <param name="paginationParams">Parameters for paging</param>
    /// <param name="orderBy">list of <see cref="SortDescriptor"/>s to control the order of the returned items</param>
    /// <param name="includeDeleted">true, if concerts that are marked as "deleted" should be returned, too. (default: false)</param>
    /// <returns>List of concerts</returns>
    IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, ConcertFilter? filter = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false);
    
    [Obsolete("Use Database.Tours.Filters.ConcertFilter instead")]
    Task<PaginatedQueryResult<ConcertDo>> GetConcertsAsync(CancellationToken token, ConcertFilter? filter, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns a filtered list of concerts with pagination and sorting
    /// </summary>
    /// <param name="token">Token to cancel the operation</param>
    /// <param name="filter">Filter to apply to the query</param>
    /// <param name="orderBy">sorting</param>
    /// <param name="paginationParams">pagination options</param>
    /// <param name="includeDeleted">true, if deleted concerts should be returned, too</param>
    /// <returns></returns>
    Task<PaginatedQueryResult<ConcertDo>> GetConcertsAsync(CancellationToken token, Filters.ConcertFilter? filter = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false);
    
    /// <summary>
    /// Returns a list of concerts that are linked to a given Linkinpedia page. Ideally, this should only return one concert, but there is technically no unique key.
    /// </summary>
    /// <param name="wikiPageId">ID of the page in Linkinpedia</param>
    /// <returns></returns>
    IAsyncEnumerable<ConcertDo> GetConcertsByWikiPageId(string wikiPageId);
    
    /// <summary>
    /// Starts a query that returns all concerts with their references. The query can be filtered.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    IAsyncEnumerable<ConcertDo> FindAllWithReferencesAsync(CancellationToken token);
}

[Obsolete("Use Database.Tours.Filters.ConcertFilter instead")]
public class ConcertFilter
{
    public string? CountryCode { get; set; }
    public DateTimeOffset? Before { get; set; }
    public DateTimeOffset? After { get; set; }
}