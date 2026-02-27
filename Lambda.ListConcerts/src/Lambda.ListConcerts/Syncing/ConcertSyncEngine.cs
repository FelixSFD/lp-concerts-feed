using Common.Utils;
using Database.Concerts;
using LPCalendar.DataStructure;

namespace Lambda.ListConcerts.Syncing;

public class ConcertSyncEngine(IConcertRepository repository) : ISyncEngine<Concert, string>
{
    private Concert[] _addedOrChangedConcerts = [];
    private Concert[] _deletedConcerts = [];

    private async Task LoadChangedConcertsSince(DateTimeOffset lastChange)
    {
        _addedOrChangedConcerts = await repository.GetConcertsChangedAfterAsync(lastChange).ToArrayAsync();
    }
    
    
    private async Task LoadDeletedConcertsSince(DateTimeOffset lastChange)
    {
        _deletedConcerts = await repository.GetConcertsDeletedAfterAsync(lastChange).ToArrayAsync();
    }


    public async Task<SyncResult<Concert, string>> ChangesSince(DateTimeOffset lastSync)
    {
        var taskLoadChanged = LoadChangedConcertsSince(lastSync);
        var taskLoadDeleted = LoadDeletedConcertsSince(lastSync);
        await Task.WhenAll(taskLoadChanged, taskLoadDeleted);
        
        var result = new SyncResult<Concert, string>
        {
            LatestChange = DateTimeOffset.MinValue
        };
        
        // Find changed or added
        DateTimeOffset? latestChange = DateTimeOffset.MinValue;
        foreach (var addedOrChangedConcert in _addedOrChangedConcerts)
        {
            result.ChangedObjects.Add(addedOrChangedConcert);

            if ((addedOrChangedConcert.LastChange != null && addedOrChangedConcert.LastChange > latestChange) || (lastSync <= DateTimeOffset.UnixEpoch && addedOrChangedConcert.LastChange == null))
            {
                latestChange = addedOrChangedConcert.LastChange;
            }
        }
        
        // Find deleted
        DateTimeOffset latestDelete = DateTimeOffset.MinValue;
        foreach (var deletedConcert in _deletedConcerts.Where(dc => dc.DeletedAt != null))
        {
            result.DeletedIds.Add(deletedConcert.Id);

            if ((deletedConcert.DeletedAt != null && deletedConcert.DeletedAt > latestDelete) || (lastSync <= DateTimeOffset.UnixEpoch && deletedConcert.DeletedAt == null))
            {
                // delete was later than the most recent change
                latestDelete = deletedConcert.DeletedAt ?? DateTimeOffset.MinValue;
            }
        }

        result.LatestChange = (latestChange > latestDelete ? latestChange : latestDelete).Value.RoundingUpToSecond();

        if (result.LatestChange < lastSync)
        {
            result.LatestChange = lastSync.RoundingUpToSecond();
        }
        
        return result;
    }


    public async Task<SyncResult<Concert, string>> SyncWith(string[] knownIds, DateTimeOffset lastSync)
    {
        await LoadChangedConcertsSince(lastSync);

        var result = new SyncResult<Concert, string>
        {
            LatestChange = DateTimeOffset.MinValue
        };

        // Find changed or added
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

            if ((addedOrChangedConcert.LastChange != null && addedOrChangedConcert.LastChange > result.LatestChange) || (lastSync <= DateTimeOffset.UnixEpoch && addedOrChangedConcert.LastChange == null))
            {
                result.LatestChange = addedOrChangedConcert.LastChange ?? DateTimeOffset.MinValue;
            }
        }
        
        // find deleted
        // those that are in the changed objects don't need to be queried
        // those that were added are not in this list anyway
        var toCheckIfStillExist = knownIds
            .Where(id => result.ChangedObjects.All(c => c.Id != id))
            .ToArray();
        var foundConcerts = repository.GetByIds(toCheckIfStillExist);
        result.DeletedIds = await toCheckIfStillExist
            .ToAsyncEnumerable()
            .WhereAwait(async id => await foundConcerts.AllAsync(c => c.Id != id))
            .ToListAsync();
        
        // Build the result object
        return result;
    }
}