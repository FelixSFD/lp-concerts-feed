using Database.Concerts;
using Lambda.ListConcerts.Syncing;
using LPCalendar.DataStructure;
using Xunit;

namespace Lambda.ListConcerts.Tests.Syncing;

public class ConcertSyncEngineTest
{
    private InMemoryDbConcertRepository _concertRepository = new();
    
    [Fact]
    public async Task Sync()
    {
        var syncEngine = new ConcertSyncEngine(_concertRepository);
        
        // Generate test data
        var server1 = new Concert
        {
            Id = Guid.NewGuid().ToString(),
            Status = "PUBLISHED",
            PostedStartTime = new DateTimeOffset(2027, 2, 10, 20, 0, 0, TimeSpan.Zero),
            City = "Test City",
            Country = "USA",
            TourName = "Test Tour 2027",
            LastChange = DateTimeOffset.Now.AddHours(-5),
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
    }
}