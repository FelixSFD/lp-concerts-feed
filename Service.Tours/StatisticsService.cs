using Database.Setlists.Repositories;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Microsoft.Extensions.Logging;
using Service.Tours.DataStructure.Stats;
using Service.Tours.Statistics;

namespace Service.Tours;

/// <summary>Calculates statistics for global, tour and user concert scopes.</summary>
public class StatisticsService(IConcertRepository concertRepository, ISongRepository songRepository, ILogger<StatisticsService> logger, TimeProvider? timeProvider = null)
{
    /// <summary>Calculates concert and location statistics for the selected scope.</summary>
    public async Task<ConcertStatsBo> GetConcertStatsAsync(StatsScope scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        Log.StartGeneratingConcertStats(logger, scope.UserId, scope.TourId);
        var concerts = await LoadConcertsAsync(scope, cancellationToken);
        return CalculateConcertStats(concerts, scope);
    }

    /// <summary>
    /// Calculates live song play counts for past concerts in the selected scope.
    /// Missing setlists do not contribute counts, so the result may be incomplete.
    /// </summary>
    public async Task<SongStatBo[]> GetSongStatsAsync(StatsScope scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        Log.StartGeneratingSongStats(logger, scope.UserId, scope.TourId);
        var concerts = await LoadConcertsAsync(scope, cancellationToken);
        return await CalculateSongStatsAsync(concerts, scope, cancellationToken);
    }

    /// <summary>
    /// Calculates concert and song statistics using the same selection of concerts.
    /// Song counts exclude future concerts and may be incomplete when setlists are missing.
    /// </summary>
    public async Task<(ConcertStatsBo Concerts, SongStatBo[] Songs)> GetConcertAndSongStatsAsync(
        StatsScope scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        Log.StartGeneratingConcertStats(logger, scope.UserId, scope.TourId);
        Log.StartGeneratingSongStats(logger, scope.UserId, scope.TourId);
        var concerts = await LoadConcertsAsync(scope, cancellationToken);
        var concertStats = CalculateConcertStats(concerts, scope);
        var songStats = await CalculateSongStatsAsync(concerts, scope, cancellationToken);
        return (concertStats, songStats);
    }

    private async Task<ConcertDo[]> LoadConcertsAsync(StatsScope scope, CancellationToken cancellationToken)
    {
        var query = scope.UserId is null
            ? concertRepository.GetConcertsForStatsAsync(scope.TourId, cancellationToken)
            : concertRepository.GetAttendedConcertsForAsync(scope.UserId, cancellationToken);

        if (scope.UserId is not null && scope.TourId is not null)
            query = query.Where(concert => concert.TourId == scope.TourId);

        var concerts = await query.ToArrayAsync(cancellationToken);
        Log.LoadedConcertsForStats(logger, concerts.Length, scope.UserId, scope.TourId);
        return concerts;
    }

    private ConcertStatsBo CalculateConcertStats(ConcertDo[] concerts, StatsScope scope)
    {
        var stats = ConcertStatsCalculator.Calculate(concerts);
        Log.FinishedGeneratingConcertStats(logger, scope.UserId, scope.TourId, stats.NumberOfConcerts,
            stats.NumberOfVenues, stats.NumberOfCities, stats.NumberOfCountries);
        return stats;
    }

    private async Task<SongStatBo[]> CalculateSongStatsAsync(ConcertDo[] concerts, StatsScope scope, CancellationToken cancellationToken)
    {
        // User selections already apply the same cutoff in the attendance repository.
        var cutoff = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime;
        var ids = concerts.Where(concert => scope.UserId is not null || concert.PostedStartTime < cutoff)
            .Select(concert => concert.Id).Distinct().ToArray();
        SongStatBo[] stats = [];
        if (ids.Length > 0)
        {
            Log.LoadingLiveSongCounts(logger, ids.Length, scope.UserId, scope.TourId);
            var songs = await songRepository.GetSongsPlayedAtConcertsAsync(ids, cancellationToken);
            Log.LoadedLiveSongCounts(logger, songs.Count, scope.UserId, scope.TourId);
            stats = SongStatsCalculator.Calculate(songs);
        }

        Log.FinishedGeneratingSongStats(logger, scope.UserId, scope.TourId, stats.Length);
        return stats;
    }
}

