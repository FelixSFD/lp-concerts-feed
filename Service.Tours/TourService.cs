using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;

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
}