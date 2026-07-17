using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Service.Tours.Tests;

public class VenueServiceTest
{
    private readonly ICountryRepository _countryRepository;
    private readonly IStateRepository _stateRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly VenueService _service;

    public VenueServiceTest()
    {
        var logger = Substitute.For<ILogger<VenueService>>();
        _countryRepository = Substitute.For<ICountryRepository>();
        _stateRepository = Substitute.For<IStateRepository>();
        _cityRepository = Substitute.For<ICityRepository>();
        _venueRepository = Substitute.For<IVenueRepository>();
        _service = new VenueService(_venueRepository, logger);
    }

    [Theory]
    [InlineData("GER", 1, "Germany", "Deutschland", 123, 45.65, 1337)]
    [InlineData("AUT", 2, "Austria", "Österreich", 0.12, 12.3456789, 123)]
    public async Task CreateVenue(string countryCode, uint cityId, string name, string timeZone, decimal latitude, decimal longitude, uint mockVenueId)
    {
        var request = new CreateVenueRequestDto
        {
            CountryCode = countryCode,
            CityId = cityId,
            CurrentName = name,
            TimeZone = timeZone,
            Latitude = latitude,
            Longitude = longitude,
        };
        
        _venueRepository
            .When(r => r.Add(Arg.Is<VenueDo>(c => c.CountryCode == countryCode && c.CurrentName == name && c.TimeZone == timeZone && c.Latitude == latitude && c.Longitude == longitude)))
            .Do(cb => cb.Arg<VenueDo>().Id = mockVenueId);
        
        // call the service
        var resultId = await _service.CreateVenueAsync(request);
        Assert.Equal(resultId, mockVenueId);
        
        // verify mock calls
        _venueRepository
            .Received(1)
            .Add(Arg.Is<VenueDo>(c => c.CountryCode == countryCode && c.CurrentName == name && c.TimeZone == timeZone && c.Latitude == latitude && c.Longitude == longitude));
        await _venueRepository
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task GetVenueById()
    {
        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland",
        };

        var stateBy = new StateDo
        {
            CountryCode = "GER",
            Code = "BY",
            Name = "Bavaria",
            NativeName = "Bayern",
        };

        var cityAux = new CityDo
        {
            Id = 1907,
            CountryCode = "GER",
            StateCode = "BY",
            Name = "Augsburg",
            NativeName = "Augschburg",
            Country = countryGer,
            State = stateBy,
        };
        
        var mockVenue = new VenueDo
        {
            Id = 1337,
            CountryCode = "GER",
            StateCode = "BY",
            CityId = 1907,
            CurrentName = "WWK Arena",
            TimeZone = "Europe/Berlin",
            Latitude = 12,
            Longitude = 21,
            Country = countryGer,
            State = stateBy,
            City = cityAux
        };
        
        // setup mocks
        _venueRepository
            .GetByPrimaryKeyAsync(Arg.Is<uint>(c => c == mockVenue.Id))
            .Returns(mockVenue);
        
        // call the service
        var result = await _service.GetVenueByIdAsync(mockVenue.Id);
        Assert.Equal(mockVenue.Id, result.Id);
        Assert.Equal(mockVenue.CountryCode, result.CountryCode);
        Assert.Equal(mockVenue.StateCode, result.StateCode);
        Assert.Equal(mockVenue.CityId, result.CityId);
        Assert.Equal(mockVenue.Latitude, result.Latitude);
        Assert.Equal(mockVenue.Longitude, result.Longitude);
        Assert.Equal(mockVenue.CurrentName, result.CurrentName);
    }
}