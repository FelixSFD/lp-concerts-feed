namespace Lambda.ListConcerts.Syncing;

/// <summary>
/// Result of syncing objects with the <see cref="ConcertSyncEngine"/>
/// </summary>
/// <typeparam name="TObj">Type of the objects that are synced</typeparam>
/// <typeparam name="TIdentifier">Type if the ID in the <typeparamref name="TObj"/></typeparam>
public class SyncResult<TObj, TIdentifier>
{
    /// <summary>
    /// Objects that have been added since the last sync
    /// </summary>
    public List<TObj> AddedObjects { get; } = [];
    
    /// <summary>
    /// Objects that have been changed since the last sync
    /// </summary>
    public List<TObj> ChangedObjects { get; } = [];
    
    /// <summary>
    /// List of IDs that were deleted since last sync
    /// </summary>
    public List<TIdentifier> DeletedIds { get; set; } = [];
}