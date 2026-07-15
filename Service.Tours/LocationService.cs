using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;
using Service.Tours.Exceptions;

namespace Service.Tours;

public class LocationService(
    ICountryRepository countryRepository,
    IStateRepository stateRepository,
    ICityRepository cityRepository,
    ILogger<LocationService> logger)
{
    #region Countries
    
    /// <summary>
    /// Creates a new country and saves the DB-context
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ISO code of the created country</returns>
    public async Task<string> CreateCountry(CreateCountryRequestDto request)
    {
        logger.LogDebug("Creating country with ISO-code: {isoCode}", request.IsoCode);
        var newCountry = request.ToDo();
        logger.LogDebug("Mapped request to the new data object");
        countryRepository.Add(newCountry);
        await countryRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created country with ISO-code: {isoCode}", newCountry.IsoCode);
        return newCountry.IsoCode;
    }
    
    /// <summary>
    /// Returns a list of countries
    /// </summary>
    /// <param name="cancellationToken">token to cancel the query</param>
    /// <returns>async enumerable of the countries that were found</returns>
    public IAsyncEnumerable<CountryDto> GetCountriesAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Requesting list of countries...");
        return countryRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToDto);
    }
    
    /// <summary>
    /// Returns a country for the given ISO-code
    /// </summary>
    /// <param name="isoCode"></param>
    /// <returns></returns>
    /// <exception cref="CountryNotFoundException">if the country does not exist</exception>
    public async Task<CountryDto> GetCountry(string isoCode)
    {
        logger.LogDebug("Fetching country with ISO-code: {isoCode}", isoCode);
        var country = await countryRepository.GetByPrimaryKeyAsync(isoCode);
        if (country == null)
        {
            logger.LogWarning("Country '{isoCode}' not found!", isoCode);
            throw new CountryNotFoundException(isoCode);
        }
        
        logger.LogDebug("Country '{countryName}' ({isoCode}) found.", country.Name, isoCode);
        return country.ToDto();
    }
    
    /// <summary>
    /// Deletes a country with a given ISO code
    /// </summary>
    /// <param name="isoCode"></param>
    /// <exception cref="CountryNotFoundException"></exception>
    public async Task DeleteCountryAsync(string isoCode)
    {
        logger.LogInformation("Deleting country with ISO-code: {isoCode}", isoCode);
        var country = await countryRepository.GetByPrimaryKeyAsync(isoCode);
        if (country == null)
        {
            logger.LogWarning("Country '{isoCode}' not found!", isoCode);
            throw new CountryNotFoundException(isoCode);
        }
        
        countryRepository.Delete(country);
        await countryRepository.SaveChangesAsync();
        logger.LogInformation("Country '{countryName}' ({isoCode}) was deleted successfully!", country.Name, isoCode);
    }
    
    #endregion
    
    #region States

    /// <summary>
    /// Creates a new state in a country and saves the DB-context
    /// </summary>
    /// <param name="request"></param>
    /// <param name="countryCode">ISO code of the country where the state is located in</param>
    /// <returns>the created state</returns>
    public async Task<StateWithCountryDto> CreateState(CreateStateRequestDto request, string countryCode)
    {
        logger.LogDebug("Creating state '{stateName}' in country '{isoCode}'", request.Name, countryCode);
        
        logger.LogDebug("Checking if country actually exists in the database...");
        var country = await countryRepository.GetByPrimaryKeyAsync(countryCode);
        if (country == null)
        {
            logger.LogError("Country '{isoCode}' not found while creating a new state!", countryCode);
            throw new CountryNotFoundException(countryCode);
        }
        logger.LogDebug("Country exists in the database.");
        
        var newState = request.ToDo(country.IsoCode);
        newState.CountryCode = countryCode;
        logger.LogDebug("Mapped request to the new data object");
        stateRepository.Add(newState);
        await stateRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created state");
        newState = await stateRepository.GetByPrimaryKeyAsync(countryCode, request.Code);
        return newState?.ToDtoWithCountry() ?? throw new Exception("Creating state was successful but the created entry could not be found in the database! This shouldn't happen.");
    }
    
    /// <summary>
    /// Returns a state for the given code in a country
    /// </summary>
    /// <param name="countryCode">ISO code of the country</param>
    /// <param name="stateCode">code of the state</param>
    /// <returns></returns>
    /// <exception cref="StateNotFoundException">if the state does not exist</exception>
    public async Task<StateWithCountryDto> GetStateInCountryAsync(string countryCode, string stateCode)
    {
        logger.LogDebug("Fetching state: {countryCode} - {stateCode}", countryCode, stateCode);
        var state = await stateRepository.GetByPrimaryKeyAsync(countryCode, stateCode);
        if (state == null)
        {
            logger.LogWarning("State '{countryCode}' not found!", stateCode);
            throw new StateNotFoundException(countryCode, stateCode);
        }
        
        logger.LogDebug("State '{stateName}' ({stateCode}) found.", state.Name, stateCode);
        return state.ToDtoWithCountry();
    }
    
    /// <summary>
    /// Returns a list of all states in a country
    /// </summary>
    /// <param name="countryCode">ISO code of the country</param>
    /// <param name="cancellationToken">token to cancel the query</param>
    /// <returns>async enumerable of the states that were found</returns>
    public IAsyncEnumerable<StateDto> GetStatesInCountryAsync(string countryCode, CancellationToken cancellationToken)
    {
        logger.LogDebug("Requesting list of states in '{countryCode}'...", countryCode);
        return stateRepository
            .QueryAsync(cancellationToken)
            .Where(s => s.CountryCode == countryCode)
            .Select(DtoMapper.ToDto);
    }
    
    /// <summary>
    /// Deletes a state with a given ISO code and state code
    /// </summary>
    /// <param name="isoCode">ISO code of the country</param>
    /// <param name="stateCode">Code of the state within <paramref name="isoCode"/></param>
    /// <exception cref="StateNotFoundException"></exception>
    public async Task DeleteStateAsync(string isoCode, string stateCode)
    {
        logger.LogInformation("Deleting country with ISO-code: {isoCode}", isoCode);
        var state = await stateRepository.GetByPrimaryKeyAsync(isoCode, stateCode);
        if (state == null)
        {
            logger.LogWarning("Country '{isoCode}' not found!", isoCode);
            throw new CountryNotFoundException(isoCode);
        }
        
        stateRepository.Delete(state);
        await stateRepository.SaveChangesAsync();
        logger.LogInformation("State '{stateName}' ({isoCode} - {stateCode}) was deleted successfully!", state.Name, state.CountryCode, state.Code);
    }

    #endregion

    #region Cities

    /// <summary>
    /// Creates a new city in a country and saves the DB-context
    /// </summary>
    /// <param name="request"></param>
    /// <param name="countryCode">ISO code of the country where the city is located in</param>
    /// <returns>the created city</returns>
    public async Task<CityWithCountryDto> CreateCity(CreateCityRequestDto request, string countryCode)
    {
        logger.LogDebug("Creating city '{cityName}' in country '{isoCode}'", request.Name, countryCode);
        
        logger.LogDebug("Checking if country actually exists in the database...");
        var country = await countryRepository.GetByPrimaryKeyAsync(countryCode);
        if (country == null)
        {
            logger.LogError("Country '{isoCode}' not found while creating a new city!", countryCode);
            throw new CountryNotFoundException(countryCode);
        }
        logger.LogDebug("Country exists in the database.");

        if (!string.IsNullOrEmpty(request.StateCode))
        {
            logger.LogDebug("Checking if state actually exists in the database...");
            var stateCode = request.StateCode!;
            var state = await stateRepository.GetByPrimaryKeyAsync(countryCode, stateCode);
            if (state == null)
            {
                logger.LogError("State '{isoCode}' not found while creating a new city!", stateCode);
                throw new StateNotFoundException(countryCode, stateCode);
            }
            logger.LogDebug("State exists in the database.");
        }
        
        var newCity = request.ToDo(country.IsoCode);
        newCity.CountryCode = countryCode;
        newCity.StateCode = request.StateCode;
        logger.LogDebug("Mapped request to the new data object");
        cityRepository.Add(newCity);
        await cityRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created city");
        newCity = await cityRepository.GetByPrimaryKeyAsync(countryCode, newCity.Id);
        return newCity?.ToDtoWithCountry() ?? throw new Exception("Creating city was successful but the created entry could not be found in the database! This shouldn't happen.");
    }
    
    /// <summary>
    /// Returns a city for the given ID in a country
    /// </summary>
    /// <param name="countryCode">ISO code of the country</param>
    /// <param name="cityId">ID of the city</param>
    /// <returns></returns>
    /// <exception cref="StateNotFoundException">if the state does not exist</exception>
    public async Task<CityWithCountryDto> GetCityInCountryAsync(uint cityId, string countryCode)
    {
        logger.LogDebug("Fetching city: {countryCode} - {id}", countryCode, cityId);
        var city = await cityRepository.GetByPrimaryKeyAsync(countryCode, cityId);
        if (city == null)
        {
            logger.LogWarning("City with ID '{id}' not found!", cityId);
            throw new CityNotFoundException(cityId);
        }
        
        logger.LogDebug("City '{cityName}' ({countryCode}) found.", city.Name, countryCode);
        return city.ToDtoWithCountry();
    }
    
    /// <summary>
    /// Returns a list of all cities in a country
    /// </summary>
    /// <param name="countryCode">ISO code of the country</param>
    /// <param name="cancellationToken">token to cancel the query</param>
    /// <returns>async enumerable of the cities that were found</returns>
    public IAsyncEnumerable<CityDto> GetCitiesInCountryAsync(string countryCode, CancellationToken cancellationToken)
    {
        logger.LogDebug("Requesting list of cities in '{countryCode}'...", countryCode);
        return cityRepository
            .QueryAsync(cancellationToken)
            .Where(s => s.CountryCode == countryCode)
            .Select(DtoMapper.ToDto);
    }
    
    /// <summary>
    /// Deletes a city with a given ISO code and state code
    /// </summary>
    /// <param name="isoCode">ISO code of the country</param>
    /// <param name="cityId">ID of the city within <paramref name="isoCode"/></param>
    /// <exception cref="StateNotFoundException"></exception>
    public async Task DeleteCityAsync(string isoCode, uint cityId)
    {
        logger.LogInformation("Deleting city with ID: {cityId}", cityId);
        var city = await cityRepository.GetByPrimaryKeyAsync(isoCode, cityId);
        if (city == null)
        {
            logger.LogWarning("City with ID '{cityId}' not found!", cityId);
            throw new CityNotFoundException(cityId);
        }
        
        cityRepository.Delete(city);
        await cityRepository.SaveChangesAsync();
        logger.LogInformation("City '{cityName}' ({isoCode} - {cityId}) was deleted successfully!", city.Name, city.CountryCode, city.Id);
    }

    #endregion
}