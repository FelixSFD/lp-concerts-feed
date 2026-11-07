using Common.Utils;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
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
        _engine = new ConcertSyncEngine(_concertRepository);
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

    [Fact]
    public async Task Sync()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);

        var server0 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: null);
        var server1 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: latestChange);
        var server2 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-19));
        var server3 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-20));
        var server4 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: DateTimeOffset.Now.AddHours(-21));

        var allConcerts = new[] { server0, server1, server2, server3, server4 };

        var fakeDeletedId = Guid.NewGuid().ToString();
        string[] existingOnClient = [server3.Id, server4.Id, fakeDeletedId];

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
            .GetByIds(Arg.Any<IEnumerable<string>>())
            .Returns(ci =>
            {
                var requestedIds = ci.Arg<IEnumerable<string>>().ToHashSet();
                return allConcerts
                    .Where(c => requestedIds.Contains(c.Id))
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await _engine.SyncWith(existingOnClient, lastSync);

        // Validate result
        Assert.Equivalent(new[] { server1.Id, server2.Id }, syncResult.AddedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(new[] { server3.Id }, syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(new[] { fakeDeletedId }, syncResult.DeletedIds.ToArray());

        // make sure the latest change is sent in the result. Having the same timestamp for all users helps with caching
        Assert.Equal(latestChange, syncResult.LatestChange);
    }

    [Fact]
    public async Task Sync_FirstTime()
    {
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        string[] existingOnClient = [];

        var server0 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: null);
        var server1 = CreateMockConcert(Guid.NewGuid().ToString(), updatedAt: latestChange);
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
            .GetByIds(Arg.Any<IEnumerable<string>>())
            .Returns(ci =>
            {
                var requestedIds = ci.Arg<IEnumerable<string>>().ToHashSet();
                return allConcerts
                    .Where(c => requestedIds.Contains(c.Id))
                    .ToAsyncEnumerable();
            });

        // run the sync
        var lastSync = DateTimeOffset.UnixEpoch;
        var syncResult = await _engine.SyncWith(existingOnClient, lastSync);

        // Validate result
        Assert.Equivalent(new[] { server1.Id, server2.Id, server3.Id, server4.Id }, syncResult.AddedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(Array.Empty<string>(), syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(Array.Empty<string>(), syncResult.DeletedIds.ToArray());

        // make sure the latest change is sent in the result. Having the same timestamp for all users helps with caching
        Assert.Equal(latestChange, syncResult.LatestChange);
    }

    [Fact]
    public async Task SyncWith_CorrectlyIdentifiesDeletedConcerts()
    {
        var lastSync = DateTimeOffset.UtcNow.AddDays(-1);
        _concertRepository
            .GetChangedAfter(lastSync)
            .Returns(new List<ConcertDo>().ToAsyncEnumerable());

        var knownIds = new[] { "id-1", "id-2", "id-3" };

        var existingConcerts = new List<ConcertDo>
        {
            new() { Id = "id-1" },
            new() { Id = "id-3" }
        };

        _concertRepository
            .GetByIds(Arg.Any<IEnumerable<string>>())
            .Returns(existingConcerts.ToAsyncEnumerable());

        var result = await _engine.SyncWith(knownIds, lastSync);

        Assert.NotNull(result);
        Assert.Single(result.DeletedIds);
        Assert.Contains("id-2", result.DeletedIds);
    }
}