using System.Collections.Concurrent;
using System.Diagnostics;
using Common.Utils.Diagnostics;
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
    private static readonly ActivitySource ActivitySource = ActivitySourceProvider.Default;
    private const decimal DefaultRarityThreshold = 0.25m;
    
    private readonly ConcurrentDictionary<string, ConcertDo[]> _concertStatsCache = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>Calculates concert and location statistics for the selected scope.</summary>
    public async Task<ConcertStatsBo> GetConcertStatsAsync(StatsScope scope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("scope.userId", scope.UserId);
        activity?.SetTag("scope.tourId", scope.TourId);
        
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
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("scope.userId", scope.UserId);
        activity?.SetTag("scope.tourId", scope.TourId);
        
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
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("scope.userId", scope.UserId);
        activity?.SetTag("scope.tourId", scope.TourId);
        
        Log.StartGeneratingConcertStats(logger, scope.UserId, scope.TourId);
        Log.StartGeneratingSongStats(logger, scope.UserId, scope.TourId);
        var concerts = await LoadConcertsAsync(scope, cancellationToken);
        var concertStats = CalculateConcertStats(concerts, scope);
        var songStats = await CalculateSongStatsAsync(concerts, scope, cancellationToken);
        return (concertStats, songStats);
    }

    /// <summary>
    /// Compares the songs observed in a selection with their frequency in a broader reference scope.
    /// Both scopes use the same exclusive start-time cutoff. Only concerts with setlists are used
    /// as the denominator; partial or missing setlists can make the observed counts incomplete.
    /// Recording-only entries and mashups are excluded, and variants count as their base song.
    /// </summary>
    /// <param name="selection">Concerts whose observed songs are returned.</param>
    /// <param name="reference">A scope containing every concert in the selection.</param>
    /// <param name="rarityThreshold">Optional inclusive rarity threshold between 0 and 1.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Song counts, observed reference shares and setlist coverage.</returns>
    /// <exception cref="ArgumentException">The selection is not provably contained in the reference.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The threshold is outside the range 0 to 1.</exception>
    public async Task<SongRarityStatsBo> GetSongRarityStatsAsync(
        StatsScope selection, StatsScope reference, decimal? rarityThreshold = DefaultRarityThreshold,
        CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("selection.userId", selection.UserId);
        activity?.SetTag("selection.tourId", selection.TourId);
        activity?.SetTag("reference.userId", reference.UserId);
        activity?.SetTag("reference.tourId", reference.TourId);
        
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(reference);
        if (rarityThreshold is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(rarityThreshold));
        if ((reference.UserId is not null && reference.UserId != selection.UserId) ||
            (reference.TourId is not null && reference.TourId != selection.TourId))
            throw new ArgumentException("The selection must be contained in the reference scope.", nameof(selection));

        rarityThreshold ??= DefaultRarityThreshold;
        
        cancellationToken.ThrowIfCancellationRequested();
        var asOf = (timeProvider ?? TimeProvider.System).GetUtcNow();
        Log.StartGeneratingSongRarityStats(logger, selection.UserId, selection.TourId, reference.UserId, reference.TourId);
        var referenceConcerts = await LoadConcertsAsync(reference, cancellationToken);
        var selectionConcerts = selection == reference
            ? referenceConcerts
            : await LoadConcertsAsync(selection, cancellationToken);

        var referenceIds = referenceConcerts.Where(concert => concert.PostedStartTime < asOf.UtcDateTime)
            .Select(concert => concert.Id).ToHashSet();
        var selectionIds = selectionConcerts.Where(concert => concert.PostedStartTime < asOf.UtcDateTime)
            .Select(concert => concert.Id).ToHashSet();
        if (!selectionIds.IsSubsetOf(referenceIds))
            throw new ArgumentException("The selected concerts are not contained in the reference results.", nameof(selection));

        SongConcertPlayData data = new([], []);
        if (referenceIds.Count > 0)
        {
            Log.LoadingSongRarityData(logger, referenceIds.Count, reference.UserId, reference.TourId);
            data = await songRepository.GetSongPlaysByConcertAsync(referenceIds, cancellationToken);
            Log.LoadedSongRarityData(logger, reference.UserId, reference.TourId, data.ConcertIdsWithSetlists.Count, data.SongPlays.Count);
        }
        cancellationToken.ThrowIfCancellationRequested();
        var stats = SongRarityStatsCalculator.Calculate(selection, reference, asOf, selectionIds, referenceIds, data, rarityThreshold);
        Log.FinishedGeneratingSongRarityStats(logger, selection.UserId, selection.TourId, reference.UserId, reference.TourId,
            stats.Songs.Count, stats.ReferenceCoverage.ConcertsWithSetlists, stats.ReferenceCoverage.ConcertCount);
        return stats;
    }

    /// <summary>
    /// Loads the concerts for the given scope, using a cache to avoid repeated database queries.
    /// </summary>
    /// <param name="scope">The scope for which to load concerts.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An array of concerts for the given scope.</returns>
    private async Task<ConcertDo[]> LoadConcertsAsync(StatsScope scope, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("scope.userId", scope.UserId);
        activity?.SetTag("scope.tourId", scope.TourId);
        
        // check if the concerts are already cached for this scope
        if (_concertStatsCache.TryGetValue(scope.ScopeIdentifier, out var cachedConcerts))
        {
            activity?.SetTag("cache.hit", true);
            return cachedConcerts;
        }

        activity?.SetTag("cache.hit", false);

        var query = scope.UserId is null
            ? concertRepository.GetConcertsForStatsAsync(scope.TourId, cancellationToken)
            : concertRepository.GetAttendedConcertsForAsync(scope.UserId, cancellationToken);

        if (scope.UserId is not null && scope.TourId is not null)
            query = query.Where(concert => concert.TourId == scope.TourId);

        var concerts = await query.ToArrayAsync(cancellationToken);
        Log.LoadedConcertsForStats(logger, concerts.Length, scope.UserId, scope.TourId);
        _concertStatsCache[scope.ScopeIdentifier] = concerts;
        return concerts;
    }
    
    private ConcertStatsBo CalculateConcertStats(ConcertDo[] concerts, StatsScope scope)
    {
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("scope.userId", scope.UserId);
        activity?.SetTag("scope.tourId", scope.TourId);
        activity?.SetTag("numberOfConcerts", concerts.Length);
        
        var stats = ConcertStatsCalculator.Calculate(concerts);
        Log.FinishedGeneratingConcertStats(logger, scope.UserId, scope.TourId, stats.NumberOfConcerts,
            stats.NumberOfVenues, stats.NumberOfCities, stats.NumberOfCountries);
        return stats;
    }

    private async Task<SongStatBo[]> CalculateSongStatsAsync(ConcertDo[] concerts, StatsScope scope, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("scope.userId", scope.UserId);
        activity?.SetTag("scope.tourId", scope.TourId);
        activity?.SetTag("numberOfConcerts", concerts.Length);
        
        // User selections already apply the same cutoff in the attendance repository.
        var cutoff = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime;
        var ids = GetRelevantConcertIds(concerts, scope, cutoff);
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

    private static string[] GetRelevantConcertIds(ConcertDo[] concerts, StatsScope scope, DateTime cutoff)
    {
        using var activity = ActivitySource.StartActivity();
        activity?.SetTag("scope.userId", scope.UserId);
        activity?.SetTag("scope.tourId", scope.TourId);
        activity?.SetTag("numberOfConcerts", concerts.Length);
        
        var ids = concerts.Where(concert => scope.UserId is not null || concert.PostedStartTime < cutoff)
            .Select(concert => concert.Id).Distinct().ToArray();
        return ids;
    }
}

