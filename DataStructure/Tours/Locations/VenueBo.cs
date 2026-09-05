using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Tours.Locations;

/// <summary>
/// Information about a venue
/// </summary>
public class VenueBo
{
    /// <summary>
    /// Unique ID of this venue
    /// </summary>
    public uint Id { get; set; }
    
    /// <summary>
    /// ISO code of the country where the venue is located in
    /// </summary>
    [MinLength(3)]
    [MaxLength(3)]
    public required string CountryCode { get; set; }
    
    /// <summary>
    /// Code of the state where the venue is located in
    /// </summary>
    [MaxLength(3)]
    public string? StateCode { get; set; }
    
    /// <summary>
    /// ID of the city where the venue is located in
    /// </summary>
    public uint CityId { get; set; }

    /// <summary>
    /// Current name of this venue
    /// </summary>
    public required string CurrentName { get; set; }
    
    /// <summary>
    /// Time Zone of this venue
    /// </summary>
    public required string TimeZone { get; set; }

    /// <summary>
    /// Latitude of the venue
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Latitude { get; set; }
    
    /// <summary>
    /// Longitude of the venue
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Longitude { get; set; }
}