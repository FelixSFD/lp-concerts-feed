using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.AspNetCore.Mvc;
using Service.Tours;

namespace Server.Api.Controllers;

[ApiController]
[Route("v3/countries")]
public class CountryController(LocationService locationService, ILogger<CountryController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new country
    /// </summary>
    /// <param name="request">Data of the new country</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<CreatedAtActionResult> CreateCountry([FromBody] CreateCountryRequestDto request)
    {
        logger.LogDebug("Requested to create country: {name}", request.Name);
        var isoCode = await locationService.CreateCountry(request);
        logger.LogDebug("Successfully created country: {isoCode}", isoCode);
        return CreatedAtAction("GetCountryByIsoCode", new { countryCode = isoCode }, isoCode);
    }
    
    /// <summary>
    /// Returns a list of all countries
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<CountryDto>> GetCountries(CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting all countries");
        var countries = await locationService
            .GetCountriesAsync(cancellationToken)
            .ToArrayAsync(cancellationToken);
        logger.LogDebug("Found {countries} countries", countries.Length);
        return Ok(countries);
    }
    
    /// <summary>
    /// Returns a country by its ISO code
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country</param>
    /// <returns></returns>
    [HttpGet("{countryCode}")]
    public async Task<ActionResult<CountryDto>> GetCountryByIsoCode(string countryCode)
    {
        logger.LogDebug("Requested country by ISO code: {countryCode}", countryCode);
        var country = await locationService.GetCountry(countryCode);
        logger.LogDebug("Found the country: {name}", country.Name);
        return Ok(country);
    }
}