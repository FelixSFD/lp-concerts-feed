using Asp.Versioning;
using Common.Utils.Cache;
using LPCalendar.DataStructure.Setlists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Setlists;
using Service.Setlists.Exceptions;

namespace Server.Api.Controllers;

/// <summary>
/// Controller for song mashup related actions
/// </summary>
/// <param name="songService"></param>
[ApiController]
[Route("v3/[controller]")]
public class MashupsController(SongService songService) : ControllerBase
{
    /// <summary>
    /// Returns all song mashups
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.MashupsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
    public async Task<ActionResult<SongMashupBo[]>> GetAllSongMashups(CancellationToken cancellationToken)
    {
        return Ok(await songService.GetAllSongMashupsAsync(cancellationToken).ToArrayAsync(cancellationToken));
    }

    /// <summary>
    /// Creates a new song mashup
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.MashupsAll])]
    public async Task<CreatedAtActionResult> CreateSongMashup([FromBody] CreateSongMashupRequestBo request)
    {
        var mashup = await songService.CreateMashupOfSongsAsync(request);
        return CreatedAtAction(nameof(GetSongMashupById), new { mashupId = mashup.Id }, mashup);
    }

    /// <summary>
    /// Returns a song mashup by its ID
    /// </summary>
    /// <param name="mashupId"></param>
    /// <returns></returns>
    [HttpGet("{mashupId:int}")]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.MashupsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
    public async Task<ActionResult<SongMashupBo>> GetSongMashupById([FromRoute] uint mashupId)
    {
        var mashup = await songService.GetMashupByIdAsync(mashupId) ?? throw new SongMashupNotFoundException(mashupId);
        return Ok(mashup);
    }

    /// <summary>
    /// Updates a song mashup
    /// </summary>
    /// <param name="mashupId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{mashupId:int}")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.MashupsAll])]
    public async Task<ActionResult<SongMashupBo>> UpdateSongMashup([FromRoute] uint mashupId,
        [FromBody] UpdateSongMashupRequestBo request)
    {
        var mashup = await songService.UpdateMashupAsync(mashupId, request);
        return Ok(mashup);
    }

    /// <summary>
    /// Deletes a song mashup
    /// </summary>
    /// <param name="mashupId"></param>
    /// <returns></returns>
    [HttpDelete("{mashupId:int}")]
    [AuthorizeRoles(RoleNames.DeleteSongs)]
    [ClearCache(Tags = [CacheTags.MashupsAll])]
    public async Task<NoContentResult> DeleteSongMashup([FromRoute] uint mashupId)
    {
        await songService.DeleteMashupWithIdAsync(mashupId);
        return NoContent();
    }
}