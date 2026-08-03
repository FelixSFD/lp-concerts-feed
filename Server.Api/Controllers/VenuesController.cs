using Common.Utils.Cache;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Tours;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to manage venues
/// </summary>
/// <param name="service">Service that manages venues</param>
/// <param name="logger">Logger</param>
[ApiController]
[Route("v3/[controller]")]
public class VenuesController(VenueService service, ILogger<VenuesController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new venue
    /// </summary>
    /// <param name="request">data of the new venue</param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles]
    public async Task<CreatedAtActionResult> CreateVenue([FromBody] CreateVenueRequestDto request)
    {
        logger.LogDebug("Requested to create venue");
        var newId = await service.CreateVenueAsync(request);
        logger.LogDebug("created venue with id {id}", newId);
        return CreatedAtAction(nameof(GetVenueById), new { venueId = newId }, null);
    }
    
    /// <summary>
    /// Updates the information about a venue. Please note that updates of the name have to be made through special routes.
    /// </summary>
    /// <param name="request">New information of the venue. Partial updates are not possible.</param>
    /// <param name="venueId">ID of the venue</param>
    /// <returns>all information about the venue</returns>
    [HttpPut("{venueId:int}")]
    [AuthorizeRoles]
    public async Task<ActionResult<VenueWithDetailsDto>> UpdateVenue([FromBody] UpdateVenueRequestDto request, [FromRoute] uint venueId)
    {
        logger.LogDebug("Update venue with ID: {venueId}", venueId);
        await service.UpdateVenueAsync(request, venueId);
        var venue = await service.GetVenueWithDetailsByIdAsync(venueId);
        return Ok(venue);
    }

    /// <summary>
    /// Returns the basic information about a venue by its ID
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <returns>basic information about the venue</returns>
    [HttpGet("{venueId:int}")]
    [OutputCache(PolicyName = CachePolicyNames.Long)]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
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
    /// <returns>list of all venues</returns>
    [HttpGet]
    [AuthorizeRoles]
    [OutputCache(PolicyName = CachePolicyNames.Medium)]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
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
    [OutputCache(PolicyName = CachePolicyNames.Long)]
    [CustomResponseCache(Duration = CacheExpiration.Long)]
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
    /// <response code="201">If the venue was deleted successfully</response>
    /// <response code="404">If the venue was not found</response>
    [HttpDelete("{venueId:int}")]
    [AuthorizeRoles]
    public async Task<NoContentResult> DeleteVenueById(uint venueId)
    {
        logger.LogInformation("Requested to delete venue with ID: {venueId}", venueId);
        await service.DeleteVenueAsync(venueId);
        logger.LogInformation("Deleted venue with ID: {venueId}", venueId);
        return NoContent();
    }

    /// <summary>
    /// Adds a new name to a venue for a given time range
    /// </summary>
    /// <param name="request"></param>
    /// <param name="venueId">ID of the venue</param>
    /// <returns>Venue information with details</returns>
    /// <response code="404">If the venue was not found</response>
    [HttpPost("{venueId:int}/names")]
    [AuthorizeRoles]
    public async Task<VenueWithDetailsDto> AddNewVenueName([FromBody] AddVenueNameRequestDto request, [FromRoute] uint venueId)
    {
        await service.AddVenueNameAsync(request, venueId);
        var venue = await service.GetVenueWithDetailsByIdAsync(venueId);
        return venue;
    }
    
    /// <summary>
    /// Changes an existing name of a venue
    /// </summary>
    /// <param name="request"></param>
    /// <param name="venueId">ID of the venue</param>
    /// <param name="venueNameId">ID of the venue name</param>
    /// <returns>no content</returns>
    /// <response code="201">If the name was updated successfully</response>
    /// <response code="404">If the venue or name was not found</response>
    [HttpPut("{venueId:int}/names/{venueNameId:int}")]
    [AuthorizeRoles]
    public async Task<NoContentResult> UpdateVenueName([FromBody] UpdateVenueNameRequestDto request, [FromRoute] uint venueId, [FromRoute] uint venueNameId)
    {
        await service.UpdateVenueNameAsync(request, venueId, venueNameId);
        return NoContent();
    }
    
    /// <summary>
    /// Deletes a previous name of a venue
    /// </summary>
    /// <param name="venueId">ID of the venue</param>
    /// <param name="venueNameId">ID of the venue name</param>
    /// <returns>no content</returns>
    /// <response code="201">If the name was deleted successfully</response>
    /// <response code="404">If the venue was not found</response>
    [HttpDelete("{venueId:int}/names/{venueNameId:int}")]
    [AuthorizeRoles]
    public async Task<NoContentResult> DeleteVenueName([FromRoute] uint venueId, [FromRoute] uint venueNameId)
    {
        await service.DeleteVenueNameAsync(venueId, venueNameId);
        return NoContent();
    }
}