using Database.Setlists.Repositories;
using Database.Tours.DataObjects;
using Database.Tours.Repositories;
using Service.Tours.DataStructure.Stats;
using Service.Tours.Statistics;

namespace Service.Tours;

/// <summary>Calculates statistics for global, tour and user concert scopes.</summary>
public class StatisticsService(IConcertRepository concertRepository, ISongRepository songRepository, TimeProvider? timeProvider = null)
{
    /// <summary>Calculates concert and location statistics for the selected scope.</summary>
    public async Task<ConcertStatsBo> GetConcertStatsAsync(StatsScope scope, CancellationToken cancellationToken = default)
    {
        var concerts = await LoadConcertsAsync(scope, cancellationToken);
        return ConcertStatsCalculator.Calculate(concerts);
    }

    /// <summary>
    /// Calculates live song play counts for past concerts in the selected scope.
    /// Missing setlists do not contribute counts, so the result may be incomplete.
    /// </summary>
    public async Task<SongStatBo[]> GetSongStatsAsync(StatsScope scope, CancellationToken cancellationToken = default)
    {
        var concerts = await LoadConcertsAsync(scope, cancellationToken);
        return await CalculateSongStatsAsync(concerts, scope.UserId is null, cancellationToken);
    }

    /// <summary>
    /// Calculates concert and song statistics using the same selection of concerts.
    /// Song counts exclude future concerts and may be incomplete when setlists are missing.
    /// </summary>
    public async Task<(ConcertStatsBo Concerts, SongStatBo[] Songs)> GetConcertAndSongStatsAsync(
        StatsScope scope, CancellationToken cancellationToken = default)
    {
        var concerts = await LoadConcertsAsync(scope, cancellationToken);
        var concertStats = ConcertStatsCalculator.Calculate(concerts);
        var songStats = await CalculateSongStatsAsync(concerts, scope.UserId is null, cancellationToken);
        return (concertStats, songStats);
    }

    private async Task<ConcertDo[]> LoadConcertsAsync(StatsScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var query = scope.UserId is null
            ? concertRepository.GetConcertsForStatsAsync(scope.TourId, cancellationToken)
            : concertRepository.GetAttendedConcertsForAsync(scope.UserId, cancellationToken);

        if (scope.UserId is not null && scope.TourId is not null)
            query = query.Where(concert => concert.TourId == scope.TourId);

        return await query.ToArrayAsync(cancellationToken);
    }

    private async Task<SongStatBo[]> CalculateSongStatsAsync(ConcertDo[] concerts, bool excludeFuture, CancellationToken cancellationToken)
    {
        // User selections already apply the same cutoff in the attendance repository.
        var cutoff = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime;
        var ids = concerts.Where(concert => !excludeFuture || concert.PostedStartTime < cutoff)
            .Select(concert => concert.Id).Distinct().ToArray();
        if (ids.Length == 0)
            return [];

        var songs = await songRepository.GetSongsPlayedAtConcertsAsync(ids, cancellationToken);
        return SongStatsCalculator.Calculate(songs);
    }
}

