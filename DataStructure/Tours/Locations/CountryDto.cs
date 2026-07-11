using System.ComponentModel.DataAnnotations;

namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Represents a country
/// </summary>
public class CountryDto
{
    /// <summary>
    /// ISO-alpha-3 code of the country
    /// </summary>
    [MaxLength(3)]
    [MinLength(3)]
    public required string IsoCode { get; set; }

    /// <summary>
    /// English name for the country
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Name of the country in its native language
    /// </summary>
    public required string NativeName { get; set; }
}