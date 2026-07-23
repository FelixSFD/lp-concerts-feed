namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Request to add a new leg to a tour
/// </summary>
public class AddTourLegRequestDto
{
    /// <summary>
    /// Unique ID of this leg
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Name of this tour leg
    /// </summary>
    public required string Name { get; set; }
}