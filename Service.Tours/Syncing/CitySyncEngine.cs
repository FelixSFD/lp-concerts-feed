using System.Diagnostics;
using Common.Utils;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;

namespace Service.Tours.Syncing;

internal class CitySyncEngine(ICityRepository repository, ILogger<CitySyncEngine> logger) : ISyncEngine<CityWithCountryBo, CityIdentifierBo>
{
    private CityWithCountryBo[] _addedOrChangedCities = [];
    private CityWithCountryBo[] _deletedCities = [];

    private async Task LoadChangedCitiesSince(DateTimeOffset lastChange, CancellationToken cancellationToken = default)
    {
        _addedOrChangedCities = await repository.GetChangedAfter(lastChange)
            .Select(DoMapper.ToDtoWithCountry)
            .ToArrayAsync(cancellationToken);
    }

    private async Task LoadDeletedCitiesSince(DateTimeOffset lastChange, CancellationToken cancellationToken = default)
    {
        _deletedCities = await repository.GetDeletedAfter(lastChange)
            .Select(DoMapper.ToDtoWithCountry)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SyncResult<CityWithCountryBo, CityIdentifierBo>> ChangesSince(DateTimeOffset lastSync, CancellationToken cancellationToken = default)
    {
        Log.StartCitySync(logger, lastSync);
        Stopwatch? stopwatch = null;
        if (logger.IsEnabled(LogLevel.Information))
        {
            stopwatch = Stopwatch.StartNew();
        }
        
        var taskLoadChanged = LoadChangedCitiesSince(lastSync, cancellationToken);
        var taskLoadDeleted = LoadDeletedCitiesSince(lastSync, cancellationToken);
        await Task.WhenAll(taskLoadChanged, taskLoadDeleted);

        var result = new SyncResult<CityWithCountryBo, CityIdentifierBo>
        {
            LatestChange = DateTimeOffset.MinValue
        };

        // Find changed or added
        DateTimeOffset? latestChange = DateTimeOffset.MinValue;
        foreach (var addedOrChangedCity in _addedOrChangedCities)
        {
            result.ChangedObjects.Add(addedOrChangedCity);

            if ((addedOrChangedCity.LastChange != null && addedOrChangedCity.LastChange > latestChange) ||
                (lastSync <= DateTimeOffset.UnixEpoch && addedOrChangedCity.LastChange == null))
            {
                latestChange = addedOrChangedCity.LastChange;
            }
        }

        // Find deleted
        var latestDelete = DateTimeOffset.MinValue;
        foreach (var deletedCity in _deletedCities.Where(dc => dc.DeletedAt != null))
        {
            result.DeletedIds.Add(new CityIdentifierBo
            {
                Id = deletedCity.Id,
                CountryCode = deletedCity.CountryCode
            });

            if ((deletedCity.DeletedAt != null && deletedCity.DeletedAt > latestDelete) ||
                (lastSync <= DateTimeOffset.UnixEpoch && deletedCity.DeletedAt == null))
            {
                // delete was later than the most recent change
                latestDelete = deletedCity.DeletedAt ?? DateTimeOffset.MinValue;
            }
        }

        result.LatestChange = (latestChange > latestDelete ? latestChange : latestDelete).Value.RoundingUpToSecond();

        if (result.LatestChange < lastSync)
        {
            result.LatestChange = lastSync.RoundingUpToSecond();
        }
        
        if (stopwatch != null)
        {
            Log.FinishedCitySync(logger, lastSync, result.ChangedObjects.Count, result.DeletedIds.Count, stopwatch.Elapsed);
        }
        else
        {
            Log.FinishedCitySync(logger, lastSync, result.ChangedObjects.Count, result.DeletedIds.Count);
        }

        return result;
    }
}
