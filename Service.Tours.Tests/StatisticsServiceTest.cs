using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using NSubstitute;
using Service.Tours.DataStructure.Stats;

namespace Service.Tours.Tests;

public class StatisticsServiceTest
{
    private readonly IConcertRepository _concerts = Substitute.For<IConcertRepository>();
    private readonly ISongRepository _songs = Substitute.For<ISongRepository>();

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("tour-a")]
    public async Task GlobalSongStats_UseGlobalConcertSelection(string? tourId)
    {
        var concert = new ConcertDo { Id = "show-a", TourId = tourId };
        _concerts.GetConcertsForStatsAsync(tourId).Returns(new[] { concert }.ToAsyncEnumerable());
        var song = new SongDo { Id = 42, Title = "Song", AlbumId = 7, Album = new AlbumDo { Id = 7, Title = "Album" } };
        _songs.GetSongsPlayedAtConcertsAsync(Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "show-a" })))
            .Returns(new Dictionary<SongDo, uint> { [song] = 2 });

        var result = await new StatisticsService(_concerts, _songs).GetSongStatsAsync(new StatsScope(tourId: tourId));

        var stat = Assert.Single(result);
        Assert.Equal(42u, stat.SongId);
        Assert.Equal(7u, stat.AlbumId);
        Assert.Equal("Album", stat.AlbumName);
        Assert.Equal(2u, stat.PlayCount);
        _concerts.DidNotReceive().GetAttendedConcertsForAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GlobalCombinedStats_ExcludeFutureSetlistsButKeepFutureConcerts()
    {
        var now = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var past = UserStatsServiceTest.CreateMockConcert();
        past.Id = "past";
        past.PostedStartTime = now.AddTicks(-1).UtcDateTime;
        var atCutoff = UserStatsServiceTest.CreateMockConcert();
        atCutoff.Id = "at-cutoff";
        atCutoff.PostedStartTime = now.UtcDateTime;
        var future = UserStatsServiceTest.CreateMockConcert();
        future.Id = "future";
        future.PostedStartTime = now.AddDays(1).UtcDateTime;
        _concerts.GetConcertsForStatsAsync("tour")
            .Returns(new[] { past, atCutoff, future }.ToAsyncEnumerable());
        _songs.GetSongsPlayedAtConcertsAsync(Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "past" })))
            .Returns(new Dictionary<SongDo, uint> { [new SongDo { Id = 1, Title = "Song" }] = 2 });

        var result = await new StatisticsService(_concerts, _songs, new FixedTimeProvider(now))
            .GetConcertAndSongStatsAsync(new StatsScope(tourId: "tour"));

        Assert.Equal(3, result.Concerts.NumberOfConcerts);
        Assert.Equal(2u, Assert.Single(result.Songs).PlayCount);
        await _songs.Received(1).GetSongsPlayedAtConcertsAsync(
            Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "past" })));
    }

    [Fact]
    public async Task UserTourSongStats_IntersectAttendedConcertsWithTour()
    {
        ConcertDo[] concerts =
        [
            new() { Id = "a", TourId = "tour-a" },
            new() { Id = "a", TourId = "tour-a" },
            new() { Id = "b", TourId = "tour-b" },
            new() { Id = "no-tour" }
        ];
        using var cancellation = new CancellationTokenSource();
        _concerts.GetAttendedConcertsForAsync("user", cancellation.Token).Returns(concerts.ToAsyncEnumerable());
        var song = new SongDo { Id = 1, Title = "Song" };
        _songs.GetSongsPlayedAtConcertsAsync(Arg.Is<IEnumerable<string>>(ids => ids.SequenceEqual(new[] { "a" })), cancellation.Token)
            .Returns(new Dictionary<SongDo, uint> { [song] = 3 });

        var result = await new StatisticsService(_concerts, _songs)
            .GetSongStatsAsync(new StatsScope(userId: "user", tourId: "tour-a"), cancellation.Token);

        Assert.Equal(3u, Assert.Single(result).PlayCount);
        _concerts.DidNotReceive().GetConcertsForStatsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _songs.Received(1).GetSongsPlayedAtConcertsAsync(Arg.Any<IEnumerable<string>>(), cancellation.Token);
    }

    [Fact]
    public async Task EmptySelection_DoesNotQuerySongs()
    {
        _concerts.GetAttendedConcertsForAsync("user").Returns(Array.Empty<ConcertDo>().ToAsyncEnumerable());
        var result = await new StatisticsService(_concerts, _songs).GetSongStatsAsync(new StatsScope(userId: "user"));
        Assert.Empty(result);
        await _songs.DidNotReceive().GetSongsPlayedAtConcertsAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CombinedStats_LoadConcertsOnlyOnce()
    {
        var concert = UserStatsServiceTest.CreateMockConcert();
        _concerts.GetAttendedConcertsForAsync("user").Returns(new[] { concert }.ToAsyncEnumerable());
        _songs.GetSongsPlayedAtConcertsAsync(Arg.Any<IEnumerable<string>>())
            .Returns(new Dictionary<SongDo, uint> { [new SongDo { Id = 1, Title = "Song" }] = 2 });

        var result = await new StatisticsService(_concerts, _songs).GetConcertAndSongStatsAsync(new StatsScope(userId: "user"));

        Assert.Equal(1, result.Concerts.NumberOfConcerts);
        Assert.Equal(2u, Assert.Single(result.Songs).PlayCount);
        _concerts.Received(1).GetAttendedConcertsForAsync("user");
    }

    [Fact]
    public async Task ConcertStats_CountDistinctLocationIdsAcrossSeparateInstances()
    {
        var first = UserStatsServiceTest.CreateMockConcert();
        var sameVenue = UserStatsServiceTest.CreateMockConcert();
        var otherVenue = UserStatsServiceTest.CreateMockConcert();
        var otherCity = UserStatsServiceTest.CreateMockConcert();
        first.VenueId = sameVenue.VenueId = 1;
        otherVenue.VenueId = 2;
        otherCity.VenueId = 3;
        first.Venue.CityId = sameVenue.Venue.CityId = otherVenue.Venue.CityId = 10;
        otherCity.Venue.CityId = 20;
        otherCity.Venue.City.CountryCode = "FRA";
        _concerts.GetConcertsForStatsAsync(null)
            .Returns(new[] { first, sameVenue, otherVenue, otherCity }.ToAsyncEnumerable());

        var result = await new StatisticsService(_concerts, _songs).GetConcertStatsAsync(new StatsScope());

        Assert.Equal(4, result.NumberOfConcerts);
        Assert.Equal(3, result.NumberOfVenues);
        Assert.Equal(2, result.NumberOfCities);
        Assert.Equal(2, result.NumberOfCountries);
    }

    [Fact]
    public void Scope_RejectsEmptyIdentifiers()
    {
        Assert.Throws<ArgumentException>(() => new StatsScope(userId: " "));
        Assert.Throws<ArgumentException>(() => new StatsScope(tourId: ""));
    }
}



