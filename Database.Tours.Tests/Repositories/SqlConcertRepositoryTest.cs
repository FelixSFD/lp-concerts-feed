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