using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;
using Service.Tours.Exceptions;

namespace Service.Tours;

/// <summary>
/// Service to manage tour information
/// </summary>
public class TourService(ITourRepository tourRepository, ILogger<TourService> logger)
{
    /// <summary>
    /// Creates a new tour
    /// </summary>
    /// <param name="request"></param>
    /// <returns>the newly created tour</returns>
    public async Task<TourDto> CreateTourAsync(CreateTourRequestDto request)
    {
        logger.LogDebug("Creating tour: {tourName}", request.Name);
        var tour = request.ToDo();
        tourRepository.Add(tour);
        await tourRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created tour: {tourName}", request.Name);
        return tour.ToDto();
    }

    /// <summary>
    /// Returns information about a tour
    /// </summary>
    /// <param name="id">ID of the tour</param>
    /// <returns>Information about the tour</returns>
    /// <exception cref="TourNotFoundException">if the tour does not exist</exception>
    public async Task<TourDto> GetTourByIdAsync(string id)
    {
        logger.LogDebug("Searching for tour with ID: {tourId}", id);
        var tour = await tourRepository.GetByPrimaryKeyAsync(id) ?? throw new TourNotFoundException(id);
        logger.LogDebug("Found tour: {tourName}", tour.Name);
        return tour.ToDto();
    }
}