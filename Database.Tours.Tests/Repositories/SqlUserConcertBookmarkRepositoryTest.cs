using Database.Tours.DataObjects;
using Database.Tours.Repositories;

namespace Database.Tours.Tests.Repositories;

public class SqlUserConcertBookmarkRepositoryTest : ToursDbIntegrationTestsBase
{
    [Fact]
    public async Task GetByUserIdAndConcertIdAsync()
    {
        var repo = new SqlUserConcertBookmarkRepository(DbContext);
        
        var userId = "testUserId";
        var concertId = "testConcertId";
        
        await CreateMockConcert(concertId);
        
        var mockBookmark = new UserConcertBookmarkDo
        {
            ConcertId = concertId,
            UserId = userId
        };
        
        repo.Add(mockBookmark);
        await repo.SaveChangesAsync();
        
        var result = await repo.GetByUserIdAndConcertIdAsync(userId, concertId);
        Assert.NotNull(result);

        repo.Delete(mockBookmark);
        await repo.SaveChangesAsync();

        result = await repo.GetByUserIdAndConcertIdAsync(userId, concertId);
        Assert.Null(result);
    }

    private async Task CreateMockConcert(string concertId)
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
            Id = concertId,
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
    }
}