using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;
using Service.Tours.Exceptions;

namespace Service.Tours.Tests;

public class LocationServiceTest
{
    private readonly ICountryRepository _countryRepository;
    private readonly LocationService _service;

    public LocationServiceTest()
    {
        var logger = Substitute.For<ILogger<LocationService>>();
        _countryRepository = Substitute.For<ICountryRepository>();
        _service = new LocationService(_countryRepository, logger);
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
    
    [Fact]
    public async Task GetCountry_ByIsoCode_NotFound()
    {
        // setup mocks
        _countryRepository
            .Configure()
            .GetByPrimaryKeyAsync("AAA")
            .ThrowsAsync(new CountryNotFoundException("AAA"));

        // Call the service
        var exception = await Assert.ThrowsAsync<CountryNotFoundException>(async () => await _service.GetCountry("AAA"));
        Assert.NotNull(exception);
        Assert.Equal("AAA", exception.CountryCode);
        
        // verify mock calls
        await _countryRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is("AAA"));
    }
}