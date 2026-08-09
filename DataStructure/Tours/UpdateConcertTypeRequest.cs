namespace LPCalendar.DataStructure.Tours;

/// <summary>
/// Request to update a concert type
/// </summary>
public class UpdateConcertTypeRequest
{
    /// <summary>
    /// Displayed name of the type
    /// </summary>
    public required string Name { get; set; }
}