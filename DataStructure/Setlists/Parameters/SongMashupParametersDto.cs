using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists.Parameters;

/// <summary>
/// Set of parameters used when song mashup data is passed to the server
/// </summary>
public class SongMashupParametersDto
{
    /// <summary>
    /// If you want to add an existing song mashup, this parameter must contain the ID of the mashup
    /// </summary>
    [JsonPropertyName("songMashupId")]
    public required uint SongMashupId { get; set; }
}