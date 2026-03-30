using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Creates a new album
/// </summary>
public class CreateAlbumRequestDto
{
    /// <summary>
    /// Unique ID of the album
    /// </summary>
    [JsonPropertyName("id")]
    public uint Id { get; set; }

    /// <summary>
    /// Name of the album
    /// </summary>
    [JsonPropertyName("title")]
    [MaxLength(31)]
    public required string Title { get; set; }

    /// <summary>
    /// Link to the wiki page on Linkinpedia
    /// </summary>
    [JsonPropertyName("linkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
}