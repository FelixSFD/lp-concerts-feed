using Common.Database.DataObjects;
using Common.Database.Filter;

namespace Common.Database.Repositories;

/// <summary>
/// Basic repository for <see name="BaseDo"/>s
/// </summary>
/// <typeparam name="TDataObject">Type of object to use in the repository</typeparam>
public interface IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    /// <summary>
    /// Adds the data object to the repository
    /// </summary>
    /// <param name="data"></param>
    public void Add(TDataObject data);
    
    /// <summary>
    /// Updates the data object in the repository
    /// </summary>
    /// <param name="data"></param>
    public void Update(TDataObject data);
    
    /// <summary>
    /// Deletes the data object from the repository
    /// </summary>
    /// <param name="data"></param>
    public void Delete(TDataObject data);
    
    [Obsolete("Use FindAsync() instead")]
    public IAsyncEnumerable<TDataObject> QueryAsync(CancellationToken token);

    /// <summary>
    /// Returns a list of data objects that match the filter in a paginated manner. Metadata about the pagination is returned as well.
    /// </summary>
    /// <param name="filter">Filter for the objects</param>
    /// <param name="orderBy">Order by</param>
    /// <param name="paginationParams">Pagination parameters</param>
    /// <param name="includeDeleted">Include deleted objects</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns></returns>
    public Task<PaginatedQueryResult<TDataObject>> FindPaginatedAsync(IQueryFilter<TDataObject>? filter = null,
        IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null,
        bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of data objects that match the filter in a paginated manner, but without returning metadata about the pagination.
    /// </summary>
    /// <param name="filter">Filter for the objects</param>
    /// <param name="orderBy">Order by</param>
    /// <param name="paginationParams">Pagination parameters</param>
    /// <param name="includeDeleted">Include deleted objects</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns></returns>
    IAsyncEnumerable<TDataObject> FindAsync(IQueryFilter<TDataObject>? filter = null,
        IEnumerable<SortDescriptor>? orderBy = null, IPaginationParams? paginationParams = null,
        bool includeDeleted = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save the changes to the repository
    /// </summary>
    /// <param name="token">Token to cancel the operation</param>
    /// <returns></returns>
    public Task SaveChangesAsync(CancellationToken token = default);
}