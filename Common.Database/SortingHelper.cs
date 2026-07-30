using System.Linq.Expressions;

namespace Common.Database;

/// <summary>
/// Helper functions to sort <see cref="IQueryable{T}"/> by a list of <see cref="SortDescriptor"/>s
/// </summary>
public static class SortingHelper
{
    /// <summary>
    /// Applies the sorting in a query
    /// </summary>
    /// <param name="query">Query to sort</param>
    /// <param name="sort">List of <see cref="SortDescriptor"/>s to sort by. Must be in the correct order in which the sorting should be applied</param>
    /// <param name="sortExpressions">The mapping of the string to an expression that uses the data object of the query</param>
    /// <typeparam name="TElement">The data object that will be returned by the query</typeparam>
    /// <returns>The sorted query</returns>
    /// <exception cref="ArgumentException">If the property in the <see cref="SortDescriptor.Property"/> is not defined in the <paramref name="sortExpressions"/></exception>
    public static IQueryable<TElement> ApplySorting<TElement>(
        this IQueryable<TElement> query,
        IEnumerable<SortDescriptor> sort,
        IReadOnlyDictionary<string, LambdaExpression> sortExpressions)
    {
        IOrderedQueryable<TElement>? orderedQuery = null;

        foreach (var descriptor in sort)
        {
            if (!sortExpressions.TryGetValue(descriptor.Property, out var expression))
                throw new ArgumentException($"Unknown sort property '{descriptor.Property}'.");

            orderedQuery = ApplyOrder(
                orderedQuery ?? query,
                expression,
                descriptor.Descending,
                orderedQuery != null);
        }

        return orderedQuery ?? query;
    }

    private static IOrderedQueryable<TElement> ApplyOrder<TElement>(
        IQueryable<TElement> query,
        LambdaExpression keySelector,
        bool descending,
        bool thenBy)
    {
        var methodName = thenBy
            ? descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy)
            : descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy);

        var method = typeof(Queryable)
            .GetMethods()
            .Single(m =>
                m.Name == methodName &&
                m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TElement), keySelector.ReturnType);

        return (IOrderedQueryable<TElement>)method.Invoke(null, [query, keySelector])!;
    }
}