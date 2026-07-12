using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using Service.Tours.Exceptions;

namespace Service.Tours.Tests;

public class LocationServiceTest
{
    private readonly ICountryRepository _countryRepository;
    private readonly IStateRepository _stateRepository;
    private readonly LocationService _service;

    public LocationServiceTest()
    {
        var logger = Substitute.For<ILogger<LocationService>>();
        _countryRepository = Substitute.For<ICountryRepository>();
        _stateRepository = Substitute.For<IStateRepository>();
        _service = new LocationService(_countryRepository, _stateRepository, logger);
    }

    [Theory]
    [InlineData("GER", "Germany", "Deutschland")]
    [InlineData("AUT", "Austria", "Österreich")]
    public async Task CreateCountry(string isoCode, string name, string nativeName)
    {
        var request = new CreateCountryRequestDto
        {
            IsoCode = isoCode,
            Name = name,
            NativeName = nativeName,
        };
        
        // call the service
        var resultIsoCode = await _service.CreateCountry(request);
        
        Assert.Equal(isoCode, resultIsoCode);
        
        // verify mock calls
        _countryRepository
            .Received(1)
            .Add(Arg.Is<CountryDo>(c => c.IsoCode == isoCode && c.Name == name && c.NativeName == nativeName));
        await _countryRepository
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllCountries()
    {
        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland",
        };
        
        var countryAut = new CountryDo
        {
            IsoCode = "AUT",
            Name = "Austria",
            NativeName = "Österreich",
        };

        CountryDo[] mockCountries = [countryGer, countryAut];
        
        _countryRepository
            .Configure()
            .QueryAsync(Arg.Any<CancellationToken>())
            .Returns(mockCountries.ToAsyncEnumerable());

        var result = await _service.GetCountriesAsync(CancellationToken.None).ToArrayAsync();
        Assert.Equal(mockCountries.Length, result.Length);
    }
    
    [Fact]
    public async Task GetAllCountries_Empty()
    {
        CountryDo[] mockCountries = [];
        
        _countryRepository
            .Configure()
            .QueryAsync(Arg.Any<CancellationToken>())
            .Returns(mockCountries.ToAsyncEnumerable());

        var result = await _service.GetCountriesAsync(CancellationToken.None).ToArrayAsync();
        Assert.Empty(result);
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
            .Returns(Task.FromResult<CountryDo?>(null));

        // Call the service
        var exception = await Assert.ThrowsAsync<CountryNotFoundException>(async () => await _service.GetCountry("AAA"));
        Assert.NotNull(exception);
        Assert.Equal("AAA", exception.CountryCode);
        
        // verify mock calls
        await _countryRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is("AAA"));
    }

    [Fact]
    public async Task DeleteCountry_ExistingCountry()
    {
        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland",
        };
        
        // setup mocks
        _countryRepository
            .Configure()
            .GetByPrimaryKeyAsync(countryGer.IsoCode)
            .Returns(countryGer);
        
        // call the service
        await _service.DeleteCountryAsync(countryGer.IsoCode);
        
        // verify mock calls
        await _countryRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is(countryGer.IsoCode));
        _countryRepository
            .Received(1)
            .Delete(Arg.Is<CountryDo>(c => c.IsoCode == countryGer.IsoCode));
        await _countryRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    [Fact]
    public async Task DeleteCountry_CountryNotFound()
    {
        // setup mocks
        _countryRepository
            .Configure()
            .GetByPrimaryKeyAsync("AAA")
            .Returns(Task.FromResult<CountryDo?>(null));
        
        // call the service
        var exception = await Assert.ThrowsAsync<CountryNotFoundException>(async () => await _service.DeleteCountryAsync("AAA"));
        Assert.NotNull(exception);
        Assert.Equal("AAA", exception.CountryCode);
        
        // verify mock calls
        await _countryRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is("AAA"));
        _countryRepository
            .DidNotReceive()
            .Delete(Arg.Any<CountryDo>());
        await _countryRepository
            .DidNotReceive()
            .SaveChangesAsync();
    }
    
    [Theory]
    [InlineData("GER", "BY", "Bavaria", "Bayern")]
    [InlineData("GER", "BW", "Baden-Württemberg", "Baden-Württemberg")]
    public async Task CreateState(string countryCode, string stateCode, string name, string nativeName)
    {
        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland",
        };

        var mockState = new StateDo
        {
            CountryCode = countryCode,
            Code = stateCode,
            Name = name,
            NativeName = nativeName,
            Country = countryGer
        };
        
        var request = new CreateStateRequestDto
        {
            Code = stateCode,
            Name = name,
            NativeName = nativeName,
        };

        // setup mocks
        _countryRepository
            .GetByPrimaryKeyAsync(Arg.Is(countryCode))
            .Returns(countryGer);
        _stateRepository
            .GetByPrimaryKeyAsync(Arg.Is(countryCode), Arg.Is(stateCode))
            .Returns(mockState);
        
        // call the service
        var resultStateDto = await _service.CreateState(request, countryCode);
        Assert.NotNull(resultStateDto);
        Assert.Equal(countryCode, resultStateDto.CountryCode);
        Assert.Equal(stateCode, resultStateDto.Code);
        Assert.Equal(name, resultStateDto.Name);
        Assert.Equal(nativeName, resultStateDto.NativeName);
        
        // verify mock calls
        await _stateRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is(countryCode), Arg.Is(stateCode));
        _stateRepository
            .Received(1)
            .Add(Arg.Is<StateDo>(c => c.CountryCode == countryCode && c.Name == name && c.NativeName == nativeName && c.Code == stateCode));
        await _stateRepository
            .Received(1)
            .SaveChangesAsync();
    }
    
    [Theory]
    [InlineData("GER", "Germany", "Deutschland", "BY", "Bavaria", "Bayern")]
    [InlineData("GER", "Germany", "Deutschland", "BW", "Baden-Württemberg", "Nähe Stuttgart")]
    [InlineData("AUT", "Austria", "Österreich", "TT", "Test", "Testing")]
    public async Task GetState_ByCode(string countryCode, string mockCountryName, string mockCountryNativeName, string stateCode, string mockStateName, string mockStateNativeName)
    {
        var mockCountry = new CountryDo
        {
            IsoCode = countryCode,
            Name = mockCountryName,
            NativeName = mockCountryNativeName,
        };
        
        var mockState = new StateDo
        {
            CountryCode = countryCode,
            Code = stateCode,
            Name = mockStateName,
            NativeName = mockStateNativeName,
            Country = mockCountry
        };

        // setup mocks
        _stateRepository
            .Configure()
            .GetByPrimaryKeyAsync(Arg.Is(countryCode), Arg.Is(stateCode))
            .Returns(mockState);

        // Call the service
        var stateWithCountry = await _service.GetStateInCountryAsync(countryCode, stateCode);
        
        // verify result
        Assert.NotNull(stateWithCountry);
        Assert.Equal(mockState.Name, stateWithCountry.Name);
        Assert.Equal(mockState.NativeName, stateWithCountry.NativeName);
        Assert.Equal(mockState.CountryCode, stateWithCountry.CountryCode);
        Assert.Equal(mockState.Code, stateWithCountry.Code);
        Assert.Equal(mockCountry.IsoCode, stateWithCountry.Country.IsoCode);
        Assert.Equal(mockCountry.Name, stateWithCountry.Country.Name);
        Assert.Equal(mockCountry.NativeName, stateWithCountry.Country.NativeName);
        
        // verify mock calls
        await _stateRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is(countryCode), Arg.Is(stateCode));
    }
    
    [Fact]
    public async Task GetState_NotFound()
    {
        // setup mocks
        _stateRepository
            .Configure()
            .GetByPrimaryKeyAsync("AAA", "BB")
            .Returns(Task.FromResult<StateDo?>(null));

        // Call the service
        var exception = await Assert.ThrowsAsync<StateNotFoundException>(async () => await _service.GetStateInCountryAsync("AAA", "BB"));
        Assert.NotNull(exception);
        Assert.Equal("AAA", exception.CountryCode);
        Assert.Equal("BB", exception.StateCode);
        
        // verify mock calls
        await _stateRepository
            .Received(1)
            .GetByPrimaryKeyAsync("AAA", "BB");
    }
}