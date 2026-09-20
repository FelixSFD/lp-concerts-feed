using Common.Database.DataObjects;

namespace Common.Database.Repositories;

/// <summary>
/// Basic repository for <see name="BaseDo"/>s
/// </summary>
/// <typeparam name="TDataObject">Type of object to use in the repository</typeparam>
public interface IRepositoryBase<TDataObject> where TDataObject : BaseDo
{
    /// <summary>
    /// Adds the data object to the repository
    /// </summary>
    /// <param name="data"></param>
    public void Add(TDataObject data);
    
    /// <summary>
    /// Updates the data object in the repository
    /// </summary>
    /// <param name="data"></param>
    public void Update(TDataObject data);
    
    /// <summary>
    /// Deletes the data object from the repository
    /// </summary>
    /// <param name="data"></param>
    public void Delete(TDataObject data);
    
    [Obsolete("Use FindAsync() instead")]
    public IAsyncEnumerable<TDataObject> QueryAsync(CancellationToken token);

    /// <summary>
    /// Save the changes to the repository
    /// </summary>
    /// <param name="token">Token to cancel the operation</param>
    /// <returns></returns>
    public Task SaveChangesAsync(CancellationToken token = default);
}