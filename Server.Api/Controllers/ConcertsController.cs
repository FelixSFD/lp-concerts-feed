using LPCalendar.DataStructure.Tours;
using Microsoft.AspNetCore.Mvc;
using Service.Tours;

namespace Server.Api.Controllers;

[ApiController]
[Route("[controller]")]
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