using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Variation of a <see cref="SongDto"/>
/// </summary>
public class SongVariantDto
{
    /// <summary>
    /// Unique ID of the variant
    /// </summary>
    [JsonPropertyName("id")]
    public uint Id { get; set; }
    
    
    /// <summary>
    /// ID of the <see cref="SongDto"/> that is the base for this variation
    /// </summary>
    [JsonPropertyName("songId")]
    public uint SongId { get; set; }
    
    /// <summary>
    /// Overrides the <see cref="SongDo.Isrc"/> code which helps to find the song on Apple Music or Spotify.
    /// </summary>
    [MaxLength(15)]
    [JsonPropertyName("isrcOverride")]
    public string? IsrcOverride { get; set; }
    
    /// <summary>
    /// Name of this variant. This will be visible as the Song-Title in the setlist
    /// </summary>
    [MaxLength(31)]
    [JsonPropertyName("variantName")]
    public string? VariantName { get; set; }
    
    
    /// <summary>
    /// Optional description of the variant. What makes this variant different from the original?
    /// </summary>
    [MaxLength(63)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}