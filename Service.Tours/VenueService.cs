using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;

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
    public async Task<uint> CreateVenue(CreateVenueRequestDto request)
    {
        logger.LogDebug("Requested to create a new venue: {name}", request.CurrentName);
        var venue = request.ToDo();
        venueRepository.Add(venue);
        await venueRepository.SaveChangesAsync();
        logger.LogDebug("Saved venue in DB. New ID: {venueID}", venue.Id);
        return venue.Id;
    }
}