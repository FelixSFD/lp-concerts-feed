using LPCalendar.DataStructure;

namespace Lambda.ListConcerts.Syncing;

public interface ISyncEngine<TObj, TIdentifier>
{
    Task<SyncResult<TObj, TIdentifier>> SyncWith(TIdentifier[] knownIds, DateTimeOffset lastSync);
}