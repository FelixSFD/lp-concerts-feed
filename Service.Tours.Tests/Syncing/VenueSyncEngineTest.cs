using Common.Utils;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Service.Tours.Syncing;

namespace Service.Tours.Tests.Syncing;

public class VenueSyncEngineTest
{
    private static readonly TimeSpan DateAssertionPrecision = TimeSpan.FromSeconds(1);
    private readonly IVenueRepository _venueRepository;
    private readonly VenueSyncEngine _engine;

    public VenueSyncEngineTest()
    {
        _venueRepository = Substitute.For<IVenueRepository>();
        var logger = Substitute.For<ILogger<VenueSyncEngine>>();
        _engine = new VenueSyncEngine(_venueRepository, logger);
    }

    private static VenueDo CreateMockVenue(
        uint id,
        DateTimeOffset? updatedAt = null,
        DateTimeOffset? deletedAt = null)
    {
        var country = new CountryDo
        {
            IsoCode = "US",
            Name = "United States",
            NativeName = "United States",
        };
        var city = new CityDo
        {
            Id = 1u,
            CountryCode = country.IsoCode,
            Name = "Test City",
            NativeName = "Test City",
            Country = country,
        };

        return new VenueDo
        {
            Id = id,
            CountryCode = country.IsoCode,
            CityId = city.Id,
            City = city,
            CurrentName = $"Test Venue {id}",
            TimeZone = "UTC",
            Country = country,
            PreviousNames = [],
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt,
            CreatedAt = updatedAt ?? DateTimeOffset.UtcNow,
        };
    }

    [Fact]
    public async Task SyncVenues()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();

        var server0 = CreateMockVenue(1, updatedAt: null);
        var server1 = CreateMockVenue(2, updatedAt: latestChange);
        var server2 = CreateMockVenue(3, updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockVenue(4, updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockVenue(5, updatedAt: DateTimeOffset.Now.AddHours(-21));
        var server5 = CreateMockVenue(
            6,
            updatedAt: DateTimeOffset.Now.AddHours(-25),
            deletedAt: DateTimeOffset.Now.AddHours(-10));

        var allVenues = new[] { server0, server1, server2, server3, server4, server5 };

        _venueRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allVenues
                    .Where(v => v.UpdatedAt != null && v.UpdatedAt > threshold && v.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _venueRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allVenues
                    .Where(v => v.DeletedAt != null && v.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Equivalent(new[] { server1.Id, server2.Id, server3.Id }, syncResult.ChangedObjects.Select(v => v.Id).ToArray());
        Assert.Equivalent(new[] { server5.Id }, syncResult.DeletedIds.ToArray());
        Assert.Equal(expectedLatestChange, syncResult.LatestChange, DateAssertionPrecision);

        // run again with the new date. Should not return any more results
        syncResult = await _engine.ChangesSince(syncResult.LatestChange);
        Assert.Empty(syncResult.ChangedObjects);
        Assert.Empty(syncResult.DeletedIds);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange, DateAssertionPrecision);
    }

    [Fact]
    public async Task SyncVenues_LastChangeWasDelete()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();

        var server0 = CreateMockVenue(1, updatedAt: null);
        var server1 = CreateMockVenue(
            2,
            updatedAt: latestChange.AddHours(-15),
            deletedAt: latestChange);
        var server2 = CreateMockVenue(3, updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockVenue(4, updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockVenue(5, updatedAt: DateTimeOffset.Now.AddHours(-21));

        var allVenues = new[] { server0, server1, server2, server3, server4 };

        _venueRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allVenues
                    .Where(v => v.UpdatedAt != null && v.UpdatedAt > threshold && v.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _venueRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allVenues
                    .Where(v => v.DeletedAt != null && v.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Equivalent(new[] { server2.Id, server3.Id }, syncResult.ChangedObjects.Select(v => v.Id).ToArray());
        Assert.Equivalent(new[] { server1.Id }, syncResult.DeletedIds.ToArray());
        Assert.Equal(expectedLatestChange, syncResult.LatestChange, DateAssertionPrecision);
    }

    [Fact]
    public async Task SyncVenues_NoChanges()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);

        var server0 = CreateMockVenue(1, updatedAt: null);
        var server1 = CreateMockVenue(
            2,
            updatedAt: latestChange.AddHours(-15),
            deletedAt: latestChange);
        var server2 = CreateMockVenue(3, updatedAt: DateTimeOffset.Now.AddHours(-19));

        var allVenues = new[] { server0, server1, server2 };

        _venueRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allVenues
                    .Where(v => v.UpdatedAt != null && v.UpdatedAt > threshold && v.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _venueRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allVenues
                    .Where(v => v.DeletedAt != null && v.DeletedAt > threshold)
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
