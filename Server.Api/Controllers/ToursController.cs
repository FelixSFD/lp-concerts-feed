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
    /// Returns a list of all tours
    /// </summary>
    /// <returns>List of all tours</returns>
    [HttpGet]
    public async Task<ActionResult<TourDto[]>> GetTours(CancellationToken cancellationToken)
    {
        var tours = await tourService
            .GetToursAsync(cancellationToken)
            .ToArrayAsync(cancellationToken);
        return Ok(tours);
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
        return CreatedAtAction(nameof(GetTourLeg), new { tourId = createdTourLeg.TourId, legId = createdTourLeg.Id }, request);
    }
    
    /// <summary>
    /// Returns information about a tour leg
    /// </summary>
    /// <param name="tourId">ID of the tour</param>
    /// <param name="legId">ID of the tour leg</param>
    /// <returns>information about the leg</returns>
    [HttpGet("{tourId}/legs/{legId}")]
    public async Task<ActionResult<TourLegDto>> GetTourLeg([FromRoute] string tourId, [FromRoute] string legId)
    {
        var leg = await tourService.GetTourLegByIdAsync(tourId, legId);
        logger.LogDebug("Found tour leg: {legName} (Tour: {tourId})", leg.Name, leg.TourId);
        return Ok(leg);
    }
}