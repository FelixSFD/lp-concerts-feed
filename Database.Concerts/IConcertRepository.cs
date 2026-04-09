using Database.Concerts.Models;

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
    public Task<ConcertModel?> GetByIdAsync(string id);
    
    
    /// <summary>
    /// Returns a list of concerts by their IDs
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    public IAsyncEnumerable<ConcertModel> GetByIds(IEnumerable<string> ids);
    
    
    /// <summary>
    /// Returns the next concert
    /// </summary>
    /// <returns>Concert or null if there is no upcoming concert</returns>
    public Task<ConcertModel?> GetNextAsync();

    
    /// <summary>
    /// Returns a list of concerts based on filters
    /// </summary>
    /// <param name="filterTour">Filter for tour name</param>
    /// <param name="dateRange">Filter for concert date</param>
    /// <returns>List of concerts</returns>
    public IAsyncEnumerable<ConcertModel> GetConcertsAsync(string? filterTour = null, DateRange? dateRange = null);
    
    
    /// <summary>
    /// Returns a list of concerts after a given date. If no other filters are used, this is faster/cheaper than the overload.
    /// </summary>
    /// <param name="afterDate">Only return concerts after a given date</param>
    /// <returns></returns>
    public IAsyncEnumerable<ConcertModel> GetConcertsAsync(DateTimeOffset afterDate);
    
    
    /// <summary>
    /// Returns a list of concerts changed after a given date.
    /// </summary>
    /// <param name="changedAfterDate">Only return concerts changed after a given date</param>
    /// <returns></returns>
    public IAsyncEnumerable<ConcertModel> GetConcertsChangedAfterAsync(DateTimeOffset changedAfterDate);
    
    
    /// <summary>
    /// Returns a list of concerts deleted after a given date.
    /// </summary>
    /// <param name="deletedAfterDate">Only return concerts deleted after a given date</param>
    /// <returns></returns>
    public IAsyncEnumerable<ConcertModel> GetConcertsDeletedAfterAsync(DateTimeOffset deletedAfterDate);
    
    
    /// <summary>
    /// Returns the timestamp from when the most recent add, update or delete happened.
    /// </summary>
    /// <returns></returns>
    public Task<DateTimeOffset?> GetLastChangedAsync();
    
    
    /// <summary>
    /// Saves the concert in the database
    /// </summary>
    /// <param name="concert"></param>
    /// <returns></returns>
    public Task SaveAsync(ConcertModel concert);
    
    
    /// <summary>
    /// Deletes a concert from the Database. Depending on the implementation, this might just mark the entry as deleted.
    /// </summary>
    /// <param name="concert">Concert to delete</param>
    /// <returns></returns>
    public Task DeleteAsync(ConcertModel concert);
}