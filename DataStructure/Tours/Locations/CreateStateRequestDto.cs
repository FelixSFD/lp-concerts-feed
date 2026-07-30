

namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Request to create a new state in a country
/// </summary>
public class CreateStateRequestDto
{
    public required string Code { get; set; }

    /// <summary>
    /// English name of this state
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// name of this state in its native language
    /// </summary>
    public required string NativeName { get; set; }
}