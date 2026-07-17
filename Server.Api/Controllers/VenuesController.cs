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
}