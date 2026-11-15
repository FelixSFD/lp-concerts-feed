using Common.Database;
using Common.Database.DataObjects;
using Common.Database.Repositories;
using Database.Tours.DataObjects;

namespace Database.Tours.Repositories;

/// <summary>
/// Repository to manage concerts
/// </summary>
public interface IConcertRepository : ISingleKeyRepositoryBase<ConcertDo, string>, IRepositoryBase<ConcertDo>, ITimestampedRepository<ConcertDo>, ISoftDeletableRepository<ConcertDo>
{
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
    /// Returns a filtered list of concerts with pagination and sorting but not returning paging metadata
    /// </summary>
    /// <param name="token">Token to cancel the operation</param>
    /// <param name="filter">Filter to apply to the query</param>
    /// <param name="orderBy">sorting</param>
    /// <param name="paginationParams">pagination options</param>
    /// <param name="includeDeleted">true, if deleted concerts should be returned, too</param>
    /// <returns></returns>
    IAsyncEnumerable<ConcertDo> GetConcerts(CancellationToken token, Filters.ConcertFilter? filter = null, IEnumerable<SortDescriptor>? orderBy = null,
        IPaginationParams? paginationParams = null, bool includeDeleted = false);
    
    /// <summary>
    /// Returns all concert that happened on a given day and month, including previous years
    /// </summary>
    /// <param name="token"></param>
    /// <param name="month"></param>
    /// <param name="day"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    IAsyncEnumerable<ConcertDo> GetOnThisDay(CancellationToken token, int month, int day, IEnumerable<SortDescriptor>? orderBy = null);
    
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

    IAsyncEnumerable<ConcertDo> GetUpcomingBookmarkedConcertsForAsync(string userId,
        UserConcertBookmarkDo.BookmarkStatus status, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns all concerts, a given user has attended. This will not return concerts that are happening in the future.
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns></returns>
    IAsyncEnumerable<ConcertDo> GetAttendedConcertsForAsync(string userId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns all <see name="ConcertDo" /> that match the IDs
    /// </summary>
    /// <param name="ids">Primary keys of all entries that should be returned</param>
    /// <returns></returns>
    public IAsyncEnumerable<ConcertDo> GetByIds(IEnumerable<string> ids);
}
