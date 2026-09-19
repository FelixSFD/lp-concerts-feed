namespace Common.Database.DataObjects;

public class PaginatedQueryResult<TEntity> where TEntity : BaseDo
{
    public int TotalCount { get; set; }
    public IAsyncEnumerable<TEntity> Results { get; set; }
}