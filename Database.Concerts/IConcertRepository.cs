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
}