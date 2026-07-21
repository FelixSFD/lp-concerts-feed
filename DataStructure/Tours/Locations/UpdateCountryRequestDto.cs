namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Request to update an existing country
/// </summary>
public class UpdateCountryRequestDto
{
    /// <summary>
    /// English name for the country
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Name of the country in its native language
    /// </summary>
    public required string NativeName { get; set; }
}