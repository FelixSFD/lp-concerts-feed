using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.AspNetCore.Mvc;
using Service.Tours;

namespace Server.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class VenuesController(VenueService service, ILogger<VenuesController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new venue
    /// </summary>
    /// <param name="request">data of the new venue</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<CreatedAtActionResult> CreateVenue([FromBody] CreateVenueRequestDto request)
    {
        logger.LogDebug("Requested to create venue");
        var newId = await service.CreateVenueAsync(request);
        logger.LogDebug("created venue with id {id}", newId);
        return CreatedAtAction(nameof(GetVenueById), new { venueId = newId }, null);
    }

    /// <summary>
    /// Returns the basic information about a venue by its ID
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <returns>basic information about the venue</returns>
    [HttpGet("{venueId:int}")]
    public async Task<ActionResult<VenueDto>> GetVenueById(uint venueId)
    {
        logger.LogDebug("Requested venue with ID: {venueId}", venueId);
        var venue = await service.GetVenueByIdAsync(venueId);
        return Ok(venue);
    }

    /// <summary>
    /// Returns an unfiltered list of all venues
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>lift of all venues</returns>
    [HttpGet]
    public async Task<ActionResult<VenueDto[]>> GetAllVenues(CancellationToken cancellationToken)
    {
        logger.LogDebug("Requested to get all venues.");
        var venues = await service
            .GetAllVenuesAsync(cancellationToken)
            .ToArrayAsync(cancellationToken);
        logger.LogDebug("Found {count} venues", venues.Length);
        return Ok(venues);
    }
    
    /// <summary>
    /// Returns the detailed information about a venue by its ID including information about the city and country
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <returns>detailed information about the venue</returns>
    [HttpGet("{venueId:int}/details")]
    public async Task<ActionResult<VenueWithCityDto>> GetVenueWithCityById(uint venueId)
    {
        logger.LogDebug("Requested venue including details with ID: {venueId}", venueId);
        var venue = await service.GetVenueWithDetailsByIdAsync(venueId);
        return Ok(venue);
    }
    
    /// <summary>
    /// Deletes a venue
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <returns>no content</returns>
    [HttpDelete("{venueId:int}")]
    public async Task<NoContentResult> DeleteVenueById(uint venueId)
    {
        logger.LogInformation("Requested to delete venue with ID: {venueId}", venueId);
        await service.DeleteVenueAsync(venueId);
        logger.LogInformation("Deleted venue with ID: {venueId}", venueId);
        return NoContent();
    }
}