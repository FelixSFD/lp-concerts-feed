using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using Service.Tours.Exceptions;

namespace Service.Tours;

public class LocationService(ICountryRepository countryRepository, ILogger<LocationService> logger)
{
    /// <summary>
    /// Returns a country for the given ISO-code
    /// </summary>
    /// <param name="isoCode"></param>
    /// <returns></returns>
    /// <exception cref="CountryNotFoundException"></exception>
    public async Task<CountryDo> GetCountry(string isoCode)
    {
        logger.LogDebug("Fetching country with ISO-code: {isoCode}", isoCode);
        var country = await countryRepository.GetByPrimaryKeyAsync(isoCode);
        if (country == null)
        {
            logger.LogWarning("Country '{isoCode}' not found!", isoCode);
            throw new CountryNotFoundException(isoCode);
        }
        
        logger.LogDebug("Country '{countryName}' ({isoCode}) found.", country.Name, isoCode);
        return country;
    }
}