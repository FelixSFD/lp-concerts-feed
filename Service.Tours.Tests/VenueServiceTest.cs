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
        var resultId = await _service.CreateVenue(request);
        Assert.Equal(resultId, mockVenueId);
        
        // verify mock calls
        _venueRepository
            .Received(1)
            .Add(Arg.Is<VenueDo>(c => c.CountryCode == countryCode && c.CurrentName == name && c.TimeZone == timeZone && c.Latitude == latitude && c.Longitude == longitude));
        await _venueRepository
            .Received(1)
            .SaveChangesAsync();
    }
}