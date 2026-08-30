namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Information about a venue including all details
/// </summary>
public class VenueWithDetailsBo : VenueWithCityBo
{
    /// <summary>
    /// List of all names the venue has/had
    /// </summary>
    public required PreviousVenueNameBo[] VenueNames { get; set; }
}