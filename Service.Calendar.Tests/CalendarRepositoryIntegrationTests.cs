using Common.Database.Tests;
using Database.Tours;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.EntityFrameworkCore;
using Service.Calendar.DataStructure;

namespace Service.Calendar.Tests;

/// <summary>Verifies calendar generation against the tours SQL repository.</summary>
public class CalendarRepositoryIntegrationTests : DbIntegrationTestsBase<ToursDbContext>
{
    /// <inheritdoc />
    protected override ToursDbContext CreateDbContext(DbContextOptions<ToursDbContext> options) => new(options);

    [Fact]
    public async Task GeneratesAllNonDeletedConcertsWithoutDefaultPagination()
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

        for (var i = 0; i < 105; i++)
        {
            DbContext.Concerts.Add(new ConcertDo
            {
                Id = $"show-{i}",
                VenueId = venue.Id,
                ConcertTypeId = 1,
                PostedStartTime = new DateTime(2020, 1, 1, 18, 0, 0).AddDays(i),
                Status = i == 0 ? ConcertDo.ConcertStatus.Cancelled : ConcertDo.ConcertStatus.Past,
                DeletedAt = i == 104 ? DateTimeOffset.UtcNow : null
            });
        }
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var service = new CalendarGenerationService(new SqlConcertRepository(DbContext));
        var calendar = await service.GenerateCalendarAsync(new CalendarGenerationSettingsBo());

        Assert.Equal(104, calendar.Events.Count);
        Assert.DoesNotContain(calendar.Events, e => e.Uid is not null && e.Uid.StartsWith("show-104-", StringComparison.Ordinal));
        Assert.Equal(Ical.Net.EventStatus.Cancelled,
            calendar.Events.Single(e => e.Uid is not null && e.Uid.StartsWith("show-0-", StringComparison.Ordinal)).Status);
        Assert.All(calendar.Events, e => Assert.Equal("Arena, Berlin, Germany", e.Location));
    }
}
