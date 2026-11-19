using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Ical.Net;
using Ical.Net.Serialization;
using NSubstitute;
using Service.Calendar.DataStructure;
using System.Runtime.CompilerServices;

namespace Service.Calendar.Tests;

/// <summary>Tests the calendar generation service and its event mapping.</summary>
public class CalendarGenerationServiceTests
{
    [Theory]
    [InlineData(false, false, "concert")]
    [InlineData(true, false, "doors")]
    [InlineData(false, true, "stage")]
    [InlineData(true, true, "doors,stage")]
    public async Task SettingsSelectDetailedEventsOrFullFallback(bool doors, bool stage, string expected)
    {
        var concert = MakeConcert();
        var service = CreateService(concert);
        var calendar = await service.GenerateCalendarAsync(new CalendarGenerationSettingsBo
        {
            IncludeDoorsTime = doors,
            IncludeLpStageTime = stage
        });

        var ids = calendar.Events.Select(e => Assert.IsType<string>(e.Uid).Split('@')[0].Split('-').Last());
        Assert.Equal(expected.Split(','), ids);
        Assert.All(calendar.Events, e => Assert.Equal(EventStatus.Confirmed, e.Status));
        if (doors)
        {
            var doorsEvent = calendar.Events.First();
            Assert.Equal(new DateTime(2026, 6, 11, 19, 30, 0), doorsEvent.Start!.Value);
            Assert.Equal("Europe/Berlin", doorsEvent.Start.TzId);
            Assert.Equal(stage ? new DateTime(2026, 6, 11, 20, 55, 0) : (DateTime?)null, doorsEvent.End?.Value);
        }
        if (stage)
        {
            var stageEvent = calendar.Events.Last();
            Assert.Equal(new DateTime(2026, 6, 11, 20, 55, 0), stageEvent.Start!.Value);
            Assert.Equal(new DateTime(2026, 6, 11, 22, 40, 0), stageEvent.End!.Value);
        }
    }

    [Theory]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, true, false, false)]
    public async Task MissingSelectedTimesProduceFullFallback(bool doors, bool stage, bool hasDoors, bool hasStage)
    {
        var concert = MakeConcert();
        if (!hasDoors) concert.DoorsTime = null;
        if (!hasStage) concert.MainStageTime = null;
        var calendar = await CreateService(concert).GenerateCalendarAsync(new CalendarGenerationSettingsBo
        {
            IncludeDoorsTime = doors,
            IncludeLpStageTime = stage
        });

        var result = Assert.Single(calendar.Events);
        Assert.EndsWith("-concert@lpshows.live", result.Uid);
        Assert.Equal(hasStage ? EventStatus.Confirmed : EventStatus.Tentative, result.Status);
        Assert.Equal(hasStage ? new DateTime(2026, 6, 11, 20, 55, 0) : new DateTime(2026, 6, 11, 20, 0, 0), result.Start!.Value);
        Assert.Equal(hasStage ? new DateTime(2026, 6, 11, 22, 55, 0) : new DateTime(2026, 6, 11, 23, 0, 0), result.End!.Value);
    }

    [Fact]
    public async Task DetailsStatusLocationAndLinksAreMapped()
    {
        var concert = MakeConcert();
        concert.Status = ConcertDo.ConcertStatus.Cancelled;
        concert.UpdatedAt = new DateTimeOffset(2026, 1, 3, 10, 0, 0, TimeSpan.FromHours(2));
        var calendar = await CreateService([concert], new Uri("https://example.test/")).GenerateCalendarAsync(
            new CalendarGenerationSettingsBo { IncludeDoorsTime = true, IncludeLpStageTime = true }, subscriptionId: "sub-1");
        var doors = Assert.IsType<Ical.Net.CalendarComponents.CalendarEvent>(calendar.Events[0]);
        var stage = Assert.IsType<Ical.Net.CalendarComponents.CalendarEvent>(calendar.Events[1]);

        Assert.Equal("Doors open: Linkin Park in Munich", doors.Summary);
        Assert.Equal("From Zero: Munich (Stage Time)", stage.Summary);
        Assert.Equal("Arena, Munich, Bavaria, Germany", stage.Location);
        Assert.Contains("Type of show: Linkin Park Show", stage.Description);
        Assert.Contains("https://example.test/calendar-subscriptions/sub-1?mtm_kwd=show-1&mtm_campaign=ical-feed", stage.Description);
        Assert.Equal("https://example.test/concert/show-1", stage.Url!.AbsoluteUri);
        Assert.Equal(48.1, stage.GeographicLocation!.Latitude);
        Assert.Equal(11.5, stage.GeographicLocation.Longitude);
        Assert.All(calendar.Events, e => Assert.Equal(EventStatus.Cancelled, e.Status));
        Assert.Equal(new DateTime(2026, 1, 3, 8, 0, 0), stage.LastModified!.Value);
        Assert.True(stage.LastModified.IsUtc);
        Assert.Contains("LAST-MODIFIED:20260103T080000Z", new CalendarSerializer().SerializeToString(calendar));
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, false, true)]
    [InlineData(false, false, false)]
    public async Task ManagementLinksUseSubscriptionIdForEveryEvent(bool doors, bool stage, bool hasTour)
    {
        var concert = MakeConcert("show /1");
        if (!hasTour) concert.Tour = null;
        var calendar = await CreateService([concert], new Uri("https://example.test/base/")).GenerateCalendarAsync(
            new CalendarGenerationSettingsBo { IncludeDoorsTime = doors, IncludeLpStageTime = stage },
            subscriptionId: "sub /1");

        Assert.Equal(doors && stage ? 2 : 1, calendar.Events.Count);
        Assert.All(calendar.Events, e => Assert.Contains(
            "https://example.test/base/calendar-subscriptions/sub%20%2F1?mtm_kwd=show%20%2F1&mtm_campaign=ical-feed",
            e.Description));
    }

    [Fact]
    public async Task ManagementLinkRequiresBothSiteUriAndSubscriptionId()
    {
        var concert = MakeConcert();
        var withoutId = await CreateService([concert], new Uri("https://example.test/")).GenerateCalendarAsync(new CalendarGenerationSettingsBo());
        var withoutSite = await CreateService(concert).GenerateCalendarAsync(new CalendarGenerationSettingsBo(), subscriptionId: "sub-1");

        Assert.DoesNotContain("calendar-subscriptions/", Assert.Single(withoutId.Events).Description);
        Assert.DoesNotContain("calendar-subscriptions/", Assert.Single(withoutSite.Events).Description);
    }

    [Fact]
    public async Task FullEventWithoutTourUsesMediumLocationAndCustomTitle()
    {
        var concert = MakeConcert();
        concert.Tour = null;
        concert.CustomTitle = "Special";
        concert.Venue.Latitude = 0;
        concert.Venue.Longitude = 0;
        var calendar = await CreateService(concert).GenerateCalendarAsync(new CalendarGenerationSettingsBo());
        var result = Assert.Single(calendar.Events);

        Assert.Equal("Special", result.Summary);
        Assert.Equal("Arena, Munich, Germany", result.Location);
        Assert.Equal("Linkin Park Concert at Arena\nThis show is not part of a tour.", result.Description);
        Assert.Null(result.GeographicLocation);
        Assert.Null(result.Url);
        Assert.Null(result.LastModified);
    }

    [Fact]
    public async Task DifferentZonesAndDstAreRepresentedCorrectly()
    {
        var berlin = MakeConcert();
        berlin.PostedStartTime = new DateTime(2025, 3, 30, 0, 30, 0);
        berlin.MainStageTime = null;
        var newYork = MakeConcert("ny");
        newYork.Venue.TimeZone = "America/New_York";
        newYork.PostedStartTime = new DateTime(2025, 3, 30, 0, 30, 0);
        newYork.MainStageTime = null;
        var calendar = await CreateService(berlin, newYork).GenerateCalendarAsync(new CalendarGenerationSettingsBo());
        var berlinEvent = Assert.IsType<Ical.Net.CalendarComponents.CalendarEvent>(calendar.Events[0]);
        var nyEvent = Assert.IsType<Ical.Net.CalendarComponents.CalendarEvent>(calendar.Events[1]);
        Assert.Equal(new DateTime(2025, 3, 30, 1, 30, 0), berlinEvent.Start!.Value);
        Assert.Equal(new DateTime(2025, 3, 30, 5, 30, 0), berlinEvent.End!.Value);
        Assert.Equal(new DateTime(2025, 3, 29, 20, 30, 0), nyEvent.Start!.Value);
        Assert.Equal(new DateTime(2025, 3, 30, 0, 30, 0, DateTimeKind.Utc), berlinEvent.Start.AsUtc);
        Assert.Equal(new DateTime(2025, 3, 30, 0, 30, 0, DateTimeKind.Utc), nyEvent.Start.AsUtc);
        var ical = Assert.IsType<string>(new CalendarSerializer().SerializeToString(calendar));
        Assert.Contains("TZID:Europe/Berlin", ical);
        Assert.Contains("TZID:America/New_York", ical);
        Assert.Contains("BEGIN:DAYLIGHT", ical);
        Assert.Contains("BEGIN:STANDARD", ical);
        var reloaded = Assert.IsType<Ical.Net.Calendar>(Ical.Net.Calendar.Load(ical));
        Assert.Equal(2, reloaded.TimeZones.Count);
        Assert.Equal(berlinEvent.Start.AsUtc, reloaded.Events.Single(e => e.Uid == berlinEvent.Uid).Start!.AsUtc);
        Assert.Equal(nyEvent.Start.AsUtc, reloaded.Events.Single(e => e.Uid == nyEvent.Uid).Start!.AsUtc);
    }

    [Fact]
    public async Task AmbiguousDstTimeRetainsTheOriginalInstant()
    {
        var concert = MakeConcert();
        concert.MainStageTime = new DateTime(2025, 10, 26, 1, 30, 0);
        var calendar = await CreateService(concert).GenerateCalendarAsync(new CalendarGenerationSettingsBo { IncludeLpStageTime = true });
        var result = Assert.Single(calendar.Events);
        Assert.True(result.Start!.IsUtc);
        Assert.Equal(new DateTime(2025, 10, 26, 1, 30, 0, DateTimeKind.Utc), result.Start.AsUtc);
        Assert.Contains("DTSTART:20251026T013000Z", new CalendarSerializer().SerializeToString(calendar));
    }

    [Fact]
    public async Task StageTitleAndDoorsStatusFollowLegacyRules()
    {
        var concert = MakeConcert();
        concert.Type.Name = "Festival";
        var stageCalendar = await CreateService(concert).GenerateCalendarAsync(new CalendarGenerationSettingsBo { IncludeLpStageTime = true });
        Assert.Equal("Linkin Park: Munich (Stage Time)", Assert.Single(stageCalendar.Events).Summary);

        concert.CustomTitle = "Festival special";
        stageCalendar = await CreateService(concert).GenerateCalendarAsync(new CalendarGenerationSettingsBo { IncludeLpStageTime = true });
        Assert.Equal("Festival special", Assert.Single(stageCalendar.Events).Summary);

        concert.MainStageTime = null;
        var doorsCalendar = await CreateService(concert).GenerateCalendarAsync(new CalendarGenerationSettingsBo { IncludeDoorsTime = true });
        var doorsEvent = Assert.Single(doorsCalendar.Events);
        Assert.Equal(EventStatus.Tentative, doorsEvent.Status);
        Assert.Null(doorsEvent.End);
    }

    [Fact]
    public async Task PlaceholderTimesRemainTimedAndTimezonesAreNotDuplicated()
    {
        var first = MakeConcert();
        first.TimeIsPlaceholder = true;
        first.MainStageTime = null;
        var second = MakeConcert("second");
        var calendar = await CreateService(first, second).GenerateCalendarAsync(new CalendarGenerationSettingsBo());
        Assert.True(calendar.Events.First().Start!.HasTime);
        Assert.Single(calendar.TimeZones);
    }

    [Fact]
    public async Task IdsAreStableDistinctAndIndependentOfSettings()
    {
        var concert = MakeConcert();
        var service = CreateService(concert);
        var both = await service.GenerateCalendarAsync(new CalendarGenerationSettingsBo { IncludeDoorsTime = true, IncludeLpStageTime = true });
        var stageOnly = await service.GenerateCalendarAsync(new CalendarGenerationSettingsBo { IncludeLpStageTime = true });
        var full = await service.GenerateCalendarAsync(new CalendarGenerationSettingsBo());
        Assert.Equal(2, both.Events.Select(e => e.Uid).Distinct().Count());
        var stage = Assert.IsType<Ical.Net.CalendarComponents.CalendarEvent>(both.Events[1]);
        var fullEvent = Assert.Single(full.Events);
        Assert.Equal(stage.Uid, Assert.Single(stageOnly.Events).Uid);
        Assert.DoesNotContain(fullEvent.Uid, both.Events.Select(e => e.Uid));
    }

    [Fact]
    public async Task CancellationIsPropagatedDuringEnumeration()
    {
        using var cts = new CancellationTokenSource();
        var repository = Substitute.For<IConcertRepository>();
        repository.FindAllWithReferencesAsync(Arg.Any<CancellationToken>()).Returns(_ => CancelAfterFirst(cts));
        var service = new CalendarGenerationService(repository);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GenerateCalendarAsync(new CalendarGenerationSettingsBo(), cts.Token));
        repository.Received(1).FindAllWithReferencesAsync(cts.Token);
    }

    private static async IAsyncEnumerable<ConcertDo> CancelAfterFirst(
        CancellationTokenSource cts, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return MakeConcert();
        cts.Cancel();
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        yield return MakeConcert("second");
    }

    [Fact]
    public async Task EmptyInvalidAndCancelledInputsAreHandled()
    {
        var service = CreateService();
        Assert.Empty((await service.GenerateCalendarAsync(new CalendarGenerationSettingsBo())).Events);
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GenerateCalendarAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateService([], new Uri("ftp://example.test")).GenerateCalendarAsync(new CalendarGenerationSettingsBo()));
        var invalid = MakeConcert();
        invalid.Venue.TimeZone = "not/a-timezone";
        await Assert.ThrowsAsync<TimeZoneNotFoundException>(() => CreateService(invalid).GenerateCalendarAsync(new CalendarGenerationSettingsBo()));
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CreateService(MakeConcert()).GenerateCalendarAsync(new CalendarGenerationSettingsBo(), cts.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.GenerateCalendarAsync(new CalendarGenerationSettingsBo(), cts.Token));
    }

    private static CalendarGenerationService CreateService(params ConcertDo[] concerts) => CreateService(concerts, null);

    private static CalendarGenerationService CreateService(ConcertDo[] concerts, Uri? baseUri)
    {
        var repository = Substitute.For<IConcertRepository>();
        repository.FindAllWithReferencesAsync(Arg.Any<CancellationToken>()).Returns(_ => concerts.ToAsyncEnumerable());
        return new CalendarGenerationService(repository, baseUri);
    }

    internal static ConcertDo MakeConcert(string id = "show-1")
    {
        var country = new CountryDo { IsoCode = "DEU", Name = "Germany", NativeName = "Deutschland" };
        var state = new StateDo { CountryCode = "DEU", Code = "BY", Name = "Bavaria", NativeName = "Bayern", Country = country };
        var city = new CityDo { CountryCode = "DEU", Name = "Munich", NativeName = "München", Country = country, State = state };
        var venue = new VenueDo
        {
            CountryCode = "DEU", CurrentName = "Arena", TimeZone = "Europe/Berlin",
            City = city, Country = country, State = state, Latitude = 48.1m, Longitude = 11.5m
        };
        return new ConcertDo
        {
            Id = id, Venue = venue, Type = new ConcertTypeDo { Name = "Linkin Park Show" },
            Tour = new TourDo { Id = "tour", Name = "From Zero" },
            PostedStartTime = new DateTime(2026, 6, 11, 18, 0, 0),
            DoorsTime = new DateTime(2026, 6, 11, 17, 30, 0),
            MainStageTime = new DateTime(2026, 6, 11, 18, 55, 0),
            ExpectedSetDurationMinutes = 105,
            Status = ConcertDo.ConcertStatus.Planned
        };
    }
}
