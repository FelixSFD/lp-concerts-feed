namespace Common.Database.DataObjects;

/// <summary>
/// Objects that can be marked as deleted in the database
/// </summary>
public interface IDeletableDataObject
{
    /// <summary>
    /// Time when this concert was deleted
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }
}


public static class IDeletableDataObjectExtensions
{
    /// <summary>
    /// Removes elements from the list that are marked as deleted at the moment of calling.
    /// </summary>
    /// <param name="collection">Collection to filter</param>
    /// <typeparam name="T">deletable object</typeparam>
    /// <returns>filtered list</returns>
    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> collection) where T : IDeletableDataObject
    {
        return collection.NotDeleted(DateTimeOffset.UtcNow);
    }
    
    /// <summary>
    /// Removes elements from the list that are marked as deleted.
    /// </summary>
    /// <param name="collection">Collection to filter</param>
    /// <param name="at">Time to validate against <see cref="IDeletableDataObject.DeletedAt"/></param>
    /// <typeparam name="T">deletable object</typeparam>
    /// <returns>filtered list</returns>
    public static IQueryable<T> NotDeleted<T>(this IQueryable<T> collection, DateTimeOffset at) where T : IDeletableDataObject
    {
        return collection
            .Where(d => !d.DeletedAt.HasValue || d.DeletedAt > at);
    }
}