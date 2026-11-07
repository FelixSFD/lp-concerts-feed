using System.Diagnostics;
using Common.Utils;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;

namespace Service.Tours.Syncing;

internal class VenueSyncEngine(IVenueRepository repository, ILogger<VenueSyncEngine> logger) : ISyncEngine<VenueWithDetailsBo, uint>
{
    private VenueWithDetailsBo[] _addedOrChangedVenues = [];
    private VenueWithDetailsBo[] _deletedVenues = [];

    private async Task LoadChangedVenuesSince(DateTimeOffset lastChange, CancellationToken cancellationToken = default)
    {
        _addedOrChangedVenues = await repository.GetChangedAfter(lastChange)
            .Select(DoMapper.ToBoWithAllDetails)
            .ToArrayAsync(cancellationToken);
    }

    private async Task LoadDeletedVenuesSince(DateTimeOffset lastChange, CancellationToken cancellationToken = default)
    {
        _deletedVenues = await repository.GetDeletedAfter(lastChange)
            .Select(DoMapper.ToBoWithAllDetails)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SyncResult<VenueWithDetailsBo, uint>> ChangesSince(DateTimeOffset lastSync, CancellationToken cancellationToken = default)
    {
        Log.StartVenueSync(logger, lastSync);
        Stopwatch? stopwatch = null;
        if (logger.IsEnabled(LogLevel.Information))
        {
            stopwatch = Stopwatch.StartNew();
        }
        
        var taskLoadChanged = LoadChangedVenuesSince(lastSync, cancellationToken);
        var taskLoadDeleted = LoadDeletedVenuesSince(lastSync, cancellationToken);
        await Task.WhenAll(taskLoadChanged, taskLoadDeleted);

        var result = new SyncResult<VenueWithDetailsBo, uint>
        {
            LatestChange = DateTimeOffset.MinValue
        };

        // Find changed or added
        DateTimeOffset? latestChange = DateTimeOffset.MinValue;
        foreach (var addedOrChangedVenue in _addedOrChangedVenues)
        {
            result.ChangedObjects.Add(addedOrChangedVenue);

            if ((addedOrChangedVenue.LastChange != null && addedOrChangedVenue.LastChange > latestChange) ||
                (lastSync <= DateTimeOffset.UnixEpoch && addedOrChangedVenue.LastChange == null))
            {
                latestChange = addedOrChangedVenue.LastChange;
            }
        }

        // Find deleted
        var latestDelete = DateTimeOffset.MinValue;
        foreach (var deletedVenue in _deletedVenues.Where(dv => dv.DeletedAt != null))
        {
            result.DeletedIds.Add(deletedVenue.Id);

            if ((deletedVenue.DeletedAt != null && deletedVenue.DeletedAt > latestDelete) ||
                (lastSync <= DateTimeOffset.UnixEpoch && deletedVenue.DeletedAt == null))
            {
                // delete was later than the most recent change
                latestDelete = deletedVenue.DeletedAt ?? DateTimeOffset.MinValue;
            }
        }

        result.LatestChange = (latestChange > latestDelete ? latestChange : latestDelete).Value.RoundingUpToSecond();

        if (result.LatestChange < lastSync)
        {
            result.LatestChange = lastSync.RoundingUpToSecond();
        }
        
        if (stopwatch != null)
        {
            Log.FinishedVenueSync(logger, lastSync, result.ChangedObjects.Count, result.DeletedIds.Count, stopwatch.Elapsed);
        }
        else
        {
            Log.FinishedVenueSync(logger, lastSync, result.ChangedObjects.Count, result.DeletedIds.Count);
        }

        return result;
    }
}
