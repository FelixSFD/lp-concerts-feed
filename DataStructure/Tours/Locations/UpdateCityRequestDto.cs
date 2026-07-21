namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Request to update an existing city in the database
/// </summary>
public class UpdateCityRequestDto
{
    /// <summary>
    /// State code
    /// </summary>
    public string? StateCode { get; set; }
    
    /// <summary>
    /// English name of this city
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// name of this city in its native language
    /// </summary>
    public required string NativeName { get; set; }
}