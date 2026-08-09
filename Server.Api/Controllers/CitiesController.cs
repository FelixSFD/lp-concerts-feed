using Common.Contracts.Generated.Models;
using Microsoft.AspNetCore.Mvc;
using Service.Tours;
using Service.Tours.Filters;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to read cities. Managing cities is done via <see cref="CountriesController"/>
/// </summary>
[ApiController]
[Route("v3/[controller]")]
public class CitiesController(LocationService locationService, ILogger<CitiesController> logger) : ControllerBase
{
    /// <summary>
    /// Returns a (filtered) list of cities
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="filter">filter and sorting</param>
    /// <returns></returns>
    public async Task<ActionResult<CityWithCountryDto>> GetCities(CancellationToken cancellationToken, [FromQuery] CitiesFilter filter)
    {
        var cities = await locationService.GetCitiesAsync(filter, cancellationToken).ToArrayAsync(cancellationToken);
        logger.LogDebug("Retrieved {count} city details.", cities.Length);
        return Ok(cities);
    }
}