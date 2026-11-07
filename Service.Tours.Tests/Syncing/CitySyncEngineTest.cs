using Common.Utils;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Service.Tours.Syncing;

namespace Service.Tours.Tests.Syncing;

public class CitySyncEngineTest
{
    private static readonly TimeSpan DateAssertionPrecision = TimeSpan.FromSeconds(1);
    
    private readonly ICityRepository _cityRepository;
    private readonly CitySyncEngine _engine;

    public CitySyncEngineTest()
    {
        _cityRepository = Substitute.For<ICityRepository>();
        var logger = Substitute.For<ILogger<CitySyncEngine>>();
        _engine = new CitySyncEngine(_cityRepository, logger);
    }

    private static CityDo CreateMockCity(
        uint id,
        string countryCode = "US",
        DateTimeOffset? updatedAt = null,
        DateTimeOffset? deletedAt = null)
    {
        var country = new CountryDo
        {
            IsoCode = countryCode,
            Name = "United States",
            NativeName = "United States",
        };

        return new CityDo
        {
            Id = id,
            CountryCode = countryCode,
            Name = $"Test City {id}",
            NativeName = $"Test City {id}",
            Country = country,
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt,
            CreatedAt = updatedAt ?? DateTimeOffset.UtcNow,
        };
    }

    [Fact]
    public async Task SyncCities()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();

        var server0 = CreateMockCity(1, updatedAt: null);
        var server1 = CreateMockCity(2, updatedAt: latestChange);
        var server2 = CreateMockCity(3, updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockCity(4, updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockCity(5, updatedAt: DateTimeOffset.Now.AddHours(-21));
        var server5 = CreateMockCity(
            6,
            countryCode: "DE",
            updatedAt: DateTimeOffset.Now.AddHours(-25),
            deletedAt: DateTimeOffset.Now.AddHours(-10));

        var allCities = new[] { server0, server1, server2, server3, server4, server5 };

        _cityRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCities
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _cityRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCities
                    .Where(c => c.DeletedAt != null && c.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Equivalent(new[] { server1.Id, server2.Id, server3.Id }, syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Single(syncResult.DeletedIds);
        Assert.Equal(server5.Id, syncResult.DeletedIds[0].Id);
        Assert.Equal(server5.CountryCode, syncResult.DeletedIds[0].CountryCode);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange, DateAssertionPrecision);

        // run again with the new date. Should not return any more results
        syncResult = await _engine.ChangesSince(syncResult.LatestChange);
        Assert.Empty(syncResult.ChangedObjects);
        Assert.Empty(syncResult.DeletedIds);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange, DateAssertionPrecision);
    }

    [Fact]
    public async Task SyncCities_LastChangeWasDelete()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();

        var server0 = CreateMockCity(1, updatedAt: null);
        var server1 = CreateMockCity(
            2,
            countryCode: "DE",
            updatedAt: latestChange.AddHours(-15),
            deletedAt: latestChange);
        var server2 = CreateMockCity(3, updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockCity(4, updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockCity(5, updatedAt: DateTimeOffset.Now.AddHours(-21));

        var allCities = new[] { server0, server1, server2, server3, server4 };

        _cityRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCities
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _cityRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCities
                    .Where(c => c.DeletedAt != null && c.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Equivalent(new[] { server2.Id, server3.Id }, syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Single(syncResult.DeletedIds);
        Assert.Equal(server1.Id, syncResult.DeletedIds[0].Id);
        Assert.Equal(server1.CountryCode, syncResult.DeletedIds[0].CountryCode);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange, DateAssertionPrecision);
    }

    [Fact]
    public async Task SyncCities_NoChanges()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);

        var server0 = CreateMockCity(1, updatedAt: null);
        var server1 = CreateMockCity(
            2,
            updatedAt: latestChange.AddHours(-15),
            deletedAt: latestChange);
        var server2 = CreateMockCity(3, updatedAt: DateTimeOffset.Now.AddHours(-19));

        var allCities = new[] { server0, server1, server2 };

        _cityRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCities
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _cityRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allCities
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
        Assert.Equal(expectedLatestChange, syncResult.LatestChange, DateAssertionPrecision);
    }
}
