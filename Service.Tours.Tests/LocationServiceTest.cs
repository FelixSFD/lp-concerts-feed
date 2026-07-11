using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;

namespace Service.Tours.Tests;

public class LocationServiceTest
{
    private ILogger<LocationService> _logger;
    private ICountryRepository _countryRepository;
    private LocationService _service;

    public LocationServiceTest()
    {
        _logger = Substitute.For<ILogger<LocationService>>();
        _countryRepository = Substitute.For<ICountryRepository>();
        _service = new LocationService(_countryRepository, _logger);
    }
    
    [Theory]
    [InlineData("GER", "Germany", "Deutschland")]
    [InlineData("AUT", "Austria", "Österreich")]
    public async Task GetCountry_ByIsoCode(string isoCode, string mockName, string mockNativeName)
    {
        var mockCountry = new CountryDo
        {
            IsoCode = isoCode,
            Name = mockName,
            NativeName = mockNativeName,
        };

        // setup mocks
        _countryRepository
            .Configure()
            .GetByPrimaryKeyAsync(Arg.Is(isoCode))
            .Returns(mockCountry);

        // Call the service
        var country = await _service.GetCountry(isoCode);
        
        // verify result
        Assert.NotNull(country);
        Assert.Equal(mockCountry.Name, country.Name);
        Assert.Equal(mockCountry.NativeName, country.NativeName);
        Assert.Equal(mockCountry.IsoCode, country.IsoCode);
        
        // verify mock calls
        await _countryRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is(isoCode));
    }
}