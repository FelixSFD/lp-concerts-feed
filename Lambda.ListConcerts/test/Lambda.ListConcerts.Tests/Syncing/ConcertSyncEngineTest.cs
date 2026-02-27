using Common.Utils;
using Database.Concerts;
using Lambda.ListConcerts.Syncing;
using LPCalendar.DataStructure;
using Xunit;

namespace Lambda.ListConcerts.Tests.Syncing;

public class ConcertSyncEngineTest
{
    private readonly InMemoryDbConcertRepository _concertRepository = new();
    
    
    [Fact]
    public async Task SyncV2()
    {
        var syncEngine = new ConcertSyncEngine(_concertRepository);
        
        // Generate test data
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();
        var server0 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 9, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = null, // technically possible but there is a lambda to clean those up as DynamoDB doesn't like null values here either
        };
        await _concertRepository.SaveAsync(server0);
        var server1 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 10, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = latestChange,
        };
        await _concertRepository.SaveAsync(server1);
        
        var server2 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 11, 20, 0, 0, TimeSpan.Zero),
            City = "Test 2",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-19),
        };
        await _concertRepository.SaveAsync(server2);
        
        var server3 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 13, 20, 0, 0, TimeSpan.Zero),
            City = "Test 3",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-20),
        };
        await _concertRepository.SaveAsync(server3);
        
        var server4 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 14, 20, 0, 0, TimeSpan.Zero),
            City = "Test 4",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-21),
        };
        await _concertRepository.SaveAsync(server4);
        
        var server5 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "DELETED",
            PostedStartTime = new DateTimeOffset(2027, 2, 11, 20, 0, 0, TimeSpan.Zero),
            City = "Test 2",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-25),
            DeletedAt = DateTimeOffset.Now.AddHours(-10)
        };
        await _concertRepository.SaveAsync(server5);
        
        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await syncEngine.ChangesSince(lastSync);
        
        // Validate result
        Assert.Equivalent(new[] { server1.Id, server2.Id, server3.Id }, syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(new[] { server5.Id }, syncResult.DeletedIds.ToArray());
        
        // make sure the latest change is sent in the result. Having the same timestamp for all users helps with caching
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
        
        // run again with the new date. Should not return any more results
        syncResult = await syncEngine.ChangesSince(syncResult.LatestChange);
        Assert.Empty(syncResult.ChangedObjects);
        Assert.Empty(syncResult.DeletedIds);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
    }
    
    
    [Fact]
    public async Task SyncV2_LastChangeWasDelete()
    {
        var syncEngine = new ConcertSyncEngine(_concertRepository);
        
        // Generate test data
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var expectedLatestChange = latestChange.RoundingUpToSecond();
        var server0 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 9, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = null,
        };
        await _concertRepository.SaveAsync(server0);
        var server1 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "DELETED",
            PostedStartTime = new DateTimeOffset(2027, 2, 10, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = latestChange.AddHours(-15),
            DeletedAt = latestChange
        };
        await _concertRepository.SaveAsync(server1);
        
        var server2 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 11, 20, 0, 0, TimeSpan.Zero),
            City = "Test 2",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-19),
        };
        await _concertRepository.SaveAsync(server2);
        
        var server3 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 13, 20, 0, 0, TimeSpan.Zero),
            City = "Test 3",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-20),
        };
        await _concertRepository.SaveAsync(server3);
        
        var server4 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 14, 20, 0, 0, TimeSpan.Zero),
            City = "Test 4",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-21),
        };
        await _concertRepository.SaveAsync(server4);
        
        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await syncEngine.ChangesSince(lastSync);
        
        // Validate result
        Assert.Equivalent(new[] { server2.Id, server3.Id }, syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(new[] { server1.Id }, syncResult.DeletedIds.ToArray());
        
        // make sure the latest change is sent in the result. Having the same timestamp for all users helps with caching
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
    }
    
    
    [Fact]
    public async Task SyncV2_NoChanges()
    {
        var syncEngine = new ConcertSyncEngine(_concertRepository);
        
        // Generate test data
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var server0 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 9, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = null,
        };
        await _concertRepository.SaveAsync(server0);
        var server1 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "DELETED",
            PostedStartTime = new DateTimeOffset(2027, 2, 10, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = latestChange.AddHours(-15),
            DeletedAt = latestChange
        };
        await _concertRepository.SaveAsync(server1);
        
        var server2 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 11, 20, 0, 0, TimeSpan.Zero),
            City = "Test 2",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-19),
        };
        await _concertRepository.SaveAsync(server2);
        
        var server3 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 13, 20, 0, 0, TimeSpan.Zero),
            City = "Test 3",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-20),
        };
        await _concertRepository.SaveAsync(server3);
        
        var server4 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 14, 20, 0, 0, TimeSpan.Zero),
            City = "Test 4",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-21),
        };
        await _concertRepository.SaveAsync(server4);
        
        // run the sync
        var lastSync = DateTimeOffset.Now;
        var expectedLatestChange = lastSync.RoundingUpToSecond();
        var syncResult = await syncEngine.ChangesSince(lastSync);
        
        // Validate result
        Assert.Empty(syncResult.ChangedObjects);
        Assert.Empty(syncResult.DeletedIds);
        Assert.Equal(expectedLatestChange, syncResult.LatestChange);
    }
    
    
    [Fact]
    public async Task Sync()
    {
        var syncEngine = new ConcertSyncEngine(_concertRepository);
        
        // Generate test data
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        var server0 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 9, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = null,
        };
        await _concertRepository.SaveAsync(server0);
        var server1 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 10, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = latestChange,
        };
        await _concertRepository.SaveAsync(server1);
        
        var server2 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 11, 20, 0, 0, TimeSpan.Zero),
            City = "Test 2",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-19),
        };
        await _concertRepository.SaveAsync(server2);
        
        var server3 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 13, 20, 0, 0, TimeSpan.Zero),
            City = "Test 3",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-20),
        };
        await _concertRepository.SaveAsync(server3);
        
        var server4 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 14, 20, 0, 0, TimeSpan.Zero),
            City = "Test 4",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-21),
        };
        await _concertRepository.SaveAsync(server4);

        var fakeDeletedId = Guid.NewGuid().ToString();
        string[] existingOnClient = [server3.Id, server4.Id, fakeDeletedId];
        
        // run the sync
        var lastSync = DateTimeOffset.Now.AddHours(-20).AddSeconds(-10);
        var syncResult = await syncEngine.SyncWith(existingOnClient, lastSync);
        
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
        var syncEngine = new ConcertSyncEngine(_concertRepository);
        
        // Generate test data
        var latestChange = DateTimeOffset.Now.AddHours(-5);
        string[] existingOnClient = [];
        
        var server0 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 9, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = null,
        };
        await _concertRepository.SaveAsync(server0);
        
        var server1 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 10, 20, 0, 0, TimeSpan.Zero),
            City = "Test City 1",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = latestChange,
        };
        await _concertRepository.SaveAsync(server1);
        
        var server2 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 11, 20, 0, 0, TimeSpan.Zero),
            City = "Test 2",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-19),
        };
        await _concertRepository.SaveAsync(server2);
        
        var server3 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 13, 20, 0, 0, TimeSpan.Zero),
            City = "Test 3",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-20),
        };
        await _concertRepository.SaveAsync(server3);
        
        var server4 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 14, 20, 0, 0, TimeSpan.Zero),
            City = "Test 4",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-21),
        };
        await _concertRepository.SaveAsync(server4);
        
        // run the sync
        var lastSync = DateTimeOffset.UnixEpoch;
        var syncResult = await syncEngine.SyncWith(existingOnClient, lastSync);
        
        // Validate result
        Assert.Equivalent(new[] { server1.Id, server2.Id, server3.Id, server4.Id }, syncResult.AddedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(Array.Empty<string>(), syncResult.ChangedObjects.Select(c => c.Id).ToArray());
        Assert.Equivalent(Array.Empty<string>(), syncResult.DeletedIds.ToArray());
        
        // make sure the latest change is sent in the result. Having the same timestamp for all users helps with caching
        Assert.Equal(latestChange, syncResult.LatestChange);
    }
}