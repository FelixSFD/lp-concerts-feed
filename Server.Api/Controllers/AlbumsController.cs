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
/// Controller for album related actions
/// </summary>
/// <param name="albumService"></param>
[ApiController]
[Route("v3/[controller]")]
public class AlbumsController(AlbumService albumService) : ControllerBase
{
    /// <summary>
    /// Returns all albums
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.AlbumsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
    public async Task<ActionResult<AlbumBo[]>> GetAllAlbums(CancellationToken cancellationToken)
    {
        return Ok(await albumService.GetAllAlbumsAsync(cancellationToken).ToArrayAsync(cancellationToken));
    }

    /// <summary>
    /// Creates a new album
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.AlbumsAll])]
    public async Task<CreatedAtActionResult> CreateAlbum([FromBody] CreateAlbumRequestBo request)
    {
        var album = await albumService.CreateAlbumAsync(request);
        return CreatedAtAction(nameof(GetAlbumById), new { albumId = album.Id }, album);
    }

    /// <summary>
    /// Returns an album by its ID
    /// </summary>
    /// <param name="albumId"></param>
    /// <returns></returns>
    [HttpGet("{albumId:int}")]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.AlbumsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
    public async Task<ActionResult<AlbumBo>> GetAlbumById([FromRoute] uint albumId)
    {
        var album = await albumService.GetAlbumById(albumId);
        return Ok(album);
    }

    /// <summary>
    /// Updates an album
    /// </summary>
    /// <param name="albumId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{albumId:int}")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.AlbumsAll])]
    public async Task<ActionResult<AlbumBo>> UpdateAlbum([FromRoute] uint albumId,
        [FromBody] UpdateAlbumRequestBo request)
    {
        var album = await albumService.UpdateAlbumAsync(albumId, request);
        return Ok(album);
    }

    /// <summary>
    /// Deletes an album
    /// </summary>
    /// <param name="albumId"></param>
    /// <returns></returns>
    [HttpDelete("{albumId:int}")]
    [AuthorizeRoles(RoleNames.DeleteSongs, RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.AlbumsAll])]
    public async Task<NoContentResult> DeleteAlbum([FromRoute] uint albumId)
    {
        await albumService.DeleteAlbumWithIdAsync(albumId);
        return NoContent();
    }
}