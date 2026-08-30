using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts.Generated.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Api.Auth;
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
    /// <param name="createTourRequestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles]
    public async Task<ActionResult<TourDto>> CreateTour([FromBody] CreateTourRequestDto createTourRequestDto, CancellationToken cancellationToken)
    {
        var createdTour = await tourService.CreateTourAsync(createTourRequestDto.ToBo());
        logger.LogDebug("Successfully created tour: {tourName} (ID: {tourId})", createdTour.Name, createdTour.Id);
        return CreatedAtAction(nameof(GetTour), new { tourId = createTourRequestDto.Id }, createdTour.ToDto());
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
            .Select(DtoMapper.ToDto)
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
        return Ok(tour.ToDto());
    }
    
    /// <summary>
    /// Deletes a tour
    /// </summary>
    /// <param name="tourId">ID of the tour</param>
    /// <returns>no content</returns>
    [HttpDelete("{tourId}")]
    [AuthorizeRoles]
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
    [AuthorizeRoles]
    public async Task<CreatedAtActionResult> CreateTourLeg([FromBody] AddTourLegRequestDto request, [FromRoute] string tourId)
    {
        var createdTourLeg = await tourService.AddTourLegAsync(request.ToBo(), tourId);
        logger.LogDebug("Successfully created tour leg: {tourName} (ID: {tourId})", createdTourLeg.Name, createdTourLeg.Id);
        return CreatedAtAction(nameof(GetTourLeg), new { tourId = createdTourLeg.TourId, legId = createdTourLeg.Id }, createdTourLeg.ToDto());
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
        return Ok(leg.ToDto());
    }
    
    /// <summary>
    /// Deletes a leg of a tour
    /// </summary>
    /// <param name="tourId">ID of the tour</param>
    /// <param name="legId">ID of the tour leg</param>
    /// <returns>no content</returns>
    [HttpDelete("{tourId}/legs/{legId}")]
    [AuthorizeRoles]
    public async Task<NoContentResult> DeleteTourLeg([FromRoute] string tourId, [FromRoute] string legId)
    {
        await tourService.DeleteTourLegAsync(tourId, legId);
        return NoContent();
    }
}