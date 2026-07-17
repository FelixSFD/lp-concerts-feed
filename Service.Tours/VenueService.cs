using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;
using Mysqlx;
using Service.Tours.Exceptions;

namespace Service.Tours;

/// <summary>
/// Service to manage venues and their names
/// </summary>
public class VenueService(IVenueRepository venueRepository, ILogger<VenueService> logger)
{
    /// <summary>
    /// Creates a new venue
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ID of the created venue</returns>
    public async Task<uint> CreateVenueAsync(CreateVenueRequestDto request)
    {
        logger.LogDebug("Requested to create a new venue: {name}", request.CurrentName);
        var venue = request.ToDo();
        venueRepository.Add(venue);
        await venueRepository.SaveChangesAsync();
        logger.LogDebug("Saved venue in DB. New ID: {venueID}", venue.Id);
        return venue.Id;
    }

    /// <summary>
    /// Returns the basic information about a venue by its ID
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <returns></returns>
    /// <exception cref="VenueNotFoundException">if the venue was not found</exception>
    public async Task<VenueDto> GetVenueByIdAsync(uint venueId)
    {
        logger.LogDebug("Searching for venue with ID: {venueId}", venueId);
        var venue = await venueRepository.GetByPrimaryKeyWithoutReferencesAsync(venueId) ?? throw new VenueNotFoundException(venueId);
        logger.LogDebug("Found venue: {name}", venue.CurrentName);
        return venue.ToDto();
    }
    
    /// <summary>
    /// Returns the information about a venue by its ID including the data of the city, state and country.
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <returns></returns>
    /// <exception cref="VenueNotFoundException">if the venue was not found</exception>
    public async Task<VenueDto> GetVenueWithDetailsByIdAsync(uint venueId)
    {
        logger.LogDebug("Searching for venue details with ID: {venueId}", venueId);
        var venue = await venueRepository.GetByPrimaryKeyAsync(venueId) ?? throw new VenueNotFoundException(venueId);
        logger.LogDebug("Found venue: {name}", venue.CurrentName);
        return venue.ToDtoWithCityDetails();
    }
}