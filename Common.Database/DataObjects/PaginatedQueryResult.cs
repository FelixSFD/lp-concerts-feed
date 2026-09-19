namespace Common.Database.DataObjects;

public class PaginatedQueryResult<TEntity>
{
    public int TotalCount { get; set; }
    public required IAsyncEnumerable<TEntity> Results { get; set; }
}