using Common.Utils.Cache;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Server.Api.Auth;
using Server.Api.Cache;
using Service.Tours;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to manage countries and cities
/// </summary>
/// <param name="locationService"></param>
/// <param name="logger"></param>
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
    [AuthorizeRoles]
    public async Task<CreatedAtActionResult> CreateCountry([FromBody] CreateCountryRequest request)
    {
        logger.LogDebug("Requested to create country: {name}", request.Name);
        var isoCode = await locationService.CreateCountry(request);
        logger.LogDebug("Successfully created country: {isoCode}", isoCode);
        return CreatedAtAction("GetCountryByIsoCode", new { countryCode = isoCode }, isoCode);
    }
    
    /// <summary>
    /// Updates an existing country
    /// </summary>
    /// <param name="request">new data of the country</param>
    /// <param name="countryCode">ISO-code of the country</param>
    /// <returns>the updated data</returns>
    [HttpPut("{countryCode}")]
    [AuthorizeRoles]
    public async Task<ActionResult<CountryBo>> UpdateCountry([FromBody] UpdateCountryRequestDto request, [FromRoute] string countryCode)
    {
        logger.LogDebug("Requested to update country: {countryCode}", countryCode);
        var updatedCountry = await locationService.UpdateCountryAsync(request, countryCode);
        logger.LogDebug("Successfully updated country: {isoCode}", updatedCountry.IsoCode);
        return Ok(updatedCountry);
    }
    
    /// <summary>
    /// Returns a list of all countries
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [OutputCache(PolicyName = CachePolicyNames.Long)]
    [CustomResponseCache(Duration = CacheExpiration.Long)]
    public async Task<ActionResult<CountryBo>> GetCountries(CancellationToken cancellationToken)
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
    [OutputCache(PolicyName = CachePolicyNames.Default)]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<CountryBo>> GetCountryByIsoCode(string countryCode)
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
    [AuthorizeRoles]
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
    [AuthorizeRoles]
    public async Task<CreatedAtActionResult> CreateState([FromRoute(Name = "countryCode")] string countryCode, [FromBody] CreateStateRequestDto request)
    {
        logger.LogDebug("Requested to create state: {name}", request.Name);
        var stateWithCountryDto = await locationService.CreateState(request, countryCode);
        logger.LogDebug("Successfully created state: {isoCode}", stateWithCountryDto.Name);
        return CreatedAtAction("GetState", new { countryCode = stateWithCountryDto.CountryCode, stateCode = stateWithCountryDto.Code }, stateWithCountryDto);
    }
    
    /// <summary>
    /// Updates a state in a country
    /// </summary>
    /// <param name="countryCode">ISO code of the country where this state is located in</param>
    /// <param name="request">Data of the updated state</param>
    /// <returns></returns>
    [HttpPut("{countryCode}/states/{stateCode}")]
    [AuthorizeRoles]
    public async Task<ActionResult<StateWithCountryDto>> UpdateState([FromRoute(Name = "countryCode")] string countryCode, [FromRoute] string stateCode, [FromBody] UpdateStateRequestDto request)
    {
        logger.LogDebug("Requested to update state: {stateCode}", stateCode);
        var stateWithCountryDto = await locationService.UpdateStateAsync(request, countryCode, stateCode);
        logger.LogDebug("Successfully updated state: {stateCode}", stateWithCountryDto.Code);
        return Ok(stateWithCountryDto);
    }
    
    /// <summary>
    /// Returns a state in a country
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country</param>
    /// <param name="stateCode">Code of the state</param>
    /// <returns>The state including the information about the country</returns>
    [HttpGet("{countryCode}/states/{stateCode}")]
    [OutputCache(PolicyName = CachePolicyNames.Default)]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<CountryBo>> GetState(string countryCode, string stateCode)
    {
        logger.LogDebug("Requested state '{stateCode}' in country '{countryCode}'", stateCode, countryCode);
        var stateWithCountry = await locationService.GetStateInCountryAsync(countryCode, stateCode);
        logger.LogDebug("Found the state: {stateName} (Country: {countryName})", stateWithCountry.Name, stateWithCountry.Country.Name);
        return Ok(stateWithCountry);
    }
    
    /// <summary>
    /// Returns a list of all states in a country
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{countryCode}/states")]
    [OutputCache(PolicyName = CachePolicyNames.Default)]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<StateWithCountryDto>> GetStatesInCountry(string countryCode, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting all states in '{countryCode}'", countryCode);
        var states = await locationService
            .GetStatesInCountryAsync(countryCode, cancellationToken)
            .ToArrayAsync(cancellationToken);
        logger.LogDebug("Found {states} states", states.Length);
        return Ok(states);
    }
    
    /// <summary>
    /// Deletes a state
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country</param>
    /// <param name="stateCode">Code of the state</param>
    /// <returns>no content</returns>
    [HttpDelete("{countryCode}/states/{stateCode}")]
    [AuthorizeRoles]
    public async Task<NoContentResult> DeleteState(string countryCode, string stateCode)
    {
        logger.LogDebug("Requested to delete state: {countryCode} - {stateCode}", countryCode, stateCode);
        await locationService.DeleteStateAsync(countryCode, stateCode);
        logger.LogDebug("Successfully deleted state: {countryCode} - {stateCode}", countryCode, stateCode);
        return NoContent();
    }

    #endregion

    #region Cities

    /// <summary>
    /// Creates a new city in a country
    /// </summary>
    /// <param name="countryCode">ISO code of the country where this city is located in</param>
    /// <param name="request">Data of the new city</param>
    /// <returns></returns>
    [HttpPost("{countryCode}/cities")]
    [AuthorizeRoles]
    public async Task<CreatedAtActionResult> CreateCity([FromRoute(Name = "countryCode")] string countryCode, [FromBody] CreateCityRequestDto request)
    {
        logger.LogDebug("Requested to create city: {name}", request.Name);
        var cityWithCountryDto = await locationService.CreateCity(request, countryCode);
        logger.LogDebug("Successfully created city: {isoCode}", cityWithCountryDto.Name);
        return CreatedAtAction("GetCity", new { countryCode = cityWithCountryDto.CountryCode, cityId = cityWithCountryDto.Id }, cityWithCountryDto);
    }
    
    /// <summary>
    /// Updates a city in a country
    /// </summary>
    /// <param name="countryCode">ISO code of the country where this state is located in</param>
    /// <param name="request">Data of the updated city</param>
    /// <returns></returns>
    [HttpPut("{countryCode}/cities/{cityId:int}")]
    [AuthorizeRoles]
    public async Task<ActionResult<StateWithCountryDto>> UpdateCity([FromRoute(Name = "countryCode")] string countryCode, [FromRoute] uint cityId, [FromBody] UpdateCityRequestDto request)
    {
        logger.LogDebug("Requested to update city: {cityId}", cityId);
        var updatedCity = await locationService.UpdateCityAsync(request, countryCode, cityId);
        logger.LogDebug("Successfully updated city: {cityId}", updatedCity.Id);
        return Ok(updatedCity);
    }
    
    /// <summary>
    /// Returns a state in a country
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country</param>
    /// <param name="cityId">ID of the city</param>
    /// <returns>The city including the information about the country</returns>
    [HttpGet("{countryCode}/cities/{cityId}")]
    [OutputCache(PolicyName = CachePolicyNames.Default)]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<CityWithCountryDto>> GetCity(string countryCode, uint cityId)
    {
        logger.LogDebug("Requested city with ID '{cityId}' in country '{countryCode}'", cityId, countryCode);
        var cityInCountry = await locationService.GetCityInCountryAsync(cityId, countryCode);
        logger.LogDebug("Found the city: {cityName} (Country: {countryName})", cityInCountry.Name, cityInCountry.Country.Name);
        return Ok(cityInCountry);
    }
    
    /// <summary>
    /// Returns a list of all cities in a country
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{countryCode}/cities")]
    [OutputCache(PolicyName = CachePolicyNames.Default)]
    [CustomResponseCache(Duration = CacheExpiration.Default)]
    public async Task<ActionResult<CityWithCountryDto>> GetCitiesInCountry(string countryCode, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting all cities in '{countryCode}'", countryCode);
        var cities = await locationService
            .GetCitiesInCountryAsync(countryCode, cancellationToken)
            .ToArrayAsync(cancellationToken);
        logger.LogDebug("Found {cities} cities", cities.Length);
        return Ok(cities);
    }
    
    /// <summary>
    /// Deletes a city
    /// </summary>
    /// <param name="countryCode">3-letter ISO-code of the country where the city is located in</param>
    /// <param name="cityIdStr">ID of the city</param>
    /// <returns>no content</returns>
    [HttpDelete("{countryCode}/cities/{cityId}")]
    [AuthorizeRoles]
    public async Task<NoContentResult> DeleteCity([FromRoute] string countryCode, [FromRoute(Name = "cityId")] string cityIdStr)
    {
        logger.LogDebug("Requested to delete city: {countryCode} - {cityId}", countryCode, cityIdStr);
        var cityId = uint.Parse(cityIdStr);
        await locationService.DeleteCityAsync(countryCode, cityId);
        logger.LogDebug("Successfully deleted state: {countryCode} - {cityId}", countryCode, cityId);
        return NoContent();
    }

    #endregion
}