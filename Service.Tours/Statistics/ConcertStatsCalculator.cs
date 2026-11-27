using Database.Tours.DataObjects;
using Service.Tours.DataStructure.Stats;

namespace Service.Tours.Statistics;

/// <summary>
/// Calculates concert statistics for a given set of concerts, including counts of concerts, venues, cities, and countries.
/// </summary>
internal static class ConcertStatsCalculator
{
    /// <summary>
    /// Calculates concert statistics for the provided array of concerts.
    /// </summary>
    /// <param name="concerts">Concerts to calculate the stats for</param>
    /// <returns>Stats</returns>
    internal static ConcertStatsBo Calculate(ConcertDo[] concerts) => new(
        [.. concerts.Select(DoMapper.ToBoWithDetails)],
        concerts.Length,
        concerts.Select(c => c.VenueId).Distinct().Count(),
        concerts.Select(c => c.Venue.CityId).Distinct().Count(),
        concerts.Select(c => c.Venue.City.CountryCode).Distinct().Count());
}
