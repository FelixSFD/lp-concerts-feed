using System.Security.Claims;
using Common.Contracts.Generated.Models;
using LPCalendar.DataStructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Users;

namespace Server.Api.Controllers;

/// <summary>
/// Controller for user-related operations
/// </summary>
/// <param name="userService"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/[controller]")]
public class UsersController(UserService userService, ILogger<UsersController> logger) : ControllerBase
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
            return Unauthorized();
        
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
        
        await userService.UpdateUserAsync(userId, request.Username, cancellationToken);
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
}