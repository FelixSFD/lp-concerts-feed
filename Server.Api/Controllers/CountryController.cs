using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.AspNetCore.Mvc;
using Service.Tours;

namespace Server.Api.Controllers;

[ApiController]
[Route("v3/countries")]
public class CountryController(LocationService locationService, ILogger<CountryController> logger) : ControllerBase
{
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
    
    [HttpGet("{countryCode}")]
    public async Task<ActionResult<CountryDto>> GetCountryByIsoCode(string countryCode)
    {
        logger.LogDebug("Requested country by ISO code: {countryCode}", countryCode);
        var country = await locationService.GetCountry(countryCode);
        logger.LogDebug("Found the country: {name}", country.Name);
        return Ok(country);
    }
}