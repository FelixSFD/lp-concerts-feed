

namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// A state within a country including the data of the country
/// </summary>
public class StateWithCountryDto
{
    public required string CountryCode { get; set; }
    
    public required string Code { get; set; }
    
    /// <summary>
    /// Country the state is in
    /// </summary>
    public required CountryDto Country { get; set; }

    /// <summary>
    /// English name of this state
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// name of this state in its native language
    /// </summary>
    public required string NativeName { get; set; }
}