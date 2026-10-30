using Asp.Versioning;
using Common.Utils.Cache;
using LPCalendar.DataStructure.Setlists;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Setlists;

namespace Server.Api.Controllers;

/// <summary>
/// Controller for song related actions
/// </summary>
/// <param name="songService"></param>
[ApiController]
[Route("v3/[controller]")]
public class SongsController(SongService songService) : ControllerBase
{
    /// <summary>
    /// Returns all songs
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.SongsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
    public async Task<ActionResult<SongBo[]>> GetAllSongs(CancellationToken cancellationToken)
    {
        return Ok(await songService.GetAllSongsAsync(cancellationToken).ToArrayAsync(cancellationToken));
    }

    /// <summary>
    /// Creates a new song
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SongsAll])]
    public async Task<CreatedAtActionResult> CreateSong([FromBody] CreateSongRequestBo request)
    {
        var song = await songService.CreateSongAsync(request);
        return CreatedAtAction(nameof(GetSongById), new { songId = song.Id }, song);
    }

    /// <summary>
    /// Returns a song by its ID
    /// </summary>
    /// <param name="songId"></param>
    /// <returns></returns>
    [HttpGet("{songId:int}")]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.SongsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
    public async Task<ActionResult<SongBo>> GetSongById([FromRoute] uint songId)
    {
        var song = await songService.GetSongById(songId);
        return Ok(song);
    }

    /// <summary>
    /// Updates a song
    /// </summary>
    /// <param name="songId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{songId:int}")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SongsAll])]
    public async Task<ActionResult<SongBo>> UpdateSong([FromRoute] uint songId,
        [FromBody] UpdateSongRequestBo request)
    {
        var song = await songService.UpdateSongAsync(songId, request);
        return Ok(song);
    }

    /// <summary>
    /// Deletes a song
    /// </summary>
    /// <param name="songId"></param>
    /// <returns></returns>
    [HttpDelete("{songId:int}")]
    [AuthorizeRoles(RoleNames.DeleteSongs)]
    [ClearCache(Tags = [CacheTags.SongsAll])]
    public async Task<NoContentResult> DeleteSong([FromRoute] uint songId)
    {
        await songService.DeleteSongWithIdAsync(songId);
        return NoContent();
    }

    /// <summary>
    /// Returns all variants of a song
    /// </summary>
    /// <param name="songId"></param>
    /// <returns></returns>
    [HttpGet("{songId:int}/variants")]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.SongsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
    public async Task<ActionResult<List<SongVariantBo>>> GetVariantsOfSong([FromRoute] uint songId)
    {
        var variants = await songService.GetVariantsOfSong(songId);
        return Ok(variants);
    }
}