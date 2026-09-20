namespace Common.Utils.Pagination;

public abstract class PaginationResultBase<TItem> where TItem : class
{
    public int TotalResults { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
}


public class AsyncPaginationResult<TItem> : PaginationResultBase<TItem> where TItem : class
{
    public required IAsyncEnumerable<TItem> Results { get; set; }
}

public class PaginationResult<TItem> : PaginationResultBase<TItem> where TItem : class
{
    public required IEnumerable<TItem> Results { get; set; }
}
