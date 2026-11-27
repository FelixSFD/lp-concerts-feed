using Database.Setlists.DataObjects;

namespace Database.Setlists.Repositories;

/// <summary>Observed live song entries and concerts with at least one setlist.</summary>
/// <param name="ConcertIdsWithSetlists">Distinct concert IDs with at least one setlist, including empty setlists.</param>
/// <param name="SongPlays">Live song counts grouped by concert and base song.</param>
public sealed record SongConcertPlayData(
    IReadOnlyList<string> ConcertIdsWithSetlists,
    IReadOnlyList<SongConcertPlayCount> SongPlays);

/// <summary>The number of live entries for a base song at one concert.</summary>
/// <param name="Song">Song metadata, including its album when available.</param>
/// <param name="ConcertId">Concert where the song was played.</param>
/// <param name="PlayCount">Number of live entries, including repetitions.</param>
public sealed record SongConcertPlayCount(SongDo Song, string ConcertId, uint PlayCount);
