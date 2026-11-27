using Database.Setlists.DataObjects;
using Service.Tours.DataStructure.Stats;

namespace Service.Tours.Statistics;

internal static class SongStatsCalculator
{
    /// <summary>
    /// Calculates song statistics for the provided dictionary of songs and their play counts.
    /// </summary>
    /// <param name="songs">A dictionary where the key is a SongDo object and the value is the play count for that song.</param>
    /// <returns>An array of SongStatBo objects representing the calculated statistics for each song.</returns>
    internal static SongStatBo[] Calculate(IDictionary<SongDo, uint> songs) =>
        [.. songs.Select(entry => new SongStatBo(
            entry.Key.Id,
            entry.Key.Title,
            entry.Key.AlbumId,
            entry.Key.Album?.Title,
            entry.Key.AppleMusicId,
            entry.Value))];
}

