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
    public async Task<CreatedResult> CreateConcert([FromBody] CreateConcertRequestDto request)
    {
        logger.LogDebug("Requested to create a new concert...");
        var concert = await concertService.CreateConcertAsync(request);
        logger.LogDebug("Created concert with id: {id}", concert.Id);
        return Created(concert.Id, concert);
    }
}