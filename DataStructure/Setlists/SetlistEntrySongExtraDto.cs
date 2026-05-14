using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

public class SetlistEntrySongExtraDto
{
    public enum ExtraType
    {
        ExtendedBridge,
        ExtraVerse
    }
    
    /// <summary>
    /// ID of this extra
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// Optional <see cref="SongDto"/> that was added to a different song if it's a song that is referenced in our database.
    /// </summary>
    [JsonPropertyName("song")]
    public SongDto? Song { get; set; }
    
    /// <summary>
    /// Defines how the <see cref="Song"/> was included
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    
    /// <summary>
    /// Description of the extra
    /// </summary>
    [JsonPropertyName("description")]
    [MaxLength(127)]
    public required string Description { get; set; }
}