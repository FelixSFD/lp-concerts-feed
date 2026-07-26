using LPCalendar.DataStructure.Tours;
using Microsoft.AspNetCore.Mvc;
using Service.Tours;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to manage concert data
/// </summary>
/// <param name="concertService"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/[controller]")]
public class ConcertsController(ConcertService concertService, ILogger<ConcertsController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new concert in the database
    /// </summary>
    /// <param name="request"></param>
    /// <returns>the created concert</returns>
    [HttpPost]
    public async Task<CreatedAtActionResult> CreateConcert([FromBody] CreateConcertRequestDto request)
    {
        logger.LogDebug("Requested to create a new concert...");
        var concert = await concertService.CreateConcertAsync(request);
        logger.LogDebug("Created concert with id: {id}", concert.Id);
        return CreatedAtAction(nameof(GetRawConcertById), new { concertId = concert.Id }, concert);
    }
    
    /// <summary>
    /// Updates a concert in the database
    /// </summary>
    /// <param name="concertId">ID of the concert to update</param>
    /// <param name="request"></param>
    /// <returns>no content</returns>
    [HttpPut("{concertId}")]
    public async Task<NoContentResult> UpdateConcert([FromBody] UpdateConcertRequestDto request, [FromRoute] string concertId)
    {
        logger.LogDebug("Requested to update the concert with id: {concertId}", concertId);
        var concert = await concertService.UpdateConcertAsync(concertId, request);
        logger.LogDebug("Updated concert with id: {id}", concert.Id);
        return NoContent();
    }

    /// <summary>
    /// Returns a concert without loading the referenced objects like the venue details
    /// </summary>
    /// <param name="concertId"></param>
    /// <returns></returns>
    [HttpGet("{concertId}")]
    public async Task<ActionResult<RawConcertDto>> GetRawConcertById([FromRoute] string concertId)
    {
        var concert = await concertService.GetConcertWithoutDetailsByIdAsync(concertId);
        return Ok(concert);
    }
    
    /// <summary>
    /// Returns all details about a concert. This includes information about the venue and general location.
    /// </summary>
    /// <param name="concertId"></param>
    /// <returns></returns>
    [HttpGet("{concertId}/details")]
    public async Task<ActionResult<ConcertDetailsDto>> GetConcertById([FromRoute] string concertId)
    {
        var concert = await concertService.GetConcertByIdAsync(concertId);
        return Ok(concert);
    }
}