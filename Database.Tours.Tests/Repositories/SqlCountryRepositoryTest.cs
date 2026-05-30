using Database.Tours.DataObjects;
using Database.Tours.Repositories;

namespace Database.Tours.Tests.Repositories;

public class SqlCountryRepositoryTest : ToursDbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var repo = new SqlCountryRepository(DbContext);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland"
        };
        
        var countryAut = new CountryDo
        {
            IsoCode = "AUT",
            Name = "Austria",
            NativeName = "Österreich"
        };
        
        repo.Add(countryGer);
        repo.Add(countryAut);
        
        await repo.SaveChangesAsync();

        var retrievedCountry = await repo.GetByPrimaryKeyAsync("GER");
        Assert.NotNull(retrievedCountry);
        AssertCountriesEqual(countryGer, retrievedCountry);
        
        repo.Delete(countryGer);
        
        await repo.SaveChangesAsync();
        
        retrievedCountry = await repo.GetByPrimaryKeyAsync("GER");
        Assert.Null(retrievedCountry);
        
        repo.Delete(countryAut);
        
        await repo.SaveChangesAsync();
    }
    
    
    [Fact]
    public async Task CreateAndGetCountryWithState()
    {
        var countryRepo = new SqlCountryRepository(DbContext);
        var stateRepo = new SqlStateRepository(DbContext);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland"
        };
        
        countryRepo.Add(countryGer);

        var stateBy = new StateDo
        {
            CountryCode = countryGer.IsoCode,
            Code = "BY",
            Name = "Bavaria",
            NativeName = "Bayern"
        };
        stateRepo.Add(stateBy);
        
        var stateRlp = new StateDo
        {
            CountryCode = countryGer.IsoCode,
            Code = "RLP",
            Name = "Rheinland-Pfalz",
            NativeName = "Rheinland-Pfalz"
        };
        stateRepo.Add(stateRlp);
        
        await stateRepo.SaveChangesAsync();
        
        var retrievedState = await stateRepo.GetByPrimaryKeyAsync(countryGer.IsoCode, stateBy.Code);
        Assert.NotNull(retrievedState);
        AssertStatesEqual(stateBy, retrievedState);
        
        // Cascade-delete should delete the states as well
        countryRepo.Delete(countryGer);
        await countryRepo.SaveChangesAsync();
        
        retrievedState = await stateRepo.GetByPrimaryKeyAsync(countryGer.IsoCode, stateBy.Code);
        Assert.Null(retrievedState);
    }
    
    
    [Fact]
    public async Task CreateAndGetCountryWithCityAndState()
    {
        var countryRepo = new SqlCountryRepository(DbContext);
        var stateRepo = new SqlStateRepository(DbContext);
        var cityRepo = new SqlCityRepository(DbContext);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland"
        };
        
        countryRepo.Add(countryGer);

        var stateBy = new StateDo
        {
            CountryCode = countryGer.IsoCode,
            Code = "BY",
            Name = "Bavaria",
            NativeName = "Bayern"
        };
        stateRepo.Add(stateBy);

        var cityMuc = new CityDo
        {
            CountryCode = countryGer.IsoCode,
            StateCode = stateBy.Code,
            Name = "Munich",
            NativeName = "München"
        };
        cityRepo.Add(cityMuc);
        
        await cityRepo.SaveChangesAsync();
        
        var retrievedCity = await cityRepo.GetByPrimaryKeyAsync(countryGer.IsoCode, stateBy.Code, cityMuc.Id);
        Assert.NotNull(retrievedCity);
        AssertCitiesEqual(cityMuc, retrievedCity);
        
        // Cascade-delete should delete the states and cities as well
        countryRepo.Delete(countryGer);
        await countryRepo.SaveChangesAsync();
        
        retrievedCity = await cityRepo.GetByPrimaryKeyAsync(countryGer.IsoCode, stateBy.Code, cityMuc.Id);
        Assert.Null(retrievedCity);
    }
    
    
    [Fact]
    public async Task CreateAndGetCountryWithCityAndNoState()
    {
        var countryRepo = new SqlCountryRepository(DbContext);
        var cityRepo = new SqlCityRepository(DbContext);

        var countryGer = new CountryDo
        {
            IsoCode = "GER",
            Name = "Germany",
            NativeName = "Deutschland"
        };
        
        countryRepo.Add(countryGer);

        var cityMuc = new CityDo
        {
            CountryCode = countryGer.IsoCode,
            Name = "Munich",
            NativeName = "München"
        };
        cityRepo.Add(cityMuc);
        
        await cityRepo.SaveChangesAsync();
        
        var retrievedCity = await cityRepo.GetByPrimaryKeyAsync(countryGer.IsoCode, null, cityMuc.Id);
        Assert.NotNull(retrievedCity);
        AssertCitiesEqual(cityMuc, retrievedCity);
        
        // Cascade-delete should delete the states and cities as well
        countryRepo.Delete(countryGer);
        await countryRepo.SaveChangesAsync();
        
        retrievedCity = await cityRepo.GetByPrimaryKeyAsync(countryGer.IsoCode, null, cityMuc.Id);
        Assert.Null(retrievedCity);
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