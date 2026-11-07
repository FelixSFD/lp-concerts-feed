using Common.Database.DataObjects;

namespace Common.Database.Repositories;

/// <summary>
/// Repository methods for data objects that implement <see cref="ITimestampedDataObject"/>.
/// </summary>
/// <typeparam name="TDataObject">The type of data object</typeparam>
public interface ITimestampedRepository<out TDataObject> where TDataObject : BaseDo, ITimestampedDataObject
{
    /// <summary>
    /// Returns all <typeparamref name="TDataObject"/> that have been changed after the given timestamp.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public IAsyncEnumerable<TDataObject> GetChangedAfter(DateTimeOffset timestamp);
}
