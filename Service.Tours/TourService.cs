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
    /// Returns a list of all tours
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>Information about the tours</returns>
    /// <exception cref="TourNotFoundException">if the tour does not exist</exception>
    public IAsyncEnumerable<TourDto> GetToursAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Loading a list of all tours...");
        return tourRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToDto);
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
    
    /// <summary>
    /// Deletes a tour
    /// </summary>
    /// <param name="id">ID of the tour</param>
    /// <exception cref="TourNotFoundException">if the tour does not exist</exception>
    public async Task DeleteTourAsync(string id)
    {
        logger.LogInformation("DELETING tour with ID: {tourId}", id);
        var tour = await tourRepository.GetByPrimaryKeyAsync(id) ?? throw new TourNotFoundException(id);
        var tourName = tour.Name;
        logger.LogDebug("Found tour: {tourName}", tourName);
        tourRepository.Delete(tour);
        await tourRepository.SaveChangesAsync();
        logger.LogDebug("Successfully deleted tour: {tourName}", tourName);
    }
    
    /// <summary>
    /// Adds a new leg to a tour
    /// </summary>
    /// <param name="request"></param>
    /// <param name="tourId">ID of the tour</param>
    /// <returns>the newly created leg</returns>
    public async Task<TourLegDto> AddTourLegAsync(AddTourLegRequestDto request, string tourId)
    {
        logger.LogDebug("Creating tour leg '{tourLegName}' in tour: {tourId}", request.Name, tourId);
        var tour = await tourRepository.GetByPrimaryKeyAsync(tourId) ?? throw new TourNotFoundException(tourId);
        var tourLeg = request.ToDo(tourId);
        tour.Legs.Add(tourLeg);
        tourRepository.Update(tour);
        await tourRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created tour leg: {tourLegName}", request.Name);
        return tourLeg.ToDto();
    }
    
    /// <summary>
    /// Returns information about a tour leg
    /// </summary>
    /// <param name="tourId">ID of the tour</param>
    /// <param name="legId">ID of the tour leg</param>
    /// <returns>Information about the tour leg</returns>
    /// <exception cref="TourNotFoundException">if the tour does not exist</exception>
    /// <exception cref="TourLegNotFoundException">if the tour leg does not exist, but the tour itself does exist</exception>
    public async Task<TourLegDto> GetTourLegByIdAsync(string tourId, string legId)
    {
        logger.LogDebug("Searching for leg '{tourLegId}' in tour with ID: {tourId}", legId, tourId);
        var tour = await tourRepository.GetByPrimaryKeyAsync(tourId) ?? throw new TourNotFoundException(tourId);
        logger.LogDebug("Found tour: {tourName}", tour.Name);
        return tour.Legs.FirstOrDefault(l => l.Id == legId)?.ToDto() ?? throw new TourLegNotFoundException(tourId, legId);
    }
    
    /// <summary>
    /// Deletes a tour leg
    /// </summary>
    /// <param name="tourId">ID of the tour</param>
    /// <param name="legId">ID of the tour leg</param>
    /// <returns>Information about the tour leg</returns>
    /// <exception cref="TourNotFoundException">if the tour does not exist</exception>
    /// <exception cref="TourLegNotFoundException">if the tour leg does not exist, but the tour itself does exist</exception>
    public async Task DeleteTourLegAsync(string tourId, string legId)
    {
        logger.LogDebug("DELETING leg '{tourLegId}' from tour with ID: {tourId}", legId, tourId);
        var tour = await tourRepository.GetByPrimaryKeyAsync(tourId) ?? throw new TourNotFoundException(tourId);
        var tourName = tour.Name;
        logger.LogDebug("Found tour: {tourName}", tourName);
        var foundLeg = tour.Legs.FirstOrDefault(l => l.Id == legId) ?? throw new TourLegNotFoundException(tourId, legId);
        logger.LogDebug("Found leg to delete: {legName}", foundLeg.Name);
        tour.Legs.Remove(foundLeg);
        tourRepository.Update(tour);
        await tourRepository.SaveChangesAsync();
        logger.LogDebug("Successfully deleted tour leg: {legId}", legId);
    }
}