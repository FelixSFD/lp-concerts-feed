using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

public class AddSongExtraToSetlistEntryResponseBo
{
    /// <summary>
    /// The <see cref="RawSetlistEntryBo"/> where this extra was added
    /// </summary>
    [JsonPropertyName("entry")]
    public required RawSetlistEntryBo Entry { get; set; }
}