namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Basic information about a tour
/// </summary>
public class TourDto
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Name of this tour
    /// </summary>
    public required string Name { get; set; }
}