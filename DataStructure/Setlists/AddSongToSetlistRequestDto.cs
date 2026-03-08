using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists.Parameters;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to add a new song to a setlist
/// </summary>
public class AddSongToSetlistRequestDto
{
    /// <summary>
    /// ID of the setlist where the Song should be added to
    /// </summary>
    [JsonPropertyName("setlistId")]
    public uint SetlistId { get; set; }


    /// <summary>
    /// Parameters of the song for this entry
    /// </summary>
    [JsonPropertyName("songParameters")]
    public required SongParametersDto SongParameters { get; set; }


    /// <summary>
    /// If set, the added song will be added to this Act.
    /// Title isn't required if the Act already exists.
    /// </summary>
    [JsonPropertyName("act")]
    public ActParametersDto? Act { get; set; }

    
    /// <summary>
    /// Common parameters for that entry, no matter if it was a song or mashup
    /// </summary>
    [JsonPropertyName("entryParameters")]
    public required SetlistEntryParametersDto EntryParameters { get; set; }
}