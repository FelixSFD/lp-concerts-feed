using Common.Contracts.Generated.Models;
using Common.Utils;
using Common.Utils.Cache;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Cache;
using Service.Tours;
using Service.Tours.Filters;
using Service.Tours.Syncing;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to read cities. Managing cities is done via <see cref="CountriesController"/>
/// </summary>
[ApiController]
[Route("v3/[controller]")]
public class CitiesController(LocationService locationService, ISyncEngine<CityWithCountryBo, CityIdentifierBo> syncEngine, ILogger<CitiesController> logger) : ControllerBase
{
    /// <summary>
    /// Syncs cities that were changed or deleted since the last sync time.
    /// </summary>
    /// <param name="lastSync">Time of the last sync. Can be the time of the latest change to improve caching</param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns></returns>
    [HttpGet("sync")]
    [OutputCache(PolicyName = CachePolicyNames.VeryLong, Tags = [CacheTags.CitiesAll])]
    [CustomResponseCache(Duration = CacheExpiration.Medium)]
    public async Task<ActionResult<SyncCitiesResponseDto>> SyncCityChanges(DateTime lastSync, CancellationToken cancellationToken)
    {
        var result = await syncEngine.ChangesSince(lastSync.ToDateTimeOffset(TimeZoneInfo.Utc), cancellationToken);
        var response = new SyncCitiesResponseDto
        {
            SyncTime = result.LatestChange,
            Updated = result.ChangedObjects.Select(c => c.ToDto()).ToList(),
            DeletedCityIds = result.DeletedIds.Select(id => id.ToDto()).ToList()
        };
        return Ok(response);
    }

    /// <summary>
    /// Returns a (filtered) list of cities
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="filter">filter and sorting</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<CityWithCountryDto[]>> GetCities(CancellationToken cancellationToken, [FromQuery] CitiesFilter filter)
    {
        var cities = await locationService.GetCitiesAsync(filter, cancellationToken)
            .Select(DtoMapper.ToDto)
            .ToArrayAsync(cancellationToken);
        logger.LogDebug("Retrieved {count} city details.", cities.Length);
        return Ok(cities);
    }
}