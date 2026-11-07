using System.Diagnostics;
using Common.Utils;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;

namespace Service.Tours.Syncing;

internal class ConcertSyncEngine(IConcertRepository repository, ILogger<ConcertSyncEngine> logger) : ISyncEngine<ConcertDetailsBo, string>
{
    private ConcertDetailsBo[] _addedOrChangedConcerts = [];
    private ConcertDetailsBo[] _deletedConcerts = [];

    private async Task LoadChangedConcertsSince(DateTimeOffset lastChange, CancellationToken cancellationToken = default)
    {
        _addedOrChangedConcerts = await repository.GetChangedAfter(lastChange)
            .Select(DoMapper.ToBoWithDetails)
            .ToArrayAsync(cancellationToken);
    }


    private async Task LoadDeletedConcertsSince(DateTimeOffset lastChange, CancellationToken cancellationToken = default)
    {
        _deletedConcerts = await repository.GetDeletedAfter(lastChange)
            .Select(DoMapper.ToBoWithDetails)
            .ToArrayAsync(cancellationToken);
    }


    public async Task<SyncResult<ConcertDetailsBo, string>> ChangesSince(DateTimeOffset lastSync, CancellationToken cancellationToken = default)
    {
        Log.StartConcertSync(logger, lastSync);
        Stopwatch? stopwatch = null;
        if (logger.IsEnabled(LogLevel.Information))
        {
            stopwatch = Stopwatch.StartNew();
        }
        
        var taskLoadChanged = LoadChangedConcertsSince(lastSync, cancellationToken);
        var taskLoadDeleted = LoadDeletedConcertsSince(lastSync, cancellationToken);
        await Task.WhenAll(taskLoadChanged, taskLoadDeleted);

        var result = new SyncResult<ConcertDetailsBo, string>
        {
            LatestChange = DateTimeOffset.MinValue
        };

        // Find changed or added
        DateTimeOffset? latestChange = DateTimeOffset.MinValue;
        foreach (var addedOrChangedConcert in _addedOrChangedConcerts)
        {
            result.ChangedObjects.Add(addedOrChangedConcert);

            if ((addedOrChangedConcert.LastChange != null && addedOrChangedConcert.LastChange > latestChange) ||
                (lastSync <= DateTimeOffset.UnixEpoch && addedOrChangedConcert.LastChange == null))
            {
                latestChange = addedOrChangedConcert.LastChange;
            }
        }

        // Find deleted
        var latestDelete = DateTimeOffset.MinValue;
        foreach (var deletedConcert in _deletedConcerts.Where(dc => dc.DeletedAt != null))
        {
            result.DeletedIds.Add(deletedConcert.Id);

            if ((deletedConcert.DeletedAt != null && deletedConcert.DeletedAt > latestDelete) ||
                (lastSync <= DateTimeOffset.UnixEpoch && deletedConcert.DeletedAt == null))
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
        
        if (stopwatch != null)
        {
            Log.FinishedConcertSync(logger, lastSync, result.ChangedObjects.Count, result.DeletedIds.Count, stopwatch.Elapsed);
        }
        else
        {
            Log.FinishedConcertSync(logger, lastSync, result.ChangedObjects.Count, result.DeletedIds.Count);
        }

        return result;
    }
}