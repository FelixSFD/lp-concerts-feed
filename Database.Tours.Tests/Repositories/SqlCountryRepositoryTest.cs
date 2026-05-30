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


    private static void AssertCountriesEqual(CountryDo expected, CountryDo actual)
    {
        Assert.Equal(expected.IsoCode, actual.IsoCode);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.NativeName, actual.NativeName);
    }
}