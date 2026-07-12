using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;
using Service.Tours.Exceptions;

namespace Service.Tours;

public class LocationService(ICountryRepository countryRepository, ILogger<LocationService> logger)
{
    /// <summary>
    /// Creates a new country and saves the DB-context
    /// </summary>
    /// <param name="request"></param>
    /// <returns>ISO code of the created country</returns>
    public async Task<string> CreateCountry(CreateCountryRequestDto request)
    {
        logger.LogDebug("Creating country with ISO-code: {isoCode}", request.IsoCode);
        var newCountry = request.ToDto();
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
}