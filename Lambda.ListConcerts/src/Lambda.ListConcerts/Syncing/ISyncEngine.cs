using LPCalendar.DataStructure;

namespace Lambda.ListConcerts.Syncing;

public interface ISyncEngine<TObj, TIdentifier>
{
    [Obsolete("Use ChangesSince for better caching")]
    Task<SyncResult<TObj, TIdentifier>> SyncWith(TIdentifier[] knownIds, DateTimeOffset lastSync);
    
    /// <summary>
    /// Returns all changes since the last sync.
    /// </summary>
    /// <param name="lastSync"></param>
    /// <returns></returns>
    Task<SyncResult<TObj, TIdentifier>> ChangesSince(DateTimeOffset lastSync);
}