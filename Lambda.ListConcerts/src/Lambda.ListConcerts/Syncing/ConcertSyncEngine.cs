using Database.Concerts;
using LPCalendar.DataStructure;

namespace Lambda.ListConcerts.Syncing;

public class ConcertSyncEngine(IConcertRepository repository)
{
    private Concert[] _addedOrChangedConcerts;

    private async Task LoadChangedConcertsSince(DateTimeOffset lastChange)
    {
        _addedOrChangedConcerts = await repository.GetConcertsChangedAfterAsync(lastChange).ToArrayAsync();
    }

    public async Task<SyncResult<Concert, string>> SyncWith(string[] knownIds, DateTimeOffset lastSync)
    {
        await LoadChangedConcertsSince(lastSync);

        var result = new SyncResult<Concert, string>();
        foreach (var addedOrChangedConcert in _addedOrChangedConcerts)
        {
            if (knownIds.Contains(addedOrChangedConcert.Id))
            {
                // Concert already known. It must have been changed
                result.ChangedObjects.Add(addedOrChangedConcert);
            }
            else
            {
                // Concert previously unknown. It needs to be added
                result.AddedObjects.Add(addedOrChangedConcert);
            }
        }
        
        // Build the result object
        return result;
    }
}