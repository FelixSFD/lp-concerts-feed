using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// A mashup is used when the band plays a special mix of two or more <see cref="SongDto"/>s.
/// Example: "Remember The Name" and "When They Come For Me" during the FROM ZERO WORLD TOUR 2024-2026.
/// </summary>
public class SongMashupDto
{
    /// <summary>
    /// ID of this mashup
    /// </summary>
    [JsonPropertyName("id")]
    public uint Id { get; set; }

    /// <summary>
    /// Name of this mashup
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
    
    /// <summary>
    /// Songs that are included in this mashup
    /// </summary>
    [JsonPropertyName("songs")]
    public required List<SongDto> Songs { get; set; }
}