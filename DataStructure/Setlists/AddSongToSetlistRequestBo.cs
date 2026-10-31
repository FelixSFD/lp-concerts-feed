using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists.Parameters;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to add a new song to a setlist
/// </summary>
public class AddSongToSetlistRequestBo : AddToSetlistRequestBo
{
    /// <summary>
    /// Parameters of the song for this entry
    /// </summary>
    [JsonPropertyName("songParameters")]
    public required SongParametersBo SongParameters { get; set; }
}