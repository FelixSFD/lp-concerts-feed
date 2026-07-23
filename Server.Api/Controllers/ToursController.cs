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
    /// <param name="tourId">ID of the tour</param>
    /// <returns></returns>
    [HttpGet("{tourId}")]
    public async Task<ActionResult<TourDto>> GetTour([FromRoute] string tourId)
    {
        var tour = await tourService.GetTourByIdAsync(tourId);
        return Ok(tour);
    }
    
    /// <summary>
    /// Deletes a tour
    /// </summary>
    /// <param name="tourId">ID of the tour</param>
    /// <returns>no content</returns>
    [HttpDelete("{tourId}")]
    public async Task<NoContentResult> DeleteTour([FromRoute] string tourId)
    {
        await tourService.DeleteTourAsync(tourId);
        return NoContent();
    }
    
    /// <summary>
    /// Creates a new leg of a tour
    /// </summary>
    /// <param name="request"></param>
    /// <param name="tourId">ID of the tour</param>
    /// <returns></returns>
    [HttpPost("{tourId}/legs")]
    public async Task<CreatedAtActionResult> CreateTourLeg([FromBody] AddTourLegRequestDto request, [FromRoute] string tourId)
    {
        var createdTourLeg = await tourService.AddTourLegAsync(request, tourId);
        logger.LogDebug("Successfully created tour leg: {tourName} (ID: {tourId})", createdTourLeg.Name, createdTourLeg.Id);
        return CreatedAtAction(nameof(GetTour), new { tourId = request.Id }, request);
    }
}