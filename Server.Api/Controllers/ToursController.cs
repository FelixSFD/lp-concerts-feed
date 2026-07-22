using LPCalendar.DataStructure.Tours;
using Microsoft.AspNetCore.Mvc;
using Service.Tours;

namespace Server.Api.Controllers;

/// <summary>
/// API Controller to manage tour data
/// </summary>
/// <param name="tourService"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/[controller]")]
public class ToursController(TourService tourService, ILogger<ToursController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new tour
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<CreatedAtActionResult> CreateTour([FromBody] CreateTourRequestDto request)
    {
        var createdTour = await tourService.CreateTourAsync(request);
        logger.LogDebug("Successfully created tour: {tourName} (ID: {tourId})", createdTour.Name, createdTour.Id);
        return CreatedAtAction(nameof(GetTour), new { tourId = request.Id }, request);
    }

    /// <summary>
    /// Returns information about a tour
    /// </summary>
    /// <param name="tourId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpGet("{tourId}")]
    public async Task<ActionResult<TourDto>> GetTour([FromRoute] string tourId)
    {
        var tour = await tourService.GetTourByIdAsync(tourId);
        return Ok(tour);
    }
}