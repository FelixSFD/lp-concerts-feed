using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Creates a new setlist for a concert
/// </summary>
public class CreateSetlistRequestDto
{
    /// <summary>
    /// ID of the <see cref="Concert"/> where this set was played
    /// </summary>
    [JsonPropertyName("concertId")]
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// Title of this setlist
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [MaxLength(63)]
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
}