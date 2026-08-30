namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Information about a city
/// </summary>
public class CityBo
{
    /// <summary>
    /// Country code
    /// </summary>
    public required string CountryCode { get; set; }
    
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