namespace Common.Database.Filter;

/// <summary>
/// Filter for a query
/// </summary>
/// <typeparam name="T">Type of object to filter</typeparam>
public interface IQueryFilter<T> where T : class
{
    /// <summary>
    /// Apply the filter to an <see cref="IQueryable{T}"/>.
    /// This implementation should add where clauses to the query and not enumerate the results yet.
    /// </summary>
    /// <param name="query">The query to filter</param>
    /// <returns>the filtered query</returns>
    IQueryable<T> ApplyTo(IQueryable<T> query);
}