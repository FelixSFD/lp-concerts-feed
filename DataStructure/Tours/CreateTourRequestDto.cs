namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Request to create a new tour
/// </summary>
public class CreateTourRequestDto
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