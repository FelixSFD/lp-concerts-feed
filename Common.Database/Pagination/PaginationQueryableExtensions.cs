using System.Linq.Expressions;
using Common.Database.DataObjects;
using Common.Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Common.Database.Pagination;

public static class PaginationQueryableExtensions
{
    extension<TItem>(IQueryable<TItem> query) where TItem : class
    {
        /// <summary>
        /// Returns a paginated result of the query providing some metadata in the result
        /// </summary>
        /// <param name="orderBy">Order the results</param>
        /// <param name="sortExpressions">Expressions to use for sorting</param>
        /// <param name="paginationParams">Pagination parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated query result</returns>
        /// <exception cref="ArgumentException">if <paramref name="orderBy"/> is provided and <paramref name="sortExpressions"/> is not</exception>
        public async Task<PaginatedQueryResult<TItem>> ToPaginatedResultAsync(IEnumerable<SortDescriptor>? orderBy = null, IReadOnlyDictionary<string, LambdaExpression>? sortExpressions = null, IPaginationParams? paginationParams = null, CancellationToken cancellationToken = default)
        {
            if (orderBy != null && sortExpressions == null)
            {
                throw new ArgumentException($"When {nameof(orderBy)} is provided, {nameof(sortExpressions)} must also be provided");
            }
            
            var totalCount = await query.CountAsync(cancellationToken);

            if (orderBy != null && sortExpressions != null)
            {
                query = query.ApplySorting(orderBy, sortExpressions!);
            }
            
            var resultEnumerable = query
                .ApplyPagination(paginationParams)
                .ToAsyncEnumerable();

            return new PaginatedQueryResult<TItem>
            {
                Results = resultEnumerable,
                TotalCount = totalCount
            };
        }
        
        /// <summary>
        /// Applies pagination to the query
        /// </summary>
        /// <param name="orderBy">Order the results</param>
        /// <param name="sortExpressions">Expressions to use for sorting</param>
        /// <param name="paginationParams">Pagination parameters</param>
        /// <returns>Result of the query</returns>
        /// <exception cref="ArgumentException">if <paramref name="orderBy"/> is provided and <paramref name="sortExpressions"/> is not</exception>
        public IAsyncEnumerable<TItem> ApplyPagination(IEnumerable<SortDescriptor>? orderBy = null, IReadOnlyDictionary<string, LambdaExpression>? sortExpressions = null, IPaginationParams? paginationParams = null)
        {
            if (orderBy != null && sortExpressions == null)
            {
                throw new ArgumentException($"When {nameof(orderBy)} is provided, {nameof(sortExpressions)} must also be provided");
            }
            
            if (orderBy != null && sortExpressions != null)
            {
                query = query.ApplySorting(orderBy, sortExpressions!);
            }
            
            return query
                .ApplyPagination(paginationParams)
                .ToAsyncEnumerable();
        }
    }
}