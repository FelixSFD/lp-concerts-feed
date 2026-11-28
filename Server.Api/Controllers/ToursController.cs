using Common.Contracts.Generated.Models;
using Common.Utils.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Tours;
using Service.Tours.DataStructure.Stats;
using Service.Tours.Exceptions;

namespace Server.Api.Controllers;

/// <summary>
/// API Controller to manage tour data
/// </summary>
/// <param name="tourService"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/[controller]")]
public class ToursController(TourService tourService, StatisticsService statisticsService, ILogger<ToursController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new tour
    /// </summary>
    /// <param name="createTourRequestDto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles(RoleNames.AddConcerts)]
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
    /// Returns statistics about a tour. If <paramref name="userId"/> is provided, additional personalized statistics will be included in the response.
    /// </summary>
    /// <param name="tourId">ID of the tour</param>
    /// <param name="userId">(optional) ID of the user</param>
    /// <param name="rarityThreshold">Optional inclusive rarity threshold between 0 and 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns></returns>
    [HttpGet("{tourId}/stats")]
    //[OutputCache(Duration = CacheExpiration.Maximum, Tags = [CacheTags.AlbumsAll, CacheTags.SongsAll, CacheTags.ConcertsAll, CacheTags.ConcertBookmarksAll, CacheTags.SetlistsAll])]
    public async Task<ActionResult<TourStatsResponseDto>> GetTourStatsAsync([FromRoute] string tourId,
        [FromQuery] string? userId = null, [FromQuery] decimal? rarityThreshold = null, CancellationToken cancellationToken = default)
    {
        if (rarityThreshold is < 0 or > 1)
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "rarityThreshold must be between 0 and 1." });
        if (userId is not null && string.IsNullOrWhiteSpace(userId))
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "userId must not be empty." });

        // fetch the tour information to ensure it exists
        var tour = await tourService.GetTourByIdAsync(tourId) ?? throw new TourNotFoundException(tourId);
        
        // Scope to load stats for the tour (not personalized)
        var tourScope = new StatsScope(tourId: tour.Id);
        var (tourConcertStats, tourSongStats) = await statisticsService.GetConcertAndSongStatsAsync(tourScope, cancellationToken);
        var tourSongRarity = await statisticsService.GetSongRarityStatsAsync(tourScope, new StatsScope(), rarityThreshold, cancellationToken);
        var tourSongRarityDict = tourSongRarity.Songs.ToDictionary(s => s.SongId, s => s);
        
        PersonalizedTourStatsDto? personalizedStatsDto = null;
        if (userId is not null)
        {
            // Scope to load stats for the tour and user (personalized)
            var personalizedScope = new StatsScope(tourId: tour.Id, userId: userId);
            var (personalizedTourConcertStats, personalizedTourSongStats) = await statisticsService.GetConcertAndSongStatsAsync(personalizedScope, cancellationToken);
            var personalizedSongRarity = await statisticsService.GetSongRarityStatsAsync(personalizedScope, tourScope, rarityThreshold, cancellationToken);
            var personalizedSongRarityDict = personalizedSongRarity.Songs.ToDictionary(s => s.SongId, s => s);
            personalizedStatsDto = new PersonalizedTourStatsDto
            {
                Songs = [.. personalizedTourSongStats.Select(song => song.ToDtoWithRarity(personalizedSongRarityDict.GetValueOrDefault(song.SongId)))],
                Concerts = personalizedTourConcertStats.ToDto(),
            };
        }
        
        // build the response DTO
        var responseDto = new TourStatsResponseDto
        {
            Tour = tour.ToDto(),
            Concerts = tourConcertStats.ToDto(),
            Songs = [.. tourSongStats.Select(song => song.ToDtoWithRarity(tourSongRarityDict.GetValueOrDefault(song.SongId)))],
            Personalized = personalizedStatsDto,
        };
        
        return Ok(responseDto);
    }
    
    /// <summary>
    /// Deletes a tour
    /// </summary>
    /// <param name="tourId">ID of the tour</param>
    /// <returns>no content</returns>
    [HttpDelete("{tourId}")]
    [AuthorizeRoles(RoleNames.DeleteConcerts)]
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
    [AuthorizeRoles(RoleNames.AddConcerts)]
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
    [AuthorizeRoles(RoleNames.DeleteConcerts)]
    public async Task<NoContentResult> DeleteTourLeg([FromRoute] string tourId, [FromRoute] string legId)
    {
        await tourService.DeleteTourLegAsync(tourId, legId);
        return NoContent();
    }
}