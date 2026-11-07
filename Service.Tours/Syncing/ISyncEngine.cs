namespace Service.Tours.Syncing;

public interface ISyncEngine<TObj, TIdentifier>
{
    /// <summary>
    /// Returns all changes since the last sync.
    /// </summary>
    /// <param name="lastSync">Date when the client last synced with the server</param>
    /// <param name="cancellationToken">Token to cancel the operation</param>
    /// <returns></returns>
    Task<SyncResult<TObj, TIdentifier>> ChangesSince(DateTimeOffset lastSync, CancellationToken cancellationToken = default);
}