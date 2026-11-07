using LPCalendar.DataStructure.Tours;

namespace Service.Tours.DataStructure.Stats;

/// <summary>
/// Statistics about the concerts a user has attended
/// </summary>
public class UserConcertStatsBo
{
    /// <summary>
    /// All concerts the user has attended
    /// </summary>
    public required RawConcertBo[] Concerts { get; set; }

    /// <summary>
    /// The number of concerts the user has attended
    /// </summary>
    public int NumberOfConcerts { get; set; }

    /// <summary>
    /// The number of countries, the user has attended concerts at
    /// </summary>
    public int NumberOfCountries { get; set; }
    
    /// <summary>
    /// The number of cities, the user has attended concerts at
    /// </summary>
    public int NumberOfCities { get; set; }

    /// <summary>
    /// The number of venues, the user has attended concerts at
    /// </summary>
    public int NumberOfVenues { get; set; }
}