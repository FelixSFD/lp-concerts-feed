using Amazon.Runtime.Internal;
using Common.Utils;
using Database.Concerts.Models;
using LPCalendar.DataStructure;
using DateRange = (System.DateTimeOffset? from, System.DateTimeOffset? to);

namespace Database.Concerts;

public class InMemoryDbConcertRepository: IConcertRepository
{
    private Dictionary<string, ConcertModel> _concerts = [];

    private IEnumerable<ConcertModel> GetAllSorted()
    {
        return _concerts.Values.OrderBy(c => c.PostedStartTime);
    }
    
    public Task<ConcertModel?> GetByIdAsync(string id)
    {
        var result = GetAllSorted().FirstOrDefault(c => c.Id == id);
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<ConcertModel> GetByIds(IEnumerable<string> ids)
    {
        return ids.ToAsyncEnumerable()
            .SelectAwait(async id => await GetByIdAsync(id))
            .Where(c => c != null)
            .Select(c => c!);
    }

    public Task<ConcertModel?> GetNextAsync()
    {
        var result = GetAllSorted()
            .FirstOrDefault(c => c.PostedStartTime > DateTimeOffset.Now);
        
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<ConcertModel> GetConcertsAsync(string? filterTour = null, DateRange? dateRange = null)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ConcertModel> GetConcertsByStatusAsync(string concertStatus,
        DateRange? dateRange = null)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<ConcertModel> GetConcertsAsync(DateTimeOffset afterDate)
    {
        return GetAllSorted()
            .ToAsyncEnumerable()
            .Where(c => c.PostedStartTime > afterDate);
    }

    public IAsyncEnumerable<ConcertModel> GetConcertsChangedAfterAsync(DateTimeOffset changedAfterDate)
    {
        return GetAllSorted()
            .Where(c => c.LastChange != null && c.LastChange > changedAfterDate)
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<ConcertModel> GetConcertsDeletedAfterAsync(DateTimeOffset deletedAfterDate)
    {
        return GetAllSorted()
            .Where(c => c.DeletedAt != null && c.DeletedAt > deletedAfterDate)
            .ToAsyncEnumerable();
    }

    public Task<DateTimeOffset?> GetLastChangedAsync()
    {
        var all = GetAllSorted().ToArray();
        var lastDeleted = all
            .Where(c => c.DeletedAt != null)
            .OrderByDescending(c => c.DeletedAt)
            .Select(c => c.DeletedAt)
            .FirstOrDefault();
        var lastChanged = all
            .Where(c => c.LastChange != null)
            .OrderByDescending(c => c.LastChange)
            .Select(c => c.LastChange)
            .FirstOrDefault();

        var result = DateTimeOffsetExtensions.Max(lastChanged, lastDeleted);
        return Task.FromResult(result);
    }

    public Task SaveAsync(ConcertModel concert)
    {
        var id = concert.Id;
        _concerts[id] = concert;
        
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(ConcertModel concert)
    {
        var existing = await GetByIdAsync(concert.Id);
        if (existing != null)
        {
            existing.DeletedAt = DateTimeOffset.UtcNow;
            _concerts[concert.Id] = concert;
        }
    }
}