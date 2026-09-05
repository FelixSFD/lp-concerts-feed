using Common.Database.DataObjects;
using Database.Tours.DataObjects;
using Microsoft.EntityFrameworkCore;

namespace Database.Tours.Tests;

public class TimestampTests
{
    private static ToursDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ToursDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ToursDbContext(options);
    }

    [Fact]
    public void AllEntitiesInToursDbContext_ImplementITimestampedDataObject()
    {
        using var context = CreateInMemoryDbContext();
        var entityTypes = context.Model.GetEntityTypes().ToArray();

        Assert.NotEmpty(entityTypes);
        foreach (var entityType in entityTypes)
        {
            Assert.True(
                typeof(ITimestampedDataObject).IsAssignableFrom(entityType.ClrType),
                $"Entity type '{entityType.ClrType.Name}' does not implement ITimestampedDataObject.");
        }
    }

    [Fact]
    public void SaveChanges_SetsCreatedAtAndLeavesUpdatedAtNull_OnNewEntity()
    {
        using var context = CreateInMemoryDbContext();
        var country = new CountryDo
        {
            IsoCode = "USA",
            Name = "United States",
            NativeName = "United States"
        };

        var before = DateTimeOffset.UtcNow;
        context.Countries.Add(country);
        context.SaveChanges();
        var after = DateTimeOffset.UtcNow;

        Assert.True(country.CreatedAt >= before && country.CreatedAt <= after);
        Assert.Null(country.UpdatedAt);
    }

    [Fact]
    public async Task SaveChangesAsync_SetsCreatedAtAndLeavesUpdatedAtNull_OnNewEntity()
    {
        await using var context = CreateInMemoryDbContext();
        var tour = new TourDo
        {
            Id = "tour-2026",
            Name = "From Zero World Tour"
        };

        var before = DateTimeOffset.UtcNow;
        context.Tours.Add(tour);
        await context.SaveChangesAsync();
        var after = DateTimeOffset.UtcNow;

        Assert.True(tour.CreatedAt >= before && tour.CreatedAt <= after);
        Assert.Null(tour.UpdatedAt);
    }

    [Fact]
    public void SaveChanges_PreservesExplicitCreatedAt_OnNewEntity()
    {
        using var context = CreateInMemoryDbContext();
        var customDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var country = new CountryDo
        {
            IsoCode = "CAN",
            Name = "Canada",
            NativeName = "Canada",
            CreatedAt = customDate
        };

        context.Countries.Add(country);
        context.SaveChanges();

        Assert.Equal(customDate, country.CreatedAt);
        Assert.Null(country.UpdatedAt);
    }

    [Fact]
    public void SaveChanges_SetsUpdatedAt_OnModifiedEntity()
    {
        using var context = CreateInMemoryDbContext();
        var country = new CountryDo
        {
            IsoCode = "GBR",
            Name = "United Kingdom",
            NativeName = "United Kingdom"
        };

        context.Countries.Add(country);
        context.SaveChanges();

        var originalCreatedAt = country.CreatedAt;
        Assert.Null(country.UpdatedAt);

        country.Name = "Great Britain";
        var beforeUpdate = DateTimeOffset.UtcNow;
        context.SaveChanges();
        var afterUpdate = DateTimeOffset.UtcNow;

        Assert.Equal(originalCreatedAt, country.CreatedAt);
        Assert.NotNull(country.UpdatedAt);
        Assert.True(country.UpdatedAt >= beforeUpdate && country.UpdatedAt <= afterUpdate);
    }

    [Fact]
    public async Task SaveChangesAsync_SetsUpdatedAt_OnModifiedEntity()
    {
        await using var context = CreateInMemoryDbContext();
        var tour = new TourDo
        {
            Id = "tour-2025",
            Name = "From Zero Tour"
        };

        context.Tours.Add(tour);
        await context.SaveChangesAsync();

        var originalCreatedAt = tour.CreatedAt;
        Assert.Null(tour.UpdatedAt);

        tour.Name = "From Zero World Tour 2025";
        var beforeUpdate = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();
        var afterUpdate = DateTimeOffset.UtcNow;

        Assert.Equal(originalCreatedAt, tour.CreatedAt);
        Assert.NotNull(tour.UpdatedAt);
        Assert.True(tour.UpdatedAt >= beforeUpdate && tour.UpdatedAt <= afterUpdate);
    }

    [Fact]
    public async Task AllEntityTypes_ReceiveTimestampsOnAdd()
    {
        await using var context = CreateInMemoryDbContext();

        var concertType = new ConcertTypeDo { Id = 1, Name = "Headliner" };
        var country = new CountryDo { IsoCode = "DEU", Name = "Germany", NativeName = "Deutschland" };
        var state = new StateDo { CountryCode = "DEU", Code = "BY", Name = "Bavaria", NativeName = "Bayern", Country = country };
        var city = new CityDo { Id = 1, CountryCode = "DEU", Name = "Munich", NativeName = "München", Country = country };
        var venue = new VenueDo { Id = 1, CountryCode = "DEU", CityId = 1, CurrentName = "Olympiahalle", TimeZone = "Europe/Berlin", Latitude = 48.1755m, Longitude = 11.5519m, Country = country, City = city };
        var previousName = new PreviousVenueNameDo { Id = 1, VenueId = 1, Name = "Old Olympiahalle", From = new DateOnly(2000, 1, 1), Venue = venue };
        var tour = new TourDo { Id = "tour-1", Name = "Tour 1" };
        var leg = new TourLegDo { TourId = "tour-1", Id = "leg-1", Name = "Europe", Tour = tour };
        var concert = new ConcertDo { Id = "c-1", ConcertTypeId = 1, VenueId = 1, PostedStartTime = DateTimeOffset.UtcNow, Status = ConcertDo.ConcertStatus.Planned, Venue = venue, Type = concertType };

        ITimestampedDataObject[] entities = [concertType, country, state, city, venue, previousName, tour, leg, concert];

        context.AddRange(entities);
        await context.SaveChangesAsync();

        foreach (var entity in entities)
        {
            Assert.NotEqual(default, entity.CreatedAt);
            Assert.Null(entity.UpdatedAt);
        }
    }
}
