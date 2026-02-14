namespace Common.DynamoDb;

public static class EnumerableExtensions
{
    /// <summary>
    /// Removes all elements form the enumerable that are null. Also changes the type to the non-nullable version.
    /// </summary>
    /// <param name="enumerable"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async IAsyncEnumerable<T> NotNull<T>(this IAsyncEnumerable<T?> enumerable)
    {
        await foreach (var item in enumerable)
        {
            if (item != null)
            {
                yield return (T)item;
            }
        }
    }
}