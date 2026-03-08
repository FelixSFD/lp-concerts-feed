using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists.Parameters;

/// <summary>
/// Set of parameters used when song data is passed to the server
/// </summary>
public class SongParametersDto
{
    /// <summary>
    /// If you want to add an existing song, this parameter must contain the ID
    /// </summary>
    [JsonPropertyName("songId")]
    public uint? SongId { get; set; }


    /// <summary>
    /// Title of the song. If not present, the ID of the song must be set.
    /// </summary>
    [JsonPropertyName("songTitle")]
    public string? SongTitle { get; set; }
    
    
    /// <summary>
    /// ISRC of the song. If not present, the ID of the song must be set.
    /// </summary>
    [JsonPropertyName("isrc")]
    public string? Isrc { get; set; }
}