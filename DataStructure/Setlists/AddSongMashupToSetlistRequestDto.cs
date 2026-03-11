using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists.Parameters;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to add a new song to a setlist
/// </summary>
public class AddSongMashupToSetlistRequestDto : AddToSetlistRequestDto
{
    /// <summary>
    /// Parameters of the song variant for this entry
    /// </summary>
    [JsonPropertyName("songMashupParameters")]
    public required SongMashupParametersDto SongMashupParameters { get; set; }
}