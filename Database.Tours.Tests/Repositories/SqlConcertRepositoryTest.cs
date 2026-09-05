using Database.Tours.DataObjects;
using Database.Tours.Repositories;

namespace Database.Tours.Tests.Repositories;

public class SqlConcertRepositoryTest : ToursDbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var concertRepo = new SqlConcertRepository(DbContext);
        var concertTypeRepo = new SqlConcertTypeRepository(DbContext);
        var venueRepo = new SqlVenueRepository(DbContext);
        var tourRepo = new SqlTourRepository(DbContext);

        var tour = new TourDo
        {
            Id = "fz-world-tour",
            Name = "From Zero World Tour",
            Legs = []
        };

        var tourLegEu = new TourLegDo
        {
            TourId = tour.Id,
            Name = "European Tour",
            Id = "eu-1"
        };
        tour.Legs.Add(tourLegEu);
        
        var tourLegUs = new TourLegDo
        {
            TourId = tour.Id,
            Name = "North American Tour",
            Id = "us-1"
        };
        tour.Legs.Add(tourLegUs);
        
        tourRepo.Add(tour);

        var concertType = new ConcertTypeDo
        {
            Name = "Linkin Park Show"
        };
        concertTypeRepo.Add(concertType);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland"
        };
        var stateBy = new StateDo
        {
            CountryCode = countryGer.IsoCode,
            Code = "BY",
            Name = "Bavaria",
            NativeName = "Bayern",
            Country = countryGer
        };
        var cityAux = new CityDo
        {
            CountryCode = countryGer.IsoCode,
            StateCode = stateBy.Code,
            Name = "Augsburg",
            NativeName = "Augschburg",
            State = stateBy,
            Country = countryGer
        };
        var venue = new VenueDo
        {
            Id = 1,
            CountryCode = countryGer.IsoCode,
            StateCode = stateBy.Code,
            Country = countryGer,
            State = stateBy,
            City = cityAux,
            TimeZone = "Europe/Berlin",
            CurrentName = "WWK Arena"
        };
        venueRepo.Add(venue);
        
        await venueRepo.SaveChangesAsync();

        var concert = new ConcertDo
        {
            Id = "munich-2026-06-11",
            TourId = tour.Id,
            TourLegId = tourLegEu.Id,
            Type = concertType,
            VenueId = venue.Id,
            PostedStartTime = new DateTimeOffset(2026, 6, 11, 20, 0, 0, TimeSpan.FromHours(2)),
            DoorsTime = new DateTime(2026, 6, 11, 17, 30, 0),
            MainStageTime = new DateTime(2026, 6, 11, 20, 55, 0),
            Status = ConcertDo.ConcertStatus.Past,
            LpuEarlyEntryConfirmed = true,
        };
        
        concertRepo.Add(concert);
        await concertRepo.SaveChangesAsync();
        
        var retrievedConcert = await concertRepo.GetByPrimaryKeyAsync(concert.Id);
        Assert.NotNull(retrievedConcert);
        AssertConcertsEqual(concert, retrievedConcert);
    }

    [Fact]
    public async Task GetConcerts_WithFilter()
    {
        var concertRepo = new SqlConcertRepository(DbContext);
        var concertTypeRepo = new SqlConcertTypeRepository(DbContext);
        var venueRepo = new SqlVenueRepository(DbContext);
        var tourRepo = new SqlTourRepository(DbContext);
        var countryRepo = new SqlCountryRepository(DbContext);

        var tour = new TourDo
        {
            Id = "fz-world-tour-2",
            Name = "From Zero World Tour 2",
            Legs = []
        };
        tourRepo.Add(tour);

        var concertType = new ConcertTypeDo
        {
            Name = "Linkin Park Show 2"
        };
        concertTypeRepo.Add(concertType);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland"
        };
        var countryUs = new CountryDo
        {
            IsoCode = "USA",
            Name = "United States",
            NativeName = "United States"
        };
        countryRepo.Add(countryGer);
        countryRepo.Add(countryUs);

        var venueGer = new VenueDo
        {
            Id = 10,
            CountryCode = countryGer.IsoCode,
            Country = countryGer,
            City = new CityDo
            {
                CountryCode = countryGer.IsoCode,
                Name = "Munich",
                NativeName = "München",
                Country = countryGer
            },
            TimeZone = "Europe/Berlin",
            CurrentName = "Olympiahalle"
        };
        var venueUs = new VenueDo
        {
            Id = 11,
            CountryCode = countryUs.IsoCode,
            Country = countryUs,
            City = new CityDo
            {
                CountryCode = countryUs.IsoCode,
                Name = "New York",
                NativeName = "New York",
                Country = countryUs
            },
            TimeZone = "America/New_York",
            CurrentName = "Barclays Center"
        };
        venueRepo.Add(venueGer);
        venueRepo.Add(venueUs);

        var concert1 = new ConcertDo
        {
            Id = "concert-2026-05-01",
            TourId = tour.Id,
            Type = concertType,
            VenueId = venueGer.Id,
            PostedStartTime = new DateTimeOffset(2026, 5, 1, 20, 0, 0, TimeSpan.Zero),
            Status = ConcertDo.ConcertStatus.Planned,
        };
        var concert2 = new ConcertDo
        {
            Id = "concert-2026-06-01",
            TourId = tour.Id,
            Type = concertType,
            VenueId = venueGer.Id,
            PostedStartTime = new DateTimeOffset(2026, 6, 1, 20, 0, 0, TimeSpan.Zero),
            Status = ConcertDo.ConcertStatus.Planned,
        };
        var concert3 = new ConcertDo
        {
            Id = "concert-2026-07-01",
            TourId = tour.Id,
            Type = concertType,
            VenueId = venueUs.Id,
            PostedStartTime = new DateTimeOffset(2026, 7, 1, 20, 0, 0, TimeSpan.Zero),
            Status = ConcertDo.ConcertStatus.Planned,
        };

        concertRepo.Add(concert1);
        concertRepo.Add(concert2);
        concertRepo.Add(concert3);
        await concertRepo.SaveChangesAsync();

        // 1. No filter - should return all concerts
        var allConcerts = await concertRepo.GetConcerts(CancellationToken.None, (ConcertFilter?)null).ToListAsync();
        Assert.Contains(allConcerts, c => c.Id == concert1.Id);
        Assert.Contains(allConcerts, c => c.Id == concert2.Id);
        Assert.Contains(allConcerts, c => c.Id == concert3.Id);

        // 2. Filter by CountryCode
        var gerConcerts = await concertRepo.GetConcerts(CancellationToken.None, new ConcertFilter { CountryCode = "GER" }).ToListAsync();
        Assert.Contains(gerConcerts, c => c.Id == concert1.Id);
        Assert.Contains(gerConcerts, c => c.Id == concert2.Id);
        Assert.DoesNotContain(gerConcerts, c => c.Id == concert3.Id);

        // 3. Filter by Before
        var beforeJune = await concertRepo.GetConcerts(CancellationToken.None, new ConcertFilter
        {
            Before = new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero)
        }).ToListAsync();
        Assert.Contains(beforeJune, c => c.Id == concert1.Id);
        Assert.DoesNotContain(beforeJune, c => c.Id == concert2.Id);
        Assert.DoesNotContain(beforeJune, c => c.Id == concert3.Id);

        // 4. Filter by After
        var afterMay = await concertRepo.GetConcerts(CancellationToken.None, new ConcertFilter
        {
            After = new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero)
        }).ToListAsync();
        Assert.DoesNotContain(afterMay, c => c.Id == concert1.Id);
        Assert.Contains(afterMay, c => c.Id == concert2.Id);
        Assert.Contains(afterMay, c => c.Id == concert3.Id);

        // 5. Filter by Date range (After and Before)
        var midRange = await concertRepo.GetConcerts(CancellationToken.None, new ConcertFilter
        {
            After = new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero),
            Before = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero)
        }).ToListAsync();
        Assert.DoesNotContain(midRange, c => c.Id == concert1.Id);
        Assert.Contains(midRange, c => c.Id == concert2.Id);
        Assert.DoesNotContain(midRange, c => c.Id == concert3.Id);

        // 6. Filter by CountryCode + Date range
        var gerRange = await concertRepo.GetConcerts(CancellationToken.None, new ConcertFilter
        {
            CountryCode = "GER",
            After = new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero)
        }).ToListAsync();
        Assert.DoesNotContain(gerRange, c => c.Id == concert1.Id);
        Assert.Contains(gerRange, c => c.Id == concert2.Id);
        Assert.DoesNotContain(gerRange, c => c.Id == concert3.Id);
    }


    private static void AssertConcertsEqual(ConcertDo expected, ConcertDo actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.CustomTitle, actual.CustomTitle);
        Assert.Equal(expected.TourId, actual.TourId);
        Assert.Equal(expected.TourLegId, actual.TourLegId);
        Assert.Equal(expected.VenueId, actual.VenueId);
        Assert.Equal(expected.PostedStartTime, actual.PostedStartTime);
        Assert.Equal(expected.DoorsTime, actual.DoorsTime);
        Assert.Equal(expected.MainStageTime, actual.MainStageTime);
        Assert.Equal(expected.LpuEarlyEntryConfirmed, actual.LpuEarlyEntryConfirmed);
        Assert.Equal(expected.LpuEarlyEntryTime, actual.LpuEarlyEntryTime);
        Assert.Equal(expected.ConcertTypeId, actual.ConcertTypeId);
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.ScheduleImageFile, actual.ScheduleImageFile);
        Assert.Equal(expected.ExpectedSetDurationMinutes, actual.ExpectedSetDurationMinutes);
    }
}