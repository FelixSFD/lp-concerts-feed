using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to update header-information of a setlist
/// </summary>
public class UpdateSetlistHeaderRequestDto
{
    /// <summary>
    /// Name of the set
    /// </summary>
    [MaxLength(63)]
    [JsonPropertyName("setName")]
    public string? SetName { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [MaxLength(63)]
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
}