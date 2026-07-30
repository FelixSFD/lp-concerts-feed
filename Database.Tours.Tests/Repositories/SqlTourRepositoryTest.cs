using Database.Tours.DataObjects;
using Database.Tours.Repositories;

namespace Database.Tours.Tests.Repositories;

public class SqlTourRepositoryTest : ToursDbIntegrationTestsBase
{
    [Fact]
    public async Task GetByIdAsync()
    {
        var repo = new SqlTourRepository(DbContext);

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
        
        repo.Add(tour);
        
        await repo.SaveChangesAsync();

        var retrievedTour = await repo.GetByPrimaryKeyAsync(tour.Id);
        Assert.NotNull(retrievedTour);
        AssertToursEqual(tour, retrievedTour);
        
        repo.Delete(tour);
        
        await repo.SaveChangesAsync();
        
        retrievedTour = await repo.GetByPrimaryKeyAsync(tour.Id);
        Assert.Null(retrievedTour);
    }


    private static void AssertTourLegsEqual(TourLegDo expected, TourLegDo actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.TourId, actual.TourId);
    }
    
    private static void AssertToursEqual(TourDo expected, TourDo actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        
        var expectedLegs = expected.Legs.ToArray();
        var actualLegs = actual.Legs.ToArray();
        Assert.Equal(expectedLegs.Length, actualLegs.Length);
        for (var i = 0; i < expectedLegs.Length; i++)
        {
            AssertTourLegsEqual(expectedLegs[i], actualLegs[i]);
        }
    }
}