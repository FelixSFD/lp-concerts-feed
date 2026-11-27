using LPCalendar.DataStructure.Tours;

namespace Service.Tours.DataStructure.Stats;

/// <summary>Concert and location statistics for a selected scope.</summary>
/// <param name="Concerts">The array of concert details.</param>
/// <param name="NumberOfConcerts">The total number of concerts.</param>
/// <param name="NumberOfVenues">The total number of venues.</param>
/// <param name="NumberOfCities">The total number of cities.</param>
/// <param name="NumberOfCountries">The total number of countries.</param>
public sealed record ConcertStatsBo(
    ConcertDetailsBo[] Concerts,
    int NumberOfConcerts,
    int NumberOfVenues,
    int NumberOfCities,
    int NumberOfCountries);
