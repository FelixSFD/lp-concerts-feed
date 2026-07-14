namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Request to create a new city in the database
/// </summary>
public class CreateCityRequestDto
{
    /// <summary>
    /// City-ID
    /// </summary>
    public uint Id { get; set; }
    
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