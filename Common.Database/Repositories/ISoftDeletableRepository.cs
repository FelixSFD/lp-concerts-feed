using Common.Database.DataObjects;

namespace Common.Database.Repositories;

/// <summary>
/// Repository methods for data objects that implement <see cref="IDeletableDataObject"/>.
/// </summary>
/// <typeparam name="TDataObject">The type of data object</typeparam>
public interface ISoftDeletableRepository<out TDataObject> where TDataObject : BaseDo, IDeletableDataObject
{
    /// <summary>
    /// Returns all <typeparamref name="TDataObject"/> that have been deleted after the given timestamp.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public IAsyncEnumerable<TDataObject> GetDeletedAfter(DateTimeOffset timestamp);
}