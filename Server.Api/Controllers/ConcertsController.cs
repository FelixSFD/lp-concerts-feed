using Common.Contracts.Generated.Models;
using Common.Utils.Cache;
using LPCalendar.DataStructure.Tours;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Tours;
using Service.Tours.Exceptions;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to manage concert data
/// </summary>
/// <param name="concertService"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/[controller]")]
public class ConcertsController(ConcertService concertService, LinkinpediaImportConcertService linkinpediaImportConcertService, IOutputCacheStore outputCacheStore, IConcertImageUploadService concertImageUploadService, ILogger<ConcertsController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new concert in the database
    /// </summary>
    /// <param name="request"></param>
    /// <returns>the created concert</returns>
    [HttpPost]
    [AuthorizeRoles(RoleNames.AddConcerts)]
    [ClearCache(Tags = [CacheTags.ConcertsAll])]
    public async Task<CreatedAtActionResult> CreateConcert([FromBody] CreateConcertRequestDto request)
    {
        logger.LogDebug("Requested to create a new concert...");
        var concert = await concertService.CreateConcertAsync(request.ToBo());
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
    [AuthorizeRoles(RoleNames.AddConcerts)]
    [ClearCache(Tags = [CacheTags.ConcertsAll])]
    public async Task<NoContentResult> UpdateConcert([FromBody] UpdateConcertRequestDto request, [FromRoute] string concertId)
    {
        logger.LogDebug("Requested to update the concert with id: {concertId}", concertId);
        var concert = await concertService.UpdateConcertAsync(concertId, request.ToBo());
        logger.LogDebug("Updated concert with id: {id}", concert.Id);
        return NoContent();
    }

    /// <summary>
    /// Returns a concert without loading the referenced objects like the venue details
    /// </summary>
    /// <param name="concertId"></param>
    /// <returns></returns>
    [HttpGet("{concertId}")]
    [AuthorizeRoles(RoleNames.AddConcerts)]
    [OutputCache(PolicyName = CachePolicyNames.Short, Tags = [CacheTags.ConcertsAll])]
    public async Task<ActionResult<RawConcertBo>> GetRawConcertById([FromRoute] string concertId)
    {
        var concert = await concertService.GetConcertWithoutDetailsByIdAsync(concertId);
        return Ok(concert);
    }

    /// <summary>
    /// Returns the next concert from the current time
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ConcertNotFoundException">if no concert is scheduled</exception>
    [HttpGet("next")]
    [OutputCache(PolicyName = CachePolicyNames.Short, Tags = [CacheTags.ConcertsAll])]
    public async Task<ActionResult<ConcertDetailsDto>> GetNextConcert(CancellationToken cancellationToken)
    {
        logger.LogDebug("Loading next concert...");
        var concert = await concertService.GetNextConcert(cancellationToken);

        if (concert == null)
        {
            logger.LogDebug("No concert scheduled at the moment");
            return NotFound();
        }
        
        logger.LogDebug("Loaded next concert. Start: {startTime}", concert.PostedStartTime);
        var timeUntilStart = concert.ComputedStartTime - DateTimeOffset.Now;
        var maxCacheFor = TimeSpan.FromSeconds(CacheExpiration.VeryLong);
        var cacheDuration = timeUntilStart > maxCacheFor ? maxCacheFor : timeUntilStart;
        var cacheControl = CacheControlHeaderFactory.CacheFor(cacheDuration);
        HttpContext.Response.Headers.CacheControl = cacheControl;
        return Ok(concert.ToDto());
    }
    
    /// <summary>
    /// Returns all details about a concert. This includes information about the venue and general location.
    /// </summary>
    /// <param name="concertId"></param>
    /// <returns></returns>
    [HttpGet("{concertId}/details")]
    [OutputCache(PolicyName = CachePolicyNames.Medium, Tags = [CacheTags.ConcertsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<ConcertDetailsDto>> GetConcertById([FromRoute] string concertId)
    {
        var concert = await concertService.GetConcertByIdAsync(concertId);
        return Ok(concert.ToDto());
    }

    /// <summary>
    /// Returns a list of concerts that can be filtered
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="filter">Filter for the query</param>
    /// <returns>List of concerts including referenced objects</returns>
    [HttpGet]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    [OutputCache(PolicyName = CachePolicyNames.Medium, Tags = [CacheTags.ConcertsAll])]
    public async Task<ActionResult<ConcertListResponseDto>> GetConcertsAsync(CancellationToken cancellationToken, [FromQuery] GetConcertsFilterDto filter)
    {
        var paginatedResult = await concertService.GetConcertsWithDetailsAsync(cancellationToken, filter);
        var concerts = await paginatedResult.Results
            .Select(DtoMapper.ToDto)
            .ToListAsync(cancellationToken);
        logger.LogDebug("Retrieved {count} concert details. The query could return up to {total} concerts", concerts.Count, paginatedResult.TotalResults);

        var response = new ConcertListResponseDto
        {
            Concerts = concerts,
            Metadata = new PaginationResponseMetadataDto
            {
                Offset = (int)filter.Skip,
                TotalElements = paginatedResult.TotalResults,
            }
        };
        return Ok(response);
    }
    
    /// <summary>
    /// Deletes a concert. Note that the data will not be fully removed from the database. Admins will still be able to see deleted concerts.
    /// </summary>
    /// <param name="concertId">ID of the concert to delete</param>
    /// <returns>no content</returns>
    [HttpDelete("{concertId}")]
    [AuthorizeRoles(RoleNames.DeleteConcerts)]
    public async Task<NoContentResult> DeleteConcertById([FromRoute] string concertId)
    {
        await concertService.DeleteConcertAsync(concertId);
        await EvictConcertCacheAsync();
        return NoContent();
    }
    
    /// <summary>
    /// Returns information about the previous and next concerts to a given concert.
    /// </summary>
    /// <param name="concertId">ID of the concert where the search starts</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{concertId}/adjacent")]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.ConcertsAll])]
    public async Task<ActionResult<AdjacentConcertsResponseDto>> GetAdjacentConcerts([FromRoute] string concertId, CancellationToken cancellationToken)
    {
        var bo = await concertService.GetAdjacentConcerts(concertId, cancellationToken);
        var response = new AdjacentConcertsResponseDto
        {
            Current = concertId,
            Previous = bo.Previous?.Id,
            Next = bo.Next?.Id
        };
        
        return Ok(response);
    }

    /// <summary>
    /// Returns a presigned URL to upload a schedule image for a concert.
    /// </summary>
    /// <param name="concertId">ID of the concert</param>
    /// <param name="uploadRequest">additional data</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut("{concertId}/schedule")]
    [ClearCache(Tags = [CacheTags.ConcertsAll])]
    public async Task<ActionResult<ConcertFileUploadResponseDto>> GetPresignedScheduleUploadUrl(string concertId,
        ConcertScheduleUploadRequestDto uploadRequest,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Requested to get presigned schedule upload url for concert: {concertId}; Content-Type: {contentType}", concertId, uploadRequest.ContentType);
        var uploadUrl = await concertImageUploadService.GetPresignedScheduleUploadUrlAsync(concertId, uploadRequest.ContentType);
        var response = new ConcertFileUploadResponseDto
        {
            UploadUrl = uploadUrl
        };
        
        return Ok(response);
    }

    /// <summary>
    /// Generates a preview of the concert import plan for a given concert.
    /// This data can be used to call the correct APIs to create all the necessary data.
    /// </summary>
    /// <param name="wikiPageId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>plan for the import</returns>
    [HttpGet("import/{wikiPageId}")]
    [AuthorizeRoles(RoleNames.AddConcerts)]
    public async Task<ActionResult<ImportConcertPreviewDto>> GetConcertImportPlan(string wikiPageId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Generating concert import plan for concert: {page}", wikiPageId);
        var importPlan = await linkinpediaImportConcertService.GetConcertImportPlan(wikiPageId, cancellationToken);
        logger.LogDebug("Generated import plan for concert: {page}", wikiPageId);
        return Ok(importPlan.ToDto());
    }

    /// <summary>
    /// Returns a list of all concerts on Linkinpedia and their import status.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("import")]
    [AuthorizeRoles(RoleNames.AddConcerts)]
    [OutputCache(PolicyName = CachePolicyNames.Medium, Tags = [CacheTags.ConcertsAll])]
    public async Task<ActionResult<LinkinpediaImportStatusDto>> GetImportStatus(CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting Linkinpedia import status");
        var wikiPagesWithStatus = await linkinpediaImportConcertService
            .GetImportStatusList(cancellationToken)
            .Select(DtoMapper.ToDto)
            .ToListAsync(cancellationToken);
        logger.LogDebug("Retrieved all wiki pages and their import status.");

        var result = new LinkinpediaImportStatusDto
        {
            Concerts = wikiPagesWithStatus,
            NotImportedCount = wikiPagesWithStatus.Count(s =>
                s.ImportStatus == LinkinpediaImportConcertStatusDto.ImportStatusEnum.NotImported),
            ImportedWithoutSetlistCount = wikiPagesWithStatus.Count(s =>
                s.ImportStatus == LinkinpediaImportConcertStatusDto.ImportStatusEnum.ImportedNoSetlist),
            ImportedWithSetlistCount = wikiPagesWithStatus.Count(s =>
                s.ImportStatus == LinkinpediaImportConcertStatusDto.ImportStatusEnum.Imported)
        };
        
        logger.LogDebug("Generated import status. Counts: {countNotImported} not imported, {countNoSetlist} imported without setlist, {countWithSetlist} imported with setlist.", result.NotImportedCount, result.ImportedWithoutSetlistCount, result.ImportedWithSetlistCount);
        
        return Ok(result);
    }

    private async Task EvictConcertCacheAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await outputCacheStore.EvictByTagAsync(CacheTags.ConcertsAll, cancellationToken);
            logger.LogDebug("Evicted concerts cache.");
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to evict concerts cache!");
        }
    }
}