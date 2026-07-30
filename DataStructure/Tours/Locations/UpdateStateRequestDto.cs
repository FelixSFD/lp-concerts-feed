namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Request to update an existing state in a country
/// </summary>
public class UpdateStateRequestDto
{
    /// <summary>
    /// English name of this state
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// name of this state in its native language
    /// </summary>
    public required string NativeName { get; set; }
}