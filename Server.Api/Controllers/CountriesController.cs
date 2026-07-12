using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.AspNetCore.Mvc;
using Service.Tours;

namespace Server.Api.Controllers;

[ApiController]
[Route("v3/countries")]
public class CountriesController(LocationService locationService, ILogger<CountriesController> logger) : ControllerBase
{
    #region Countries

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

    /// <summary>
    /// Deletes a country
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country</param>
    /// <returns>no content</returns>
    [HttpDelete("{countryCode}")]
    public async Task<NoContentResult> DeleteCountryByIsoCode(string countryCode)
    {
        logger.LogDebug("Requested to delete country: {countryCode}", countryCode);
        await locationService.DeleteCountryAsync(countryCode);
        logger.LogDebug("Successfully deleted country: {isoCode}", countryCode);
        return NoContent();
    }
    
    #endregion

    #region States

    /// <summary>
    /// Creates a new state in a country
    /// </summary>
    /// <param name="countryCode">ISO code of the country where this state is located in</param>
    /// <param name="request">Data of the new country</param>
    /// <returns></returns>
    [HttpPost("{countryCode}/states")]
    public async Task<CreatedAtActionResult> CreateState([FromRoute(Name = "countryCode")] string countryCode, [FromBody] CreateStateRequestDto request)
    {
        logger.LogDebug("Requested to create state: {name}", request.Name);
        var stateWithCountryDto = await locationService.CreateState(request, countryCode);
        logger.LogDebug("Successfully created state: {isoCode}", stateWithCountryDto.Name);
        return CreatedAtAction("GetCountryByIsoCode", new { countryCode = stateWithCountryDto.CountryCode }, stateWithCountryDto); // TODO: correct action
    }
    
    /// <summary>
    /// Returns a state in a country
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country</param>
    /// <param name="stateCode">Code of the state</param>
    /// <returns>The state including the information about the country</returns>
    [HttpGet("{countryCode}/states/{stateCode}")]
    public async Task<ActionResult<CountryDto>> GetState(string countryCode, string stateCode)
    {
        logger.LogDebug("Requested state '{stateCode}' in country '{countryCode}'", stateCode, countryCode);
        var stateWithCountry = await locationService.GetStateInCountryAsync(countryCode, stateCode);
        logger.LogDebug("Found the state: {stateName} (Country: {countryName})", stateWithCountry.Name, stateWithCountry.Country.Name);
        return Ok(stateWithCountry);
    }

    #endregion
}