namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Information about a name of a venue including the time range for which this name is/was valid
/// </summary>
public class PreviousVenueNameBo
{
    /// <summary>
    /// ID of the venue
    /// </summary>
    public required uint VenueId { get; set; }
    
    /// <summary>
    /// ID of this historic name
    /// </summary>
    public required uint Id { get; set; }
    
    /// <summary>
    /// Name of the venue
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Date where the venue started using that name
    /// </summary>
    public required DateOnly UsedFrom { get; set; }
    
    /// <summary>
    /// Date where the venue stopped using that name
    /// </summary>
    public DateOnly? UsedUntil { get; set; }
}