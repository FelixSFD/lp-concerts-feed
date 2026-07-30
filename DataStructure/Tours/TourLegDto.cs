namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Basic information about a tour leg
/// </summary>
public class TourLegDto
{
    /// <summary>
    /// ID of the Tour
    /// </summary>
    public required string TourId { get; set; }
    
    /// <summary>
    /// Unique ID of the leg
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Name of this tour leg
    /// </summary>
    public required string Name { get; set; }
}