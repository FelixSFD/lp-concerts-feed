using Common.Database;
using Common.Database.Repositories;
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
            PostedStartTime = new DateTimeOffset(2026, 6, 11, 20, 0, 0, TimeSpan.FromHours(2)).UtcDateTime,
            DoorsTime = new DateTimeOffset(2026, 6, 11, 17, 30, 0, TimeSpan.FromHours(2)).UtcDateTime,
            MainStageTime = new DateTimeOffset(2026, 6, 11, 20, 55, 0, TimeSpan.FromHours(2)).UtcDateTime,
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
    public async Task GetConcerts_WithPaginationAndFilter()
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
            PostedStartTime = new DateTimeOffset(2026, 5, 1, 20, 0, 0, TimeSpan.Zero).UtcDateTime,
            Status = ConcertDo.ConcertStatus.Planned,
        };
        var concert2 = new ConcertDo
        {
            Id = "concert-2026-06-01",
            TourId = tour.Id,
            Type = concertType,
            VenueId = venueGer.Id,
            PostedStartTime = new DateTimeOffset(2026, 6, 1, 20, 0, 0, TimeSpan.Zero).UtcDateTime,
            Status = ConcertDo.ConcertStatus.Planned,
        };
        var concert3 = new ConcertDo
        {
            Id = "concert-2026-07-01",
            TourId = tour.Id,
            Type = concertType,
            VenueId = venueUs.Id,
            PostedStartTime = new DateTimeOffset(2026, 7, 1, 20, 0, 0, TimeSpan.Zero).UtcDateTime,
            Status = ConcertDo.ConcertStatus.Planned,
        };

        concertRepo.Add(concert1);
        concertRepo.Add(concert2);
        concertRepo.Add(concert3);
        await concertRepo.SaveChangesAsync();

        // 1. No pagination params - default pagination
        var defaultPage = await concertRepo.GetConcertsAsync(CancellationToken.None);
        Assert.Equal(3, defaultPage.TotalCount);
        var defaultList = await defaultPage.Results.ToListAsync();
        Assert.Equal(3, defaultList.Count);
        Assert.Contains(defaultList, c => c.Id == concert1.Id);
        Assert.Contains(defaultList, c => c.Id == concert2.Id);
        Assert.Contains(defaultList, c => c.Id == concert3.Id);

        // 2. Page 1: Skip = 0, Take = 2, sorted by date asc
        var page1 = await concertRepo.GetConcertsAsync(
            CancellationToken.None,
            orderBy: [new SortDescriptor("date")],
            paginationParams: new PaginationParams(0, 2));
        Assert.Equal(3, page1.TotalCount);
        var page1List = await page1.Results.ToListAsync();
        Assert.Equal(2, page1List.Count);
        Assert.Equal(concert1.Id, page1List[0].Id);
        Assert.Equal(concert2.Id, page1List[1].Id);
        Assert.NotNull(page1List[0].Venue);
        Assert.NotNull(page1List[0].Type);

        // 3. Page 2: Skip = 2, Take = 2, sorted by date asc
        var page2 = await concertRepo.GetConcertsAsync(
            CancellationToken.None,
            orderBy: [new SortDescriptor("date")],
            paginationParams: new PaginationParams(2, 2));
        Assert.Equal(3, page2.TotalCount);
        var page2List = await page2.Results.ToListAsync();
        Assert.Single(page2List);
        Assert.Equal(concert3.Id, page2List[0].Id);

        // 4. Page out of bounds: Skip = 4, Take = 2
        var emptyPage = await concertRepo.GetConcertsAsync(
            CancellationToken.None,
            orderBy: [new SortDescriptor("date")],
            paginationParams: new PaginationParams(4, 2));
        Assert.Equal(3, emptyPage.TotalCount);
        var emptyPageList = await emptyPage.Results.ToListAsync();
        Assert.Empty(emptyPageList);

        // 5. Pagination with filter: CountryCode = "GER", Page 1: Skip = 0, Take = 1
        var gerPage1 = await concertRepo.GetConcertsAsync(
            CancellationToken.None,
            filter: new Filters.ConcertFilter { CountryCode = "GER" },
            orderBy: [new SortDescriptor("date")],
            paginationParams: new PaginationParams(0, 1));
        Assert.Equal(2, gerPage1.TotalCount);
        var gerPage1List = await gerPage1.Results.ToListAsync();
        Assert.Single(gerPage1List);
        Assert.Equal(concert1.Id, gerPage1List[0].Id);

        // 6. Pagination with filter: CountryCode = "GER", Page 2: Skip = 1, Take = 1
        var gerPage2 = await concertRepo.GetConcertsAsync(
            CancellationToken.None,
            filter: new Filters.ConcertFilter { CountryCode = "GER" },
            orderBy: [new SortDescriptor("date")],
            paginationParams: new PaginationParams(1, 1));
        Assert.Equal(2, gerPage2.TotalCount);
        var gerPage2List = await gerPage2.Results.ToListAsync();
        Assert.Single(gerPage2List);
        Assert.Equal(concert2.Id, gerPage2List[0].Id);
    }
    
    
    [Fact]
    public async Task GetOnThisDay()
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
            Id = "concert-2026-05-11",
            TourId = tour.Id,
            Type = concertType,
            VenueId = venueGer.Id,
            PostedStartTime = new DateTimeOffset(2026, 5, 11, 20, 0, 0, TimeSpan.Zero).UtcDateTime,
            Status = ConcertDo.ConcertStatus.Planned,
        };
        var concert2 = new ConcertDo
        {
            Id = "concert-2026-06-01",
            TourId = tour.Id,
            Type = concertType,
            VenueId = venueGer.Id,
            PostedStartTime = new DateTimeOffset(2026, 6, 1, 20, 0, 0, TimeSpan.Zero).UtcDateTime,
            Status = ConcertDo.ConcertStatus.Planned,
        };
        var concert3 = new ConcertDo
        {
            Id = "concert-2025-06-01",
            TourId = tour.Id,
            Type = concertType,
            VenueId = venueUs.Id,
            PostedStartTime = new DateTimeOffset(2025, 6, 1, 20, 0, 0, TimeSpan.Zero).UtcDateTime,
            Status = ConcertDo.ConcertStatus.Planned,
        };

        concertRepo.Add(concert1);
        concertRepo.Add(concert2);
        concertRepo.Add(concert3);
        await concertRepo.SaveChangesAsync();

        // check month 06 and day 01 -> 2 results
        var concerts = await concertRepo.GetOnThisDay(CancellationToken.None, 6, 1).ToArrayAsync();
        Assert.Equal(2, concerts.Length);
        Assert.Contains(concerts, c => c.Id == concert2.Id);
        Assert.Contains(concerts, c => c.Id == concert3.Id);
        
        // check month 05 and day 11 -> 1 result
        concerts = await concertRepo.GetOnThisDay(CancellationToken.None, 5, 11).ToArrayAsync();
        Assert.Single(concerts);
        Assert.Contains(concerts, c => c.Id == concert1.Id);
        
        // check month 01 and day 31 -> 0 results
        concerts = await concertRepo.GetOnThisDay(CancellationToken.None, 1, 31).ToArrayAsync();
        Assert.Empty(concerts);
    }
    
    
    [Fact]
    public async Task GetByWikiPageIdAsync()
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
            PostedStartTime = new DateTimeOffset(2026, 6, 11, 20, 0, 0, TimeSpan.FromHours(2)).UtcDateTime,
            DoorsTime = new DateTimeOffset(2026, 6, 11, 17, 30, 0, TimeSpan.FromHours(2)).UtcDateTime,
            MainStageTime = new DateTimeOffset(2026, 6, 11, 20, 55, 0, TimeSpan.FromHours(2)).UtcDateTime,
            Status = ConcertDo.ConcertStatus.Past,
            LpuEarlyEntryConfirmed = true,
            LinkinpediaUrl = "https://linkinpedia.com/page/Live:20260611"
        };
        
        concertRepo.Add(concert);
        await concertRepo.SaveChangesAsync();
        
        var retrievedConcert = await concertRepo.GetConcertsByWikiPageId("Live:20260611").FirstOrDefaultAsync();
        Assert.NotNull(retrievedConcert);
        AssertConcertsEqual(concert, retrievedConcert);
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