namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Request to add a new name to a venue
/// </summary>
public class AddVenueNameRequestDto
{
    /// <summary>
    /// Name of the venue during the given time range
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// When the name started to be the current name
    /// </summary>
    public DateOnly From { get; set; }
    
    /// <summary>
    /// Date until which the name was valid (or will be valid)
    /// </summary>
    public DateOnly? To { get; set; }
}