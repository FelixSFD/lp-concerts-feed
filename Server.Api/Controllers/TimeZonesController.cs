using Common.Contracts.Generated.Models;
using Common.Utils.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Cache;
using Service.TimeZones;

namespace Server.Api.Controllers;

/// <summary>
/// API Controller for time zones
/// </summary>
[ApiController]
[Route("v3/[controller]")]
public class TimeZonesController(ITimeZoneService timeZoneService) : ControllerBase
{
    /// <summary>
    /// Retrieves the time zone information based on the provided latitude and longitude coordinates.
    /// </summary>
    /// <param name="lat"></param>
    /// <param name="lon"></param>
    /// <returns></returns>
    [HttpGet("byCoordinates")]
    [OutputCache(PolicyName = CachePolicyNames.BasicallyForever)]
    [CustomResponseCache(Duration = CacheExpiration.Maximum)]
    public async Task<ActionResult<TimeZoneDto>> GetTimeZoneByCoordinates(double lat, double lon)
    {
        var timeZoneInfo = await timeZoneService.GetTimeZoneByCoordinates(lat, lon);
        var timeZoneDto = new TimeZoneDto
        {
            TimeZoneId = timeZoneInfo.Id,
        };
            
        return Ok(timeZoneDto);
    }
}