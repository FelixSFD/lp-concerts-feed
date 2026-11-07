using Common.Utils;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Service.Tours.Syncing;

namespace Service.Tours.Tests.Syncing;

public class ConcertSyncEngineTest
{
    private readonly IConcertRepository _concertRepository;
    private readonly ConcertSyncEngine _engine;

    public ConcertSyncEngineTest()
    {
        _concertRepository = Substitute.For<IConcertRepository>();
        var logger = Substitute.For<ILogger<ConcertSyncEngine>>();
        _engine = new ConcertSyncEngine(_concertRepository, logger);
    }

    private static ConcertDo CreateMockConcert(
        string id,
        DateTimeOffset? updatedAt = null,
        DateTimeOffset? deletedAt = null,
        ConcertDo.ConcertStatus status = ConcertDo.ConcertStatus.Planned)
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
        var venue = new VenueDo
        {
            Id = 1u,
            CountryCode = country.IsoCode,
            CityId = city.Id,
            City = city,
            CurrentName = "Test Venue",
            TimeZone = "UTC",
            Country = country,
        };
        var type = new ConcertTypeDo
        {
            Id = 1u,
            Name = "Headliner",
        };

        return new ConcertDo
        {
            Id = id,
            ConcertTypeId = type.Id,
            Type = type,
            VenueId = venue.Id,
            Venue = venue,
            PostedStartTime = new DateTime(2027, 2, 9, 20, 0, 0, DateTimeKind.Utc),
            Status = status,
            UpdatedAt = updatedAt,
            DeletedAt = deletedAt,
            CreatedAt = updatedAt ?? DateTimeOffset.UtcNow,
        };
    }

    [Fact]
    public async Task SyncV2()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();

        var server0 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: null);
        var server1 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: latestChange);
        var server2 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-21));
        var server5 = CreateMockConcert(
            Guid.NewGuid().ToString(),
            updatedAt: DateTimeOffset.Now.AddHours(-25),
            deletedAt: DateTimeOffset.Now.AddHours(-10),
            status: ConcertDo.ConcertStatus.Cancelled);

        var allConcerts = new[] { server0, server1, server2, server3, server4, server5 };

        _concertRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allConcerts
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _concertRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allConcerts
                    .Where(c => c.DeletedAt != null && c.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Equivalent(new[] { server1.Id, server2.Id, server3.Id }, syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(new[] { server5.Id }, syncResult.DeletedIds.ToArray());

        // make sure the latest change is sent in the result. Having the same timestamp for all users helps with caching
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);

        // run again with the new date. Should not return any more results
        syncResult = await _engine.ChangesSince(syncResult.LatestChange);
        Assert.Empty(syncResult.ChangedObjects);
        Assert.Empty(syncResult.DeletedIds);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
    }

    [Fact]
    public async Task SyncV2_LastChangeWasDelete()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();

        var server0 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: null);
        var server1 = CreateMockConcert(
            Guid.NewGuid().ToString(),
            updatedAt: latestChange.AddHours(-15),
            deletedAt: latestChange,
            status: ConcertDo.ConcertStatus.Cancelled);
        var server2 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-21));

        var allConcerts = new[] { server0, server1, server2, server3, server4 };

        _concertRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allConcerts
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _concertRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allConcerts
                    .Where(c => c.DeletedAt != null && c.DeletedAt > threshold)
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.ChangesSince(lastSync);

        // Validate result
        Assert.Equivalent(new[] { server2.Id, server3.Id }, syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(new[] { server1.Id }, syncResult.DeletedIds.ToArray());

        // make sure the latest change is sent in the result. Having the same timestamp for all users helps with caching
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
    }

    [Fact]
    public async Task SyncV2_NoChanges()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);

        var server0 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: null);
        var server1 = CreateMockConcert(
            Guid.NewGuid().ToString(),
            updatedAt: latestChange.AddHours(-15),
            deletedAt: latestChange,
            status: ConcertDo.ConcertStatus.Cancelled);
        var server2 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-21));

        var allConcerts = new[] { server0, server1, server2, server3, server4 };

        _concertRepository
            .GetChangedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allConcerts
                    .Where(c => c.UpdatedAt != null && c.UpdatedAt > threshold && c.DeletedAt == null)
                    .ToAsyncEnumerable();
            });

        _concertRepository
            .GetDeletedAfter(Arg.Any<DateTimeOffset>())
            .Returns(ci =>
            {
                var threshold = ci.Arg<DateTimeOffset>();
                return allConcerts
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