using System.ComponentModel.DataAnnotations;

namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Represents a country
/// </summary>
public class CountryBo
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

    /// <summary>
    /// Time when this country was created
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Time when this country was last updated
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Time when this country was deleted
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Time when this country was last changed
    /// </summary>
    public DateTimeOffset? LastChange => UpdatedAt;
}