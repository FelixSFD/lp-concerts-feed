namespace Service.Tours.DataStructure.Stats;

/// <summary>Observed song rarity in a selection relative to a broader concert scope.</summary>
/// <param name="Selection">The scope whose observed songs are included.</param>
/// <param name="Reference">The broader comparison scope.</param>
/// <param name="AsOf">The shared exclusive concert start-time cutoff.</param>
/// <param name="SelectionCoverage">Setlist coverage in the selection.</param>
/// <param name="ReferenceCoverage">Setlist coverage in the reference.</param>
/// <param name="RarityThreshold">Optional inclusive rarity threshold, between 0 and 1.</param>
/// <param name="Songs">Songs observed in the selection.</param>
public sealed record SongRarityStatsBo(
    StatsScope Selection,
    StatsScope Reference,
    DateTimeOffset AsOf,
    ConcertSetlistCoverageBo SelectionCoverage,
    ConcertSetlistCoverageBo ReferenceCoverage,
    decimal? RarityThreshold,
    IReadOnlyList<SongRarityBo> Songs);

/// <summary>Setlist availability for eligible, non-deleted past concerts.</summary>
/// <param name="ConcertCount">Number of eligible concerts.</param>
/// <param name="ConcertsWithSetlists">Number of concerts with at least one setlist, even if it is empty or incomplete.</param>
public sealed record ConcertSetlistCoverageBo(int ConcertCount, int ConcertsWithSetlists)
{
    /// <summary>Concerts for which no setlist exists.</summary>
    public int ConcertsWithoutSetlists => ConcertCount - ConcertsWithSetlists;
}

/// <summary>Observed live plays and rarity of a song in the comparison scope.</summary>
/// <param name="SongId">Stable base song identifier.</param>
/// <param name="SongName">Song title.</param>
/// <param name="AlbumId">Album identifier, if assigned.</param>
/// <param name="AlbumName">Album title, if assigned.</param>
/// <param name="AppleMusicId">Optional Apple Music identifier.</param>
/// <param name="SelectionPlayCount">Live entries in the selection, including repeats.</param>
/// <param name="SelectionConcertCount">Distinct selection concerts where the song was observed.</param>
/// <param name="ReferencePlayCount">Live entries in the reference, including repeats.</param>
/// <param name="ReferenceConcertCount">Distinct reference concerts where the song was observed.</param>
/// <param name="ReferenceConcertShare">Observed fraction of reference concerts with setlists where the song was played, or null if none have setlists.</param>
/// <param name="IsRare">Whether the observed fraction is at or below the supplied threshold; null when no threshold or share is available.</param>
/// <remarks>Partial or empty setlists may underestimate plays. Recording-only entries and mashups are excluded.</remarks>
public sealed record SongRarityBo(
    uint SongId,
    string SongName,
    uint? AlbumId,
    string? AlbumName,
    string? AppleMusicId,
    uint SelectionPlayCount,
    int SelectionConcertCount,
    uint ReferencePlayCount,
    int ReferenceConcertCount,
    decimal? ReferenceConcertShare,
    bool? IsRare);
