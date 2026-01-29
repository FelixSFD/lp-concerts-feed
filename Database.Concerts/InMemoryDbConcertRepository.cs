using LPCalendar.DataStructure;
using DateRange = (System.DateTimeOffset? from, System.DateTimeOffset? to);

namespace Database.Concerts;

public class InMemoryDbConcertRepository: IConcertRepository
{
    private Dictionary<string, Concert> _concerts = [];

    private IEnumerable<Concert> GetAllSorted()
    {
        return _concerts.Values.OrderBy(c => c.PostedStartTime);
    }
    
    public Task<Concert?> GetByIdAsync(string id)
    {
        var result = GetAllSorted().FirstOrDefault(c => c.Id == id);
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<Concert> GetByIds(IEnumerable<string> ids)
    {
        return ids.ToAsyncEnumerable()
            .SelectAwait(async id => await GetByIdAsync(id))
            .Where(c => c != null)
            .Select(c => c!);
    }

    public Task<Concert?> GetNextAsync()
    {
        var result = GetAllSorted()
            .FirstOrDefault(c => c.PostedStartTime > DateTimeOffset.Now);
        
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<Concert> GetConcertsAsync(string? filterTour = null, DateRange? dateRange = null)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<Concert> GetConcertsAsync(DateTimeOffset afterDate)
    {
        return GetAllSorted()
            .ToAsyncEnumerable()
            .Where(c => c.PostedStartTime > afterDate);
    }

    public IAsyncEnumerable<Concert> GetConcertsChangedAfterAsync(DateTimeOffset changedAfterDate)
    {
        return GetAllSorted()
            .Where(c => c.LastChange > changedAfterDate)
            .ToAsyncEnumerable();
    }

    public Task SaveAsync(Concert concert)
    {
        var id = concert.Id;
        _concerts[id] = concert;
        
        return Task.CompletedTask;
    }
}