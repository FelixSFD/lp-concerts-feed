namespace Common.Database.Repositories;

/// <summary>
/// Parameters for pagination in repositories.
/// Usage:
/// Start with <see cref="Skip"/> 0 and a page size <see cref="Take"/>.
/// The next page should then skip the amount of entries returned in the first query.
/// </summary>
public interface IPaginationParams
{
    /// <summary>
    /// Number of results to skip
    /// </summary>
    public uint Skip { get; set; }
    
    /// <summary>
    /// Number of results to return
    /// </summary>
    public uint Take { get; set; }
}


public static class PaginationParamsExtensions
{
    /// <summary>
    /// Applies the <paramref name="paginationParams"/> to the query
    /// </summary>
    /// <param name="query"></param>
    /// <param name="paginationParams"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public static IQueryable<TItem> ApplyPagination<TItem>(this IQueryable<TItem> query, IPaginationParams? paginationParams = null) where TItem : class
    {
        paginationParams ??= new PaginationParams(0, 100);
        return query
            .Skip((int)paginationParams.Skip)
            .Take((int)paginationParams.Take);
    }
}