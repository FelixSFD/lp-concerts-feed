using Common.Utils;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Service.Tours.Syncing;

namespace Service.Tours.Tests.Syncing;

public class CountrySyncEngineTest
{
    private readonly ICountryRepository _countryRepository;
    private readonly CountrySyncEngine _engine;

    public CountrySyncEngineTest()
    {
        _countryRepository = Substitute.For<ICountryRepository>();
        var logger = Substitute.For<ILogger<CountrySyncEngine>>();
        _engine = new CountrySyncEngine(_countryRepository, logger);
    }

    private static CountryDo CreateMockCountry(
        string isoCode,
        DateTimeOffset? updatedAt = null,
        DateTimeOffset? deletedAt = null)
    {
        return new CountryDo
        {
            IsoCode = isoCode,
            Name = $"Country {isoCode}",
            NativeName = $"Country {isoCode}",
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt,
            CreatedAt = updatedAt ?? DateTimeOffset.UtcNow,
        };
    }

    [Fact]
    public async Task SyncCountries()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();

        var server0 = CreateMockCountry("USA", updatedAt: null);
        var server1 = CreateMockCountry("DEU", updatedAt: latestChange);
        var server2 = CreateMockCountry("FRA", updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockCountry("GBR", updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockCountry("ITA", updatedAt: DateTimeOffset.Now.AddHours(-21));
        var server5 = CreateMockCountry(
            "ESP",
            updatedAt: DateTimeOffset.Now.AddHours(-25),
            deletedAt: DateTimeOffset.Now.AddHours(-10));

        var allCountries = new[] { server0, server1, server2, server3, server4, server5 };

        _countryRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCountries
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _countryRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCountries
                    .Where(c => c.DeletedAt != null && c.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Equivalent(new[] { server1.IsoCode, server2.IsoCode, server3.IsoCode }, syncResult.ChangedObjects.Select(c => c.IsoCode).ToArray());
        Assert.Equivalent(new[] { server5.IsoCode }, syncResult.DeletedIds.ToArray());
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);

        // run again with the new date. Should not return any more results
        syncResult = await _engine.ChangesSince(syncResult.LatestChange);
        Assert.Empty(syncResult.ChangedObjects);
        Assert.Empty(syncResult.DeletedIds);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
    }

    [Fact]
    public async Task SyncCountries_LastChangeWasDelete()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();

        var server0 = CreateMockCountry("USA", updatedAt: null);
        var server1 = CreateMockCountry(
            "DEU",
            updatedAt: latestChange.AddHours(-15),
            deletedAt: latestChange);
        var server2 = CreateMockCountry("FRA", updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockCountry("GBR", updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockCountry("ITA", updatedAt: DateTimeOffset.Now.AddHours(-21));

        var allCountries = new[] { server0, server1, server2, server3, server4 };

        _countryRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCountries
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _countryRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCountries
                    .Where(c => c.DeletedAt != null && c.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Equivalent(new[] { server2.IsoCode, server3.IsoCode }, syncResult.ChangedObjects.Select(c => c.IsoCode).ToArray());
        Assert.Equivalent(new[] { server1.IsoCode }, syncResult.DeletedIds.ToArray());
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
    }

    [Fact]
    public async Task SyncCountries_NoChanges()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);

        var server0 = CreateMockCountry("USA", updatedAt: null);
        var server1 = CreateMockCountry(
            "DEU",
            updatedAt: latestChange.AddHours(-15),
            deletedAt: latestChange);
        var server2 = CreateMockCountry("FRA", updatedAt: DateTimeOffset.Now.AddHours(-19));

        var allCountries = new[] { server0, server1, server2 };

        _countryRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCountries
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _countryRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCountries
                    .Where(c => c.DeletedAt != null && c.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now;
        var expectedLatestChange = lastSync.RoundingUpToSecond();
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Empty(syncResult.ChangedObjects);
        Assert.Empty(syncResult.DeletedIds);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
    }
}
