using Database.Tours.DataObjects;
using Database.Tours.Repositories;

namespace Database.Tours.Tests.Repositories;

public class SqlConcertRepositoryPaginationTest : ToursDbIntegrationTestsBase
{
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(205)]
    public async Task FindAllWithReferencesAsync_ReturnsEveryNonDeletedConcertOnce(int count)
    {
        var country = new CountryDo { IsoCode = "DEU", Name = "Germany", NativeName = "Deutschland" };
        var city = new CityDo { CountryCode = country.IsoCode, Country = country, Name = "Berlin", NativeName = "Berlin" };
        var venue = new VenueDo
        {
            CountryCode = country.IsoCode, Country = country, City = city,
            CurrentName = "Arena", TimeZone = "Europe/Berlin"
        };
        DbContext.Venues.Add(venue);
        await DbContext.SaveChangesAsync();

        for (var i = 0; i < count; i++)
        {
            DbContext.Concerts.Add(new ConcertDo
            {
                Id = $"show-{i:D4}",
                VenueId = venue.Id,
                ConcertTypeId = 1,
                PostedStartTime = new DateTime(2020, 1, 1).AddDays(count - 1 - i),
                Status = ConcertDo.ConcertStatus.Past
            });
        }
        DbContext.Concerts.Add(new ConcertDo
        {
            Id = "deleted",
            VenueId = venue.Id,
            ConcertTypeId = 1,
            PostedStartTime = new DateTime(2020, 1, 1),
            DeletedAt = DateTimeOffset.UtcNow
        });
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var repository = new SqlConcertRepository(DbContext);
        var concerts = await repository.FindAllWithReferencesAsync(CancellationToken.None).ToArrayAsync();

        var expectedIds = Enumerable.Range(0, count).Select(i => $"show-{i:D4}");
        Assert.Equal(expectedIds, concerts.Select(c => c.Id).OrderBy(id => id));
        Assert.Equal(concerts.Select(c => c.PostedStartTime).Order(), concerts.Select(c => c.PostedStartTime));
        Assert.All(concerts, concert =>
        {
            Assert.Equal("Arena", concert.Venue.CurrentName);
            Assert.Equal("Berlin", concert.Venue.City.Name);
            Assert.Equal("Germany", concert.Venue.Country.Name);
            Assert.NotNull(concert.Type);
        });

        if (count > 100)
        {
            using var cancellation = new CancellationTokenSource();
            var seen = 0;
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await foreach (var concert in repository.FindAllWithReferencesAsync(cancellation.Token))
                {
                    if (++seen == 101)
                        await cancellation.CancelAsync();
                }
            });
            Assert.Equal(101, seen);
        }
    }

    [Fact]
    public async Task FindAllWithReferencesAsync_ObservesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var repository = new SqlConcertRepository(DbContext);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await repository.FindAllWithReferencesAsync(cancellation.Token).ToArrayAsync(cancellation.Token));
    }
}
