using System.Diagnostics;
using Common.Utils;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;

namespace Service.Tours.Syncing;

internal class CountrySyncEngine(ICountryRepository repository, ILogger<CountrySyncEngine> logger) : ISyncEngine<CountryBo, string>
{
    private CountryBo[] _addedOrChangedCountries = [];
    private CountryBo[] _deletedCountries = [];

    private async Task LoadChangedCountriesSince(DateTimeOffset lastChange, CancellationToken cancellationToken = default)
    {
        _addedOrChangedCountries = await repository.GetChangedAfter(lastChange)
            .Select(DoMapper.ToBo)
            .ToArrayAsync(cancellationToken);
    }

    private async Task LoadDeletedCountriesSince(DateTimeOffset lastChange, CancellationToken cancellationToken = default)
    {
        _deletedCountries = await repository.GetDeletedAfter(lastChange)
            .Select(DoMapper.ToBo)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<SyncResult<CountryBo, string>> ChangesSince(DateTimeOffset lastSync, CancellationToken cancellationToken = default)
    {
        Log.StartCountrySync(logger, lastSync);
        Stopwatch? stopwatch = null;
        if (logger.IsEnabled(LogLevel.Information))
        {
            stopwatch = Stopwatch.StartNew();
        }
        
        var taskLoadChanged = LoadChangedCountriesSince(lastSync, cancellationToken);
        var taskLoadDeleted = LoadDeletedCountriesSince(lastSync, cancellationToken);
        await Task.WhenAll(taskLoadChanged, taskLoadDeleted);

        var result = new SyncResult<CountryBo, string>
        {
            LatestChange = DateTimeOffset.MinValue
        };

        // Find changed or added
        DateTimeOffset? latestChange = DateTimeOffset.MinValue;
        foreach (var addedOrChangedCountry in _addedOrChangedCountries)
        {
            result.ChangedObjects.Add(addedOrChangedCountry);

            if ((addedOrChangedCountry.LastChange != null && addedOrChangedCountry.LastChange > latestChange) ||
                (lastSync <= DateTimeOffset.UnixEpoch && addedOrChangedCountry.LastChange == null))
            {
                latestChange = addedOrChangedCountry.LastChange;
            }
        }

        // Find deleted
        var latestDelete = DateTimeOffset.MinValue;
        foreach (var deletedCountry in _deletedCountries.Where(dc => dc.DeletedAt != null))
        {
            result.DeletedIds.Add(deletedCountry.IsoCode);

            if ((deletedCountry.DeletedAt != null && deletedCountry.DeletedAt > latestDelete) ||
                (lastSync <= DateTimeOffset.UnixEpoch && deletedCountry.DeletedAt == null))
            {
                // delete was later than the most recent change
                latestDelete = deletedCountry.DeletedAt ?? DateTimeOffset.MinValue;
            }
        }

        result.LatestChange = (latestChange > latestDelete ? latestChange : latestDelete).Value.RoundingUpToSecond();

        if (result.LatestChange < lastSync)
        {
            result.LatestChange = lastSync.RoundingUpToSecond();
        }
        
        if (stopwatch != null)
        {
            Log.FinishedCountrySync(logger, lastSync, result.ChangedObjects.Count, result.DeletedIds.Count, stopwatch.Elapsed);
        }
        else
        {
            Log.FinishedCountrySync(logger, lastSync, result.ChangedObjects.Count, result.DeletedIds.Count);
        }

        return result;
    }
}
