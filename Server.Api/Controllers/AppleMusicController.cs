using AppleDeveloperToken;
using Common.Utils.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Cache;

namespace Server.Api.Controllers;

/// <summary>
/// Controller for Apple Music-related operations.
/// </summary>
/// <param name="appleMusicTokenGenerator"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/apple-music")]
public class AppleMusicController(TokenGenerator appleMusicTokenGenerator, ILogger<AppleMusicController> logger) : ControllerBase
{
    /// <summary>
    /// Generates the Apple Music developer token, which is used to access the Apple Music API.
    /// </summary>
    /// <returns></returns>
    [HttpGet("token")]
    [OutputCache(PolicyName = CachePolicyNames.Long)]
    [CustomResponseCache(Duration = CacheExpiration.Maximum)]
    public ActionResult<string> GetAppleMusicDeveloperToken()
    {
        Log.GeneratingAppleMusicDeveloperToken(logger);
        var token = appleMusicTokenGenerator.Generate(TimeSpan.FromDays(3));
        Log.AppleMusicDeveloperTokenCreated(logger, token);
        return Ok(token);
    }
}