using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

public class AddSongExtraToSetlistEntryResponseDto
{
    /// <summary>
    /// The <see cref="RawSetlistEntryDto"/> where this extra was added
    /// </summary>
    [JsonPropertyName("entry")]
    public required RawSetlistEntryDto Entry { get; set; }
}