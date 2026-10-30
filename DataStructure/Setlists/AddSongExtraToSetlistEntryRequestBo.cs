using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

public class AddSongExtraToSetlistEntryRequestBo
{
    /// <summary>
    /// Optional ID of the <see cref="SongBo"/> that was added to a different song if it's a song that is referenced in our database.
    /// </summary>
    [JsonPropertyName("songId")]
    public uint? SongId { get; set; }
    
    /// <summary>
    /// Defines how the <see cref="SongId"/> was included
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<SetlistEntrySongExtraBo.ExtraType>))]
    [JsonPropertyName("type")]
    public SetlistEntrySongExtraBo.ExtraType Type { get; set; }
    
    /// <summary>
    /// Description of the extra
    /// </summary>
    [JsonPropertyName("description")]
    [MaxLength(127)]
    public required string Description { get; set; }
}