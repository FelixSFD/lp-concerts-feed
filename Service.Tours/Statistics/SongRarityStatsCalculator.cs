using Database.Setlists.Repositories;
using Service.Tours.DataStructure.Stats;

namespace Service.Tours.Statistics;

internal static class SongRarityStatsCalculator
{
    internal static SongRarityStatsBo Calculate(
        StatsScope selection, StatsScope reference, DateTimeOffset asOf,
        HashSet<string> selectionIds, HashSet<string> referenceIds,
        SongConcertPlayData data, decimal? rarityThreshold)
    {
        var coveredIds = data.ConcertIdsWithSetlists.Where(referenceIds.Contains).ToHashSet();
        // An observed entry also proves a setlist exists, even if it appeared after the coverage query.
        coveredIds.UnionWith(data.SongPlays.Select(play => play.ConcertId).Where(referenceIds.Contains));
        var selectionCoverage = new ConcertSetlistCoverageBo(selectionIds.Count, coveredIds.Count(selectionIds.Contains));
        var referenceCoverage = new ConcertSetlistCoverageBo(referenceIds.Count, coveredIds.Count);

        var songs = data.SongPlays
            .Where(play => referenceIds.Contains(play.ConcertId))
            .GroupBy(play => play.Song.Id)
            .Select(group =>
            {
                var selected = group.Where(play => selectionIds.Contains(play.ConcertId)).ToArray();
                if (selected.Length == 0)
                    return null;

                var song = selected[0].Song;
                var referenceConcertCount = group.Select(play => play.ConcertId).Distinct().Count();
                decimal? share = referenceCoverage.ConcertsWithSetlists == 0
                    ? null
                    : (decimal)referenceConcertCount / referenceCoverage.ConcertsWithSetlists;

                return new SongRarityBo(
                    song.Id, song.Title, song.AlbumId, song.Album?.Title, song.AppleMusicId,
                    selected.Aggregate(0u, (count, play) => checked(count + play.PlayCount)),
                    selected.Select(play => play.ConcertId).Distinct().Count(),
                    group.Aggregate(0u, (count, play) => checked(count + play.PlayCount)),
                    referenceConcertCount, share,
                    rarityThreshold.HasValue && share.HasValue ? share.Value <= rarityThreshold.Value : null);
            })
            .Where(song => song is not null)
            .Cast<SongRarityBo>()
            .OrderBy(song => song.SongName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(song => song.SongId)
            .ToArray();

        return new SongRarityStatsBo(selection, reference, asOf, selectionCoverage, referenceCoverage, rarityThreshold, songs);
    }
}

