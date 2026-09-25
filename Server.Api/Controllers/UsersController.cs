using System.Security.Claims;
using Common.Contracts.Generated.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Users;

namespace Server.Api.Controllers;

[ApiController]
[Route("v3/[controller]")]
public class UsersController(UserService userService, ILogger<UsersController> logger) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Unauthorized();
        
        var user = await userService.GetUserById(userId);
        return Ok(user.ToDto());
    }
}