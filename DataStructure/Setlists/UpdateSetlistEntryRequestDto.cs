using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists.Parameters;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to update a single setlist entry
/// </summary>
public class UpdateSetlistEntryRequestDto
{
    /// <summary>
    /// If set, the added song will be added to this Act.
    /// Title isn't required if the Act already exists.
    /// </summary>
    [JsonPropertyName("actParameters")]
    public ActParametersDto? ActParameters { get; set; }

    
    /// <summary>
    /// Common parameters for that entry, no matter if it was a song or mashup
    /// </summary>
    [JsonPropertyName("entryParameters")]
    public required SetlistEntryParametersDto EntryParameters { get; set; }
    
    
    /// <summary>
    /// If set, this contains the data of the song that was played.
    /// Only one of <see cref="SongMashupParameters"/>, <see cref="SongParameters"/> or <see cref="SongVariantParameters"/> can be set.
    /// </summary>
    [JsonPropertyName("songParameters")]
    public SongParametersDto? SongParameters { get; set; }
    
    
    /// <summary>
    /// If set, this contains the data of the song variant that was played.
    /// Only one of <see cref="SongMashupParameters"/>, <see cref="SongParameters"/> or <see cref="SongVariantParameters"/> can be set.
    /// </summary>
    [JsonPropertyName("songVariantParameters")]
    public SongVariantParametersDto? SongVariantParameters { get; set; }
    
    
    /// <summary>
    /// If set, this contains the data of the song mashup that was played.
    /// Only one of <see cref="SongMashupParameters"/>, <see cref="SongParameters"/> or <see cref="SongVariantParameters"/> can be set.
    /// </summary>
    [JsonPropertyName("songMashupParameters")]
    public SongMashupParametersDto? SongMashupParameters { get; set; }
}