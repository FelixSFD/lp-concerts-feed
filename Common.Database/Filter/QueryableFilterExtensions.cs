namespace Common.Database.Filter;

/// <summary>
/// Extensions for <see cref="IQueryFilter{T}"/> to support applying <see cref="IQueryFilter{T}"/>
/// </summary>
public static class QueryableFilterExtensions
{
    extension<T>(IQueryable<T> query) where T : class
    {
        /// <summary>
        /// Applies a filter to the query
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <returns>The filtered query</returns>
        public IQueryable<T> ApplyFilter(IQueryFilter<T> filter)
        {
            return filter.ApplyTo(query);
        }
    }
}