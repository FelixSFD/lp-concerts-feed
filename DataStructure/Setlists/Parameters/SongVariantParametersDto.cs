using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists.Parameters;

/// <summary>
/// Set of parameters used when song variant data is passed to the server
/// </summary>
public class SongVariantParametersDto
{
    /// <summary>
    /// If you want to add an existing song variant, this parameter must contain the ID of the variant
    /// </summary>
    [JsonPropertyName("songVariantId")]
    public uint? SongVariantId { get; set; }

    /// <summary>
    /// ID of the song
    /// If this is null, the <see cref="SongVariantId"/> must be set
    /// </summary>
    [JsonPropertyName("songId")]
    public uint? SongId { get; set; }

    /// <summary>
    /// Name of this variant. This will be visible as the Song-Title in the setlist. If this is null, the <see cref="SongVariantId"/> must be set
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
    
    
    /// <summary>
    /// Optional field to override the ISRC of the song
    /// </summary>
    [JsonPropertyName("isrcOverride")]
    public string? IsrcOverride { get; set; }
    
    
    /// <summary>
    /// Overrides the <see cref="SongDto.AppleMusicId"/> which is a unique ID for the Song on Apple Music
    /// </summary>
    [JsonPropertyName("appleMusicIdOverride")]
    public string? AppleMusicIdOverride { get; set; }
}