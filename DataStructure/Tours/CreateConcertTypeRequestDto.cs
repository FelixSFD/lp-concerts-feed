namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Request to create a new concert type
/// </summary>
public class CreateConcertTypeRequestDto
{
    /// <summary>
    /// Displayed name of the type
    /// </summary>
    public required string Name { get; set; }
}