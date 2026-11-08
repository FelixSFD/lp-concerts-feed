using System.Security.Claims;
using Common.Contracts.Generated.Models;
using Common.Server.Auth;
using Common.Utils.Cache;
using LPCalendar.DataStructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Tours;
using Service.Users;
using Service.Users.Exceptions;

namespace Server.Api.Controllers;

/// <summary>
/// Controller for user-related operations
/// </summary>
/// <param name="userService"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/[controller]")]
public class UsersController(UserService userService, UserStatsService userStatsService, ILogger<UsersController> logger) : ControllerBase
{
    /// <summary>
    /// Returns a list of users that can be filtered and sorted
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AuthorizeRoles(RoleNames.ManageUsers)]
    [OutputCache(PolicyName = CachePolicyNames.Medium, Tags = [CacheTags.UsersAll])]
    public async Task<ActionResult<UserListResponseDto>> GetUsersAsync([FromQuery] GetUsersFilterDto request, CancellationToken cancellationToken)
    {
        var paginatedResult = await userService.GetUsersPaginatedAsync(request, cancellationToken);
        var users = await paginatedResult.Results.Select(DtoMapper.ToDto).ToListAsync(cancellationToken);
        var response = new UserListResponseDto
        {
            Users = users,
            Metadata = new PaginationResponseMetadataDto
            {
                Offset = (int)request.Skip,
                TotalElements = paginatedResult.TotalResults,
            }
        };
        return Ok(response);
    }
    
    /// <summary>
    /// Returns information about the current user
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return NotFound();
        
        var user = await userService.GetUserById(userId, cancellationToken);
        return Ok(user.ToDto());
    }

    /// <summary>
    /// Updates the current user's profile. If it doesn't exist yet, it will be created.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPut("me")]
    [ClearCache(Tags = [CacheTags.UsersAll])]
    public async Task<ActionResult> UpdateCurrentUserAsync([FromBody] UpdateUserProfileDto request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userId is null)
            return Unauthorized();
        
        await userService.UpdateUserAsync(userId, request.Username, request.OriginCountryCode, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Returns a user by their ID.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AuthorizeRoles(RoleNames.ManageUsers)]
    [HttpGet("{userId}")]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.UsersAll])]
    public async Task<ActionResult<UserDto>> GetUserById([FromRoute] string userId, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserById(userId, cancellationToken);
        return Ok(user.ToDto());
    }
    
    /// <summary>
    /// Returns publicly available information about a user by their ID. This does not require authentication.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{userId}/public")]
    [OutputCache(PolicyName = CachePolicyNames.Long, Tags = [CacheTags.UsersAll])]
    public async Task<ActionResult<PublicUserDto>> GetPublicUserById([FromRoute] string userId, CancellationToken cancellationToken)
    {
        userId = ResolveUserId(userId)!;
        var user = await userService.GetUserById(userId, cancellationToken);
        return Ok(user.ToPublicDto());
    }
    
    /// <summary>
    /// Returns some general statistics about the concerts a user has attended
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("me/stats/concerts")]
    [Authorize]
    public async Task<ActionResult<UserConcertStatsResponseDto>> GetCurrentUserConcertStats(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }
        
        return await GetUserConcertStats(userId, cancellationToken);
    }
    
    /// <summary>
    /// Returns some general statistics about the concerts a user has attended
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{userId}/stats/concerts")]
    [OutputCache(PolicyName = CachePolicyNames.UserStatsDynamic, Duration = CacheExpiration.Maximum)]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<UserConcertStatsResponseDto>> GetUserConcertStats([FromRoute] string userId, CancellationToken cancellationToken)
    {
        var stats = await userStatsService.GetAttendedConcerts(userId, cancellationToken);
        return Ok(stats.ToDto());
    }
    
    /// <summary>
    /// Returns some statistics about the songs the current user has seen live
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("me/stats/songs")]
    [Authorize]
    public async Task<ActionResult<UserSongStatsResponseDto>> GetCurrentUserSongStats(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId == null)
        {
            return Unauthorized();
        }
        
        return await GetUserSongStats(userId, cancellationToken);
    }
    
    /// <summary>
    /// Returns some statistics about the songs a user has seen live
    /// </summary>
    /// <param name="userId">ID of the user</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{userId}/stats/songs")]
    [OutputCache(PolicyName = CachePolicyNames.UserStatsDynamic, Duration = CacheExpiration.Maximum, Tags = [CacheTags.SetlistsAll])]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<UserSongStatsResponseDto>> GetUserSongStats([FromRoute] string userId, CancellationToken cancellationToken)
    {
        var stats = await userStatsService.GetSongsUserHasSeenLiveAsync(userId, cancellationToken);
        var responseDto = stats
            .OrderByDescending(s => s.PlayCount)
            .ThenBy(s => s.SongName)
            .ToDto();
        return Ok(responseDto);
    }
    
    /// <summary>
    /// Suggests some usernames for the current user. These are based on album names for now.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet("suggestedNames")]
    public async Task<ActionResult<string[]>> GetSuggestedUserNamesAsync(CancellationToken cancellationToken)
    {
        var names = await userService.GetSuggestedUserNamesAsync(cancellationToken);
        return Ok(names);
    }
    
    /// <summary>
    /// Returns the input user ID, but if it's "me", the current user ID is returned. If the input is "me" and the user is not authenticated, null is returned.
    /// </summary>
    /// <param name="userIdParameter"></param>
    /// <returns></returns>
    private string? ResolveUserId(string? userIdParameter)
    {
        if (!"me".Equals(userIdParameter, StringComparison.CurrentCultureIgnoreCase))
        {
            return userIdParameter;
        }

        var currentUserId = HttpContext.GetUserId();
        if (currentUserId != null)
        {
            Log.ResolvedParameterMeToUserId(logger, currentUserId);
        }
        else
        {
            Log.ResolvedParameterMeToUserIdNotAuthenticated(logger);
        }
        
        return currentUserId;

    }
}