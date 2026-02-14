using LPCalendar.DataStructure;

namespace Database.Concerts;

using DateRange = (DateTimeOffset? from, DateTimeOffset? to);

/// <summary>
/// Handles DB actions for Concerts
/// </summary>
public interface IConcertRepository
{
    /// <summary>
    /// Returns a Concert by its unique ID
    /// </summary>
    /// <param name="id">ID of the concert</param>
    /// <returns>Concert or null if it was not found</returns>
    public Task<Concert?> GetByIdAsync(string id);
    
    
    /// <summary>
    /// Returns a list of concerts by their IDs
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public IAsyncEnumerable<Concert> GetByIds(IEnumerable<string> ids);
    
    
    /// <summary>
    /// Returns the next concert
    /// </summary>
    /// <returns>Concert or null if there is no upcoming concert</returns>
    public Task<Concert?> GetNextAsync();

    
    /// <summary>
    /// Returns a list of concerts based on filters
    /// </summary>
    /// <param name="filterTour">Filter for tour name</param>
    /// <param name="dateRange">Filter for concert date</param>
    /// <returns>List of concerts</returns>
    public IAsyncEnumerable<Concert> GetConcertsAsync(string? filterTour = null, DateRange? dateRange = null);
    
    
    /// <summary>
    /// Returns a list of concerts after a given date. If no other filters are used, this is faster/cheaper than the overload.
    /// </summary>
    /// <param name="afterDate">Only return concerts after a given date</param>
    /// <returns></returns>
    public IAsyncEnumerable<Concert> GetConcertsAsync(DateTimeOffset afterDate);
    
    
    /// <summary>
    /// Returns a list of concerts changed after a given date.
    /// </summary>
    /// <param name="changedAfterDate">Only return concerts after a given date</param>
    /// <returns></returns>
    public IAsyncEnumerable<Concert> GetConcertsChangedAfterAsync(DateTimeOffset changedAfterDate);
    
    
    /// <summary>
    /// Saves the concert in the database
    /// </summary>
    /// <param name="concert"></param>
    /// <returns></returns>
    public Task SaveAsync(Concert concert);
}