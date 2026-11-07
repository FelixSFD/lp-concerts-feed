using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Service.Tours.Tests;

public class UserStatsServiceTest
{
    private readonly IConcertRepository _concertRepository;
    private readonly UserStatsService _userStatsService;

    public UserStatsServiceTest()
    {
        var logger = Substitute.For<ILogger<UserStatsService>>();
        _concertRepository = Substitute.For<IConcertRepository>();
        _userStatsService = new UserStatsService(_concertRepository, logger);
    }


    [Fact]
    public async Task GetAttendedConcerts()
    {
        var userId = Guid.NewGuid().ToString();
        var mockConcert1 = CreateMockConcert();
        var mockConcert2 = CreateMockConcert();
        
        ConcertDo[] mockAttendedConcerts = [mockConcert1, mockConcert2];
        
        // setup mocks
        _concertRepository
            .GetAttendedConcertsForAsync(userId)
            .Returns(mockAttendedConcerts.ToAsyncEnumerable());
        
        var attendedConcertsStats = await _userStatsService.GetAttendedConcerts(userId);
        Assert.Equal(mockAttendedConcerts.Length, attendedConcertsStats.Concerts.Length);
        Assert.Equal(mockAttendedConcerts[0].Id, attendedConcertsStats.Concerts[0].Id);
        Assert.Equal(mockAttendedConcerts[1].Id, attendedConcertsStats.Concerts[1].Id);
        Assert.Equal(mockAttendedConcerts.Length, attendedConcertsStats.NumberOfConcerts);
        Assert.Equal(1, attendedConcertsStats.NumberOfCities);
        Assert.Equal(1, attendedConcertsStats.NumberOfCountries);
        Assert.Equal(1, attendedConcertsStats.NumberOfVenues);
    }
    
    [Fact]
    public async Task GetAttendedConcerts_None()
    {
        var userId = Guid.NewGuid().ToString();

        ConcertDo[] mockAttendedConcerts = [];
        
        // setup mocks
        _concertRepository
            .GetAttendedConcertsForAsync(userId)
            .Returns(mockAttendedConcerts.ToAsyncEnumerable());
        
        var attendedConcertsStats = await _userStatsService.GetAttendedConcerts(userId);
        Assert.Empty(attendedConcertsStats.Concerts);
        Assert.Equal(0, attendedConcertsStats.NumberOfConcerts);
        Assert.Equal(0, attendedConcertsStats.NumberOfCities);
        Assert.Equal(0, attendedConcertsStats.NumberOfCountries);
        Assert.Equal(0, attendedConcertsStats.NumberOfVenues);
    }
    
    
    private static ConcertDo CreateMockConcert()
    {
        var mockConcertId = Guid.NewGuid().ToString();
        var mockConcertType = new ConcertTypeDo
        {
            Id = 1337u,
            Name = "Linkin Park",
        };
        var mockTour = new TourDo
        {
            Id = "fz-world-tour",
            Name = "From Zero World Tour",
            Legs = [],
        };
        var mockTourLegEu = new TourLegDo
        {
            Id = "europe-2025",
            TourId = mockTour.Id,
            Tour = mockTour,
            Name = "European Tour 2025",
        };
        mockTour.Legs.Add(mockTourLegEu);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland",
        };
        var cityMuc = new CityDo
        {
            CountryCode = countryGer.IsoCode,
            Name = "Munich",
            NativeName = "München",
            Country = countryGer,
        };
        var mockVenue = new VenueDo
        {
            Id = 1900u,
            CountryCode = countryGer.IsoCode,
            CityId = cityMuc.Id,
            City = cityMuc,
            CurrentName = "Allianz Arena",
            TimeZone = "Europe/Berlin",
            Latitude = 12.34m,
            Longitude = 0.123m,
            Country = countryGer,
        };
        
        var mockConcert = new ConcertDo
        {
            Id = mockConcertId,
            TourId = mockTour.Id,
            TourLegId = mockTourLegEu.Id,
            ConcertTypeId = mockConcertType.Id,
            Type = mockConcertType,
            PostedStartTime = new DateTimeOffset(2026, 6, 30, 20, 0, 0, TimeSpan.FromHours(2)).UtcDateTime,
            MainStageTime = new DateTimeOffset(2026, 6, 30, 20, 25, 0, TimeSpan.FromHours(2)).UtcDateTime,
            DoorsTime = new DateTimeOffset(2026, 6, 30, 16, 0, 0, TimeSpan.FromHours(2)).UtcDateTime,
            LpuEarlyEntryTime = new DateTimeOffset(2026, 6, 30, 15, 30, 0, TimeSpan.FromHours(2)).UtcDateTime,
            LpuEarlyEntryConfirmed = true,
            VenueId = mockVenue.Id,
            Venue = mockVenue,
            Status = ConcertDo.ConcertStatus.Planned,
            ExpectedSetDurationMinutes = 120,
            CustomTitle = "Final Show of the tour",
            ScheduleImageFile = "test.jpg",
        };
        
        return mockConcert;
    }
}