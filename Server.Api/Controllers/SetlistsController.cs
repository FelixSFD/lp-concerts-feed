using Common.Contracts.Generated.Models;
using Common.Utils.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Setlists;
using Service.Setlists.Exceptions;

namespace Server.Api.Controllers;

/// <summary>
/// Controller for setlist related actions
/// </summary>
/// <param name="setlistService"></param>
[ApiController]
[Route("v3/[controller]")]
public class SetlistsController(SetlistService setlistService) : ControllerBase
{
    /// <summary>
    /// Returns headers of all setlists
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [OutputCache(PolicyName = CachePolicyNames.Medium, Tags = [CacheTags.SetlistsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<SetlistHeaderDto[]>> GetSetlistHeaders(CancellationToken cancellationToken)
    {
        return Ok(await setlistService.GetSetlistHeaders(cancellationToken).Select(DtoMapper.ToDto).ToArrayAsync(cancellationToken));
    }

    /// <summary>
    /// Creates a new setlist
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<CreatedAtActionResult> CreateSetlist([FromBody] CreateSetlistRequestDto request)
    {
        var responseBo = await setlistService.CreateSetlistAsync(request.ToBo());
        var responseDto = responseBo.ToDto();
        return CreatedAtAction(nameof(GetCompleteSetlist), new { setlistId = responseDto.Id }, responseDto);
    }

    /// <summary>
    /// Returns a complete setlist with all entries and acts
    /// </summary>
    /// <param name="setlistId"></param>
    /// <returns></returns>
    [HttpGet("{setlistId:int}")]
    [OutputCache(PolicyName = CachePolicyNames.Medium, Tags = [CacheTags.SetlistsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<SetlistDto>> GetCompleteSetlist([FromRoute] uint setlistId)
    {
        var setlistBo = await setlistService.GetCompleteSetlist(setlistId) ?? throw new SetlistNotFoundException(setlistId);
        return Ok(setlistBo.ToDto());
    }

    /// <summary>
    /// Updates the header information of a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{setlistId:int}")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<NoContentResult> UpdateSetlistHeader([FromRoute] uint setlistId,
        [FromBody] UpdateSetlistHeaderRequestDto request)
    {
        await setlistService.UpdateSetlistHeader(setlistId, request.ToBo());
        return NoContent();
    }

    /// <summary>
    /// Deletes a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <returns></returns>
    [HttpDelete("{setlistId:int}")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<NoContentResult> DeleteSetlist([FromRoute] uint setlistId)
    {
        await setlistService.RemoveSetlist(setlistId);
        return NoContent();
    }

    /// <summary>
    /// Returns the acts of a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{setlistId:int}/acts")]
    [OutputCache(PolicyName = CachePolicyNames.Medium, Tags = [CacheTags.SetlistsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<SetlistActDto[]>> GetActsInSetlist([FromRoute] uint setlistId,
        CancellationToken cancellationToken)
    {
        return Ok(await setlistService.GetActsWithinSetlistAsync(setlistId).Select(DtoMapper.ToDto).ToArrayAsync(cancellationToken));
    }

    /// <summary>
    /// Returns a specific entry in a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="entryId"></param>
    /// <returns></returns>
    [HttpGet("{setlistId:int}/entries/{entryId}")]
    [OutputCache(PolicyName = CachePolicyNames.Medium, Tags = [CacheTags.SetlistsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<RawSetlistEntryDto>> GetSetlistEntry([FromRoute] uint setlistId,
        [FromRoute] string entryId)
    {
        var setlistEntryBo = await setlistService.GetSetlistEntryAsync(setlistId, entryId);
        return Ok(setlistEntryBo.ToDto());
    }

    /// <summary>
    /// Updates a setlist entry
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="entryId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{setlistId:int}/entries/{entryId}")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<NoContentResult> UpdateSetlistEntry([FromRoute] uint setlistId, [FromRoute] string entryId,
        [FromBody] UpdateSetlistEntryRequestDto request)
    {
        await setlistService.UpdateSetlistEntryAsync(setlistId, entryId, request.ToBo());
        return NoContent();
    }

    /// <summary>
    /// Deletes an entry from a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="entryId"></param>
    /// <returns></returns>
    [HttpDelete("{setlistId:int}/entries/{entryId}")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<NoContentResult> DeleteSetlistEntry([FromRoute] uint setlistId, [FromRoute] string entryId)
    {
        await setlistService.RemoveSetlistEntry(entryId);
        return NoContent();
    }

    /// <summary>
    /// Adds a song to a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{setlistId:int}/songs")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<ActionResult<AddSongToSetlistResponseDto>> AddSongToSetlist([FromRoute] uint setlistId,
        [FromBody] AddSongToSetlistRequestDto request)
    {
        var entry = await setlistService.AddSongToSetlistAsync(request.ToBo(), setlistId);
        return Ok(new AddSongToSetlistResponseDto { AddedSetlistEntry = entry.ToDto() });
    }

    /// <summary>
    /// Adds a song variant to a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{setlistId:int}/variants")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<ActionResult<AddSongVariantToSetlistResponseDto>> AddSongVariantToSetlist(
        [FromRoute] uint setlistId, [FromBody] AddSongVariantToSetlistRequestDto request)
    {
        var entry = await setlistService.AddSongVariantToSetlistAsync(request.ToBo(), setlistId);
        return Ok(new AddSongVariantToSetlistResponseDto { AddedSetlistEntry = entry.ToDto() });
    }

    /// <summary>
    /// Adds a song mashup to a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{setlistId:int}/mashups")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<ActionResult<AddSongMashupToSetlistResponseDto>> AddSongMashupToSetlist(
        [FromRoute] uint setlistId, [FromBody] AddSongMashupToSetlistRequestDto request)
    {
        var entry = await setlistService.AddSongMashupToSetlistAsync(request.ToBo(), setlistId);
        return Ok(new AddSongMashupToSetlistResponseDto { AddedSetlistEntry = entry.ToDto() });
    }

    /// <summary>
    /// Adds a custom entry to a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{setlistId:int}/custom")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<ActionResult<AddCustomEntryToSetlistResponseDto>> AddCustomEntryToSetlist(
        [FromRoute] uint setlistId, [FromBody] AddCustomEntryToSetlistRequestDto request)
    {
        var entry = await setlistService.AddCustomEntryToSetlistAsync(request.ToBo(), setlistId);
        return Ok(new AddCustomEntryToSetlistResponseDto { AddedSetlistEntry = entry.ToDto() });
    }

    /// <summary>
    /// Reorders the entries in a setlist
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{setlistId:int}/reorder")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<ActionResult<ReorderSetlistEntriesResponseDto>> ReorderSetlistEntries([FromRoute] uint setlistId,
        [FromBody] ReorderSetlistEntriesRequestDto request)
    {
        var reorderedEntries = await setlistService.ReorderSetlistEntriesAsync(setlistId, request.EntryIds?.ToArray() ?? []);
        return Ok(new ReorderSetlistEntriesResponseDto
        {
            ReorderedEntries = reorderedEntries.Select(e => e.ToDto()).ToList()
        });
    }

    /// <summary>
    /// Adds an extra to a song in a setlist entry
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="entryId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{setlistId:int}/entries/{entryId}/extras")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<ActionResult<AddSongExtraToSetlistEntryResponseDto>> AddSongExtraToSetlistEntry(
        [FromRoute] uint setlistId,
        [FromRoute] string entryId,
        [FromBody] AddSongExtraToSetlistEntryRequestDto request)
    {
        await setlistService.AddSongExtraToSetlistEntry(request.ToBo(), entryId);
        var entry = await setlistService.GetSetlistEntryAsync(setlistId, entryId);
        return Ok(new AddSongExtraToSetlistEntryResponseDto
        {
            Entry = entry.ToDto()
        });
    }

    /// <summary>
    /// Removes an extra from a song in a setlist entry
    /// </summary>
    /// <param name="setlistId"></param>
    /// <param name="entryId"></param>
    /// <param name="songExtraId"></param>
    /// <returns></returns>
    [HttpDelete("{setlistId:int}/entries/{entryId}/extras/{songExtraId}")]
    [AuthorizeRoles(RoleNames.ManageSetlists)]
    [ClearCache(Tags = [CacheTags.SetlistsAll])]
    public async Task<NoContentResult> DeleteSongExtraFromSetlistEntry(
        [FromRoute] uint setlistId,
        [FromRoute] string entryId,
        [FromRoute] string songExtraId)
    {
        await setlistService.RemoveSongExtraFromSetlistEntry(entryId, songExtraId);
        return NoContent();
    }
}