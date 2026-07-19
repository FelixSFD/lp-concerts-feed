using Database.Tours.DataObjects;
using Database.Tours.Repositories;

namespace Database.Tours.Tests.Repositories;

public class SqlVenueRepositoryTest : ToursDbIntegrationTestsBase
{
    [Fact]
    public async Task CreateAndReadVenueWithNames()
    {
        var countryRepo = new SqlCountryRepository(DbContext);
        var cityRepo = new SqlCityRepository(DbContext);
        var repo = new SqlVenueRepository(DbContext);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland"
        };

        var testCity = new CityDo
        {
            CountryCode = countryGer.IsoCode,
            Name = "Test",
            NativeName = "Test",
        };
        
        countryRepo.Add(countryGer);
        await countryRepo.SaveChangesAsync();
        
        cityRepo.Add(testCity);
        await cityRepo.SaveChangesAsync();
        
        var testVenue = new VenueDo
        {
            CountryCode = countryGer.IsoCode,
            CityId = testCity.Id,
            CurrentName = "Test Venue",
            TimeZone = "Europe/Berlin",
            Latitude = 12.34m,
            Longitude = 45.67m,
            PreviousNames = [
                new PreviousVenueNameDo
                {
                    Id = 1,
                    Name = "Old Venue Name",
                    From = DateOnly.ParseExact("2021-01-01", "yyyy-MM-dd"),
                    To = DateOnly.ParseExact("2024-09-04", "yyyy-MM-dd"),
                },
                new PreviousVenueNameDo
                {
                    Id = 2,
                    Name = "Test Venue",
                    From = DateOnly.ParseExact("2024-09-05", "yyyy-MM-dd"),
                }
            ],
        };
        
        repo.Add(testVenue);
        await repo.SaveChangesAsync();
        
        // make sure auto-increment for the ID works
        Assert.NotEqual(0u, testVenue.Id);

        var retrievedVenue = await repo.GetByPrimaryKeyAsync(testVenue.Id);
        Assert.NotNull(retrievedVenue);
        Assert.Equal(testVenue.Id, retrievedVenue.Id);
        AssertCountriesEqual(countryGer, retrievedVenue.Country);
        AssertCitiesEqual(testCity, retrievedVenue.City);
        AssertVenuesEqual(testVenue, retrievedVenue);

        var oldName = retrievedVenue.PreviousNames.First();
        Assert.Equal(DateOnly.ParseExact("2021-01-01", "yyyy-MM-dd"), oldName.From);
        Assert.Equal(DateOnly.ParseExact("2024-09-04", "yyyy-MM-dd"), oldName.To);
        
        var currentName = retrievedVenue.PreviousNames.Last();
        Assert.Equal(DateOnly.ParseExact("2024-09-05", "yyyy-MM-dd"), currentName.From);
        Assert.Null(currentName.To);
        
        // add another name and make sure auto-increment for that works, too
        var newVenueName = new PreviousVenueNameDo
        {
            Name = "First venue name",
            Venue = testVenue,
            VenueId = testVenue.Id,
            From = DateOnly.ParseExact("2020-01-01", "yyyy-MM-dd"),
            To = DateOnly.ParseExact("2020-12-31", "yyyy-MM-dd"),
        };
        retrievedVenue.PreviousNames.Add(newVenueName);
        //repo.Update(retrievedVenue);
        await repo.SaveChangesAsync();
        Assert.NotEqual(0u, newVenueName.Id);
    }
    
    
    private static void AssertVenuesEqual(VenueDo expected, VenueDo actual)
    {
        Assert.Equal(expected.CountryCode, actual.CountryCode);
        Assert.Equal(expected.CurrentName, actual.CurrentName);
        Assert.Equal(expected.CityId, actual.CityId);
    }


    private static void AssertCountriesEqual(CountryDo expected, CountryDo actual)
    {
        Assert.Equal(expected.IsoCode, actual.IsoCode);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.NativeName, actual.NativeName);
    }
    
    
    private static void AssertStatesEqual(StateDo? expected, StateDo? actual)
    {
        if (expected == null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);

        Assert.Equal(expected.CountryCode, actual.CountryCode);
        Assert.Equal(expected.Code, actual.Code);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.NativeName, actual.NativeName);
        
        AssertCountriesEqual(expected.Country, actual.Country);
    }
    
    
    private static void AssertCitiesEqual(CityDo expected, CityDo actual)
    {
        Assert.Equal(expected.CountryCode, actual.CountryCode);
        Assert.Equal(expected.StateCode, actual.StateCode);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.NativeName, actual.NativeName);
        
        AssertStatesEqual(expected.State, actual.State);
        AssertCountriesEqual(expected.Country, actual.Country);
    }
}