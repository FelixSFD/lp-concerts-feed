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
        var newId = await service.CreateVenue(request);
        logger.LogDebug("created venue with id {id}", newId);
        return CreatedAtAction(nameof(GetVenueById), new { venueId = newId }, null);
    }

    [HttpGet("{venueId:int}")]
    public Task<ActionResult<VenueDto>> GetVenueById(uint venueId)
    {
        throw new NotImplementedException();
    }
}