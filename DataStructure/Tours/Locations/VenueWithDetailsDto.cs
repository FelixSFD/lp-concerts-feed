namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Information about a venue including all details
/// </summary>
public class VenueWithDetailsDto : VenueWithCityDto
{
    /// <summary>
    /// List of all names the venue has/had
    /// </summary>
    public required PreviousVenueNameDto[] VenueNames { get; set; }
}