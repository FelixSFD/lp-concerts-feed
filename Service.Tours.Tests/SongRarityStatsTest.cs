using Database.Setlists.DataObjects;
using Database.Setlists.Repositories;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Service.Tours.DataStructure.Stats;

namespace Service.Tours.Tests;

public class SongRarityStatsTest
{
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
    private readonly IConcertRepository _concerts = Substitute.For<IConcertRepository>();
    private readonly ISongRepository _songs = Substitute.For<ISongRepository>();

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class TestLogger : ILogger<StatisticsService>
    {
        public List<int> Events { get; } = [];
        public bool IsEnabled(LogLevel logLevel) => true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Events.Add(eventId.Id);
    }

    private StatisticsService CreateService() => new(_concerts, _songs, NullLogger<StatisticsService>.Instance, new FixedTimeProvider());

    [Fact]
    public async Task UserOnTour_UsesObservedConcertShareAndReportsCoverage()
    {
        ConcertDo[] reference =
        [
            new() { Id = "a", TourId = "tour" }, new() { Id = "b", TourId = "tour" },
            new() { Id = "c", TourId = "tour" }, new() { Id = "d", TourId = "tour" },
            new() { Id = "future", TourId = "tour", PostedStartTime = Now.UtcDateTime }
        ];
        ConcertDo[] selection =
        [
            reference[0], reference[3], reference[4], new() { Id = "other", TourId = "other-tour" }
        ];
        _concerts.GetConcertsForStatsAsync("tour").Returns(reference.ToAsyncEnumerable());
        _concerts.GetAttendedConcertsForAsync("user").Returns(selection.ToAsyncEnumerable());

        var rare = new SongDo { Id = 1, Title = "Rare", AlbumId = 7, Album = new AlbumDo { Id = 7, Title = "Album" } };
        var common = new SongDo { Id = 2, Title = "Common" };
        var unseen = new SongDo { Id = 3, Title = "Unseen" };
        var data = new SongConcertPlayData(["a", "b", "c"],
        [
            new(rare, "a", 2), new(common, "a", 1), new(common, "b", 2), new(unseen, "b", 1)
        ]);
        _songs.GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>()).Returns(data);

        var result = await CreateService().GetSongRarityStatsAsync(
            new StatsScope(userId: "user", tourId: "tour"), new StatsScope(tourId: "tour"), 0.34m);

        Assert.Equal(Now, result.AsOf);
        Assert.Equal(4, result.ReferenceCoverage.ConcertCount);
        Assert.Equal(3, result.ReferenceCoverage.ConcertsWithSetlists);
        Assert.Equal(1, result.ReferenceCoverage.ConcertsWithoutSetlists);
        Assert.Equal(2, result.SelectionCoverage.ConcertCount);
        Assert.Equal(1, result.SelectionCoverage.ConcertsWithSetlists);
        Assert.Equal(2, result.Songs.Count);
        var rareStat = Assert.Single(result.Songs, s => s.SongId == 1);
        Assert.Equal(2u, rareStat.SelectionPlayCount);
        Assert.Equal(1, rareStat.SelectionConcertCount);
        Assert.Equal(2u, rareStat.ReferencePlayCount);
        Assert.Equal(1, rareStat.ReferenceConcertCount);
        Assert.Equal(1m / 3m, rareStat.ReferenceConcertShare);
        Assert.True(rareStat.IsRare);
        Assert.Equal(7u, rareStat.AlbumId);
        Assert.Equal("Album", rareStat.AlbumName);
        var commonStat = Assert.Single(result.Songs, s => s.SongId == 2);
        Assert.Equal(1u, commonStat.SelectionPlayCount);
        Assert.Equal(3u, commonStat.ReferencePlayCount);
        Assert.Equal(2, commonStat.ReferenceConcertCount);
        Assert.Equal(2m / 3m, commonStat.ReferenceConcertShare);
        Assert.False(commonStat.IsRare);
        await _songs.Received(1).GetSongPlaysByConcertAsync(Arg.Is<IEnumerable<string>>(ids =>
            ids.Order().SequenceEqual(new[] { "a", "b", "c", "d" })));
    }

    [Fact]
    public async Task TourAgainstGlobal_UsesGlobalReferenceAndOptionalThreshold()
    {
        var tourConcert = new ConcertDo { Id = "tour-concert", TourId = "tour" };
        var outside = new ConcertDo { Id = "outside", TourId = "other" };
        _concerts.GetConcertsForStatsAsync(null).Returns(new[] { tourConcert, outside }.ToAsyncEnumerable());
        _concerts.GetConcertsForStatsAsync("tour").Returns(new[] { tourConcert }.ToAsyncEnumerable());
        var song = new SongDo { Id = 1, Title = "Song" };
        _songs.GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>()).Returns(
            new SongConcertPlayData(["tour-concert", "outside"], [new(song, "tour-concert", 1), new(song, "outside", 1)]));

        var result = await CreateService().GetSongRarityStatsAsync(new StatsScope(tourId: "tour"), new StatsScope());

        var stat = Assert.Single(result.Songs);
        Assert.Equal(1m, stat.ReferenceConcertShare);
        Assert.Null(stat.IsRare);
        Assert.Equal(1, stat.SelectionConcertCount);
        Assert.Equal(2, stat.ReferenceConcertCount);
    }

    [Fact]
    public async Task Threshold_IsInclusive_AndObservedEntriesImplySetlistCoverage()
    {
        var concertA = new ConcertDo { Id = "a", TourId = "tour" };
        var concertB = new ConcertDo { Id = "b" };
        _concerts.GetConcertsForStatsAsync(null).Returns(new[] { concertA, concertB }.ToAsyncEnumerable());
        _concerts.GetConcertsForStatsAsync("tour").Returns(new[] { concertA }.ToAsyncEnumerable());
        var song = new SongDo { Id = 1, Title = "Song" };
        // A setlist may appear after the repository's coverage query but before its entry query.
        _songs.GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>())
            .Returns(new SongConcertPlayData(["b"], [new(song, "a", 2)]));

        var result = await CreateService().GetSongRarityStatsAsync(new StatsScope(tourId: "tour"), new StatsScope(), 0.5m);

        Assert.Equal(2, result.ReferenceCoverage.ConcertsWithSetlists);
        Assert.Equal(0.5m, Assert.Single(result.Songs).ReferenceConcertShare);
        Assert.True(Assert.Single(result.Songs).IsRare);
    }

    [Fact]
    public async Task IdenticalScopes_LoadConcertsOnce_AndZeroCoverageIsNotAZeroFrequency()
    {
        var scope = new StatsScope(tourId: "tour");
        _concerts.GetConcertsForStatsAsync("tour").Returns(new[] { new ConcertDo { Id = "a" } }.ToAsyncEnumerable());
        _songs.GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>()).Returns(new SongConcertPlayData([], []));

        var result = await CreateService().GetSongRarityStatsAsync(scope, scope, 0m);

        Assert.Empty(result.Songs);
        Assert.Equal(1, result.ReferenceCoverage.ConcertCount);
        Assert.Equal(0, result.ReferenceCoverage.ConcertsWithSetlists);
        Assert.Equal(1, result.ReferenceCoverage.ConcertsWithoutSetlists);
        _concerts.Received(1).GetConcertsForStatsAsync("tour");
    }

    [Fact]
    public async Task EmptyReference_DoesNotQuerySongData()
    {
        _concerts.GetConcertsForStatsAsync(null).Returns(Array.Empty<ConcertDo>().ToAsyncEnumerable());
        var result = await CreateService().GetSongRarityStatsAsync(new StatsScope(), new StatsScope());
        Assert.Empty(result.Songs);
        Assert.Equal(0, result.ReferenceCoverage.ConcertCount);
        await _songs.DidNotReceive().GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("user-a", null, "user-b", null)]
    [InlineData(null, "tour-a", null, "tour-b")]
    [InlineData(null, null, "user-a", null)]
    [InlineData("user-a", null, null, "tour-a")]
    public async Task InvalidScopeRelationship_IsRejected(string? selectedUser, string? selectedTour, string? referenceUser, string? referenceTour)
    {
        var selection = new StatsScope(selectedUser, selectedTour);
        var reference = new StatsScope(referenceUser, referenceTour);
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService().GetSongRarityStatsAsync(selection, reference));
        _concerts.DidNotReceive().GetConcertsForStatsAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        _concerts.DidNotReceive().GetAttendedConcertsForAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnexpectedSelectionOutsideReference_IsRejected()
    {
        _concerts.GetConcertsForStatsAsync("tour").Returns(Array.Empty<ConcertDo>().ToAsyncEnumerable());
        _concerts.GetAttendedConcertsForAsync("user")
            .Returns(new[] { new ConcertDo { Id = "outside", TourId = "tour" } }.ToAsyncEnumerable());
        await Assert.ThrowsAsync<ArgumentException>(() => CreateService().GetSongRarityStatsAsync(
            new StatsScope("user", "tour"), new StatsScope(tourId: "tour")));
        await _songs.DidNotReceive().GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public async Task InvalidThreshold_IsRejected(double threshold)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            CreateService().GetSongRarityStatsAsync(new StatsScope(), new StatsScope(), (decimal)threshold));
    }

    [Fact]
    public async Task Cancellation_IsForwardedAndPreCancelledOperationsStop()
    {
        using var source = new CancellationTokenSource();
        _concerts.GetConcertsForStatsAsync("tour", source.Token)
            .Returns(new[] { new ConcertDo { Id = "a" } }.ToAsyncEnumerable());
        _songs.GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>(), source.Token)
            .Returns(new SongConcertPlayData([], []));
        await CreateService().GetSongRarityStatsAsync(new StatsScope(tourId: "tour"), new StatsScope(tourId: "tour"), cancellationToken: source.Token);
        await _songs.Received(1).GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>(), source.Token);
        await source.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateService().GetSongRarityStatsAsync(new StatsScope(), new StatsScope(), cancellationToken: source.Token));
    }

    [Fact]
    public async Task CancellationAfterLoading_DoesNotCompleteCalculation()
    {
        using var source = new CancellationTokenSource();
        var logger = new TestLogger();
        var scope = new StatsScope(tourId: "tour");
        _concerts.GetConcertsForStatsAsync("tour", source.Token)
            .Returns(new[] { new ConcertDo { Id = "a" } }.ToAsyncEnumerable());
        _songs.GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>(), source.Token).Returns(_ =>
        {
            source.Cancel();
            return Task.FromResult(new SongConcertPlayData([], []));
        });
        var service = new StatisticsService(_concerts, _songs, logger, new FixedTimeProvider());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.GetSongRarityStatsAsync(scope, scope, cancellationToken: source.Token));
        Assert.Equal(new[] { 10049, 10042, 10051, 10052 }, logger.Events);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SongRarityLogging_ReportsOnlyCompletedPhases(bool fail)
    {
        var logger = new TestLogger();
        var scope = new StatsScope(tourId: "tour");
        _concerts.GetConcertsForStatsAsync("tour").Returns(new[] { new ConcertDo { Id = "a" } }.ToAsyncEnumerable());
        _songs.GetSongPlaysByConcertAsync(Arg.Any<IEnumerable<string>>()).Returns(fail
            ? Task.FromException<SongConcertPlayData>(new InvalidOperationException("Failed to load"))
            : Task.FromResult(new SongConcertPlayData([], [])));
        var service = new StatisticsService(_concerts, _songs, logger, new FixedTimeProvider());

        if (fail)
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetSongRarityStatsAsync(scope, scope));
        else
            await service.GetSongRarityStatsAsync(scope, scope);

        Assert.Equal(fail ? new[] { 10049, 10042, 10051 } : new[] { 10049, 10042, 10051, 10052, 10050 }, logger.Events);
    }
}



