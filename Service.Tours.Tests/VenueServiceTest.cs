using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours.Locations;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Service.Tours.Exceptions;

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
        var dateTime = DateTime.Today;
        
        var request = new CreateVenueRequestDto
        {
            CountryCode = countryCode,
            CityId = cityId,
            CurrentName = name,
            TimeZone = timeZone,
            Latitude = latitude,
            Longitude = longitude,
        };

        VenueDo? savedVenueDo = null;
        _venueRepository
            .When(r => r.Add(Arg.Is<VenueDo>(c => c.CountryCode == countryCode && c.CurrentName == name && c.TimeZone == timeZone && c.Latitude == latitude && c.Longitude == longitude)))
            .Do(cb =>
            {
                savedVenueDo = cb.Arg<VenueDo>();
                savedVenueDo.Id = mockVenueId;
            });
        
        // call the service
        var resultId = await _service.CreateVenueAsync(request);
        Assert.Equal(resultId, mockVenueId);
        Assert.NotNull(savedVenueDo);
        Assert.Equal(mockVenueId, savedVenueDo.Id);

        // check the previous names. When a new venue is created, this should have exactly one entry with the current date as start and an open end
        var savedPreviousNames = savedVenueDo.PreviousNames.ToArray();
        var currentName = Assert.Single(savedPreviousNames);
        Assert.Equal(name, currentName.Name);
        Assert.Equal(DateOnly.FromDateTime(dateTime), currentName.From);
        Assert.Null(currentName.To);
        
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
            .GetByPrimaryKeyWithoutReferencesAsync(Arg.Is<uint>(c => c == mockVenue.Id))
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
        
        // verify mock calls
        await _venueRepository
            .Received(1)
            .GetByPrimaryKeyWithoutReferencesAsync(Arg.Is<uint>(c => c == mockVenue.Id));
    }

    [Fact]
    public async Task DeleteVenueAsync()
    {
        var mockVenue = GetSampleVenue();
        mockVenue.Id = 1234;
        
        // setup mocks
        _venueRepository
            .GetByPrimaryKeyWithoutReferencesAsync(Arg.Is<uint>(c => c == mockVenue.Id))
            .Returns(mockVenue);
        
        // call the service
        await _service.DeleteVenueAsync(mockVenue.Id);
        
        // verify mock calls
        await _venueRepository
            .Received(1)
            .GetByPrimaryKeyWithoutReferencesAsync(Arg.Is<uint>(c => c == mockVenue.Id));
        _venueRepository
            .Received(1)
            .Delete(Arg.Is<VenueDo>(c => c.Id == mockVenue.Id));
        await _venueRepository
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllVenuesAsync()
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
        
        var mockVenue1 = new VenueDo
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
        
        var mockVenue2 = new VenueDo
        {
            Id = 9999,
            CountryCode = "GER",
            StateCode = "BY",
            CityId = 1907,
            CurrentName = "City-Club",
            TimeZone = "Europe/Berlin",
            Latitude = 12,
            Longitude = 21,
            Country = countryGer,
            State = stateBy,
            City = cityAux
        };

        VenueDo[] mockVenues = [mockVenue1, mockVenue2];
        
        // setup mocks
        _venueRepository
            .QueryAsync(Arg.Any<CancellationToken>())
            .Returns(mockVenues.ToAsyncEnumerable());
        
        // call the service
        var venues = await _service.GetAllVenuesAsync().ToArrayAsync();
        Assert.Equal(2, venues.Length);
        
        AssertVenueDtoAgainstDo(mockVenue1, venues[0]);
        AssertVenueDtoAgainstDo(mockVenue2, venues[1]);
        
        // validate mock calls
        _venueRepository
            .Received(1)
            .QueryAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task GetAllVenuesAsync_Empty()
    {
        VenueDo[] mockVenues = [];
        
        // setup mocks
        _venueRepository
            .QueryAsync(Arg.Any<CancellationToken>())
            .Returns(mockVenues.ToAsyncEnumerable());
        
        // call the service
        var venues = await _service.GetAllVenuesAsync().ToArrayAsync();
        Assert.Empty(venues);
        
        // validate mock calls
        _venueRepository
            .Received(1)
            .QueryAsync(Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task DeleteVenueAsync_NotFound()
    {
        // setup mocks
        _venueRepository
            .GetByPrimaryKeyWithoutReferencesAsync(Arg.Any<uint>())
            .ThrowsAsync(new VenueNotFoundException(1234));
        
        // call the service
        var exception = await Assert.ThrowsAsync<VenueNotFoundException>(async () => await _service.DeleteVenueAsync(1234));
        Assert.Equal(1234u, exception.VenueId);
        
        // verify mock calls
        await _venueRepository
            .Received(1)
            .GetByPrimaryKeyWithoutReferencesAsync(Arg.Is<uint>(c => c == 1234u));
        _venueRepository
            .DidNotReceive()
            .Delete(Arg.Any<VenueDo>());
        await _venueRepository
            .DidNotReceive()
            .SaveChangesAsync();
    }

    #region Venue Names

    private VenueDo GetSampleVenue()
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
            City = cityAux,
            PreviousNames = [
                new PreviousVenueNameDo
                {
                    Id = 2,
                    Name = "SQL Arena",
                    From = DateOnly.ParseExact("2010-07-01", "yyyy-MM-dd"),
                    To = DateOnly.ParseExact("2015-06-30", "yyyy-MM-dd"),
                },
                new PreviousVenueNameDo
                {
                    Id = 1,
                    Name = "WWK Arena",
                    From = DateOnly.ParseExact("2015-07-01", "yyyy-MM-dd"),
                }
            ],
        };
        
        return mockVenue;
    }

    [Theory]
    [InlineData("Arena Augsburg", "2025-07-01")]
    public async Task AddVenueName_NewName(string newName, string fromDateStr)
    {
        var fromDate = DateOnly.ParseExact(fromDateStr, "yyyy-MM-dd");
        var newToDateForFirstEntry = fromDate.AddDays(-1);
        var mockVenue = GetSampleVenue();
        var oldName = mockVenue.CurrentName;

        mockVenue.PreviousNames =
        [
            new PreviousVenueNameDo
            {
                Id = 1,
                Name = "WWK Arena",
                From = DateOnly.ParseExact("2015-07-01", "yyyy-MM-dd"),
            }
        ];
        
        // setup mocks
        VenueDo? savedVenueDo = null;
        _venueRepository
            .GetByPrimaryKeyAsync(Arg.Is<uint>(c => c == mockVenue.Id))
            .Returns(mockVenue);
        _venueRepository
            .When(r => r.Update(Arg.Is<VenueDo>(v => v.Id == mockVenue.Id)))
            .Do(c => savedVenueDo = c.Arg<VenueDo>());
        
        // call the service
        Assert.Single(mockVenue.PreviousNames);
        var request = new AddVenueNameRequestDto
        {
            Name = newName,
            From = fromDate,
        };
        await _service.AddVenueNameAsync(request, mockVenue.Id);
        
        Assert.NotNull(savedVenueDo);
        Assert.Equal(newName, savedVenueDo.CurrentName);
        Assert.Equal(mockVenue.Id, savedVenueDo.Id);
        Assert.Equal(mockVenue.CountryCode, savedVenueDo.CountryCode);
        Assert.Equal(mockVenue.StateCode, savedVenueDo.StateCode);
        Assert.Equal(mockVenue.CityId, savedVenueDo.CityId);
        
        // there should now be a second name entry
        Assert.Equal(2, savedVenueDo.PreviousNames.Count);
        var latestName = savedVenueDo
            .PreviousNames
            .OrderBy(pn => pn.From)
            .Last();
        Assert.NotNull(latestName);
        Assert.Equal(newName, latestName.Name);
        Assert.Equal(fromDate, latestName.From);
        
        var firstName = savedVenueDo
            .PreviousNames
            .OrderBy(pn => pn.From)
            .First();
        Assert.NotNull(firstName);
        Assert.Equal(oldName, firstName.Name);
        Assert.Equal(mockVenue.PreviousNames.FirstOrDefault()?.From, firstName.From);
        Assert.Equal(newToDateForFirstEntry, firstName.To);
        
        // verify mock calls
        await _venueRepository
            .Received(1)
            .GetByPrimaryKeyAsync(Arg.Is<uint>(c => c == mockVenue.Id));
        _venueRepository
            .Received(1)
            .Update(Arg.Is<VenueDo>(v => v.Id == mockVenue.Id));
        await _venueRepository
            .Received(1)
            .SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteNameFromVenue()
    {
        var mockVenue = GetSampleVenue();
        
        // setup mocks
        VenueDo? savedVenueDo = null;
        _venueRepository
            .GetByPrimaryKeyAsync(Arg.Is<uint>(c => c == mockVenue.Id))
            .Returns(mockVenue);
        _venueRepository
            .When(r => r.Update(Arg.Is<VenueDo>(v => v.Id == mockVenue.Id)))
            .Do(c => savedVenueDo = c.Arg<VenueDo>());
        
        await _service.DeleteVenueNameAsync(mockVenue.Id, 2);
        Assert.NotNull(savedVenueDo);
        var remainingName = Assert.Single(savedVenueDo.PreviousNames);
        Assert.Equal(1u, remainingName.Id);
    }

    #endregion

    private void AssertVenueDtoAgainstDo(VenueDo expected, VenueDto actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.CurrentName, actual.CurrentName);
        Assert.Equal(expected.Latitude, actual.Latitude);
        Assert.Equal(expected.Longitude, actual.Longitude);
        Assert.Equal(expected.CountryCode, actual.CountryCode);
        Assert.Equal(expected.StateCode, actual.StateCode);
        Assert.Equal(expected.CityId, actual.CityId);
    }
}