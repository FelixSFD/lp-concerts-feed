using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Response after a new song was added to the setlist
/// </summary>
public class AddSongToSetlistResponseDto
{
    /// <summary>
    /// Response after a new song was added to the setlist
    /// </summary>
    public AddSongToSetlistResponseDto(SetlistEntryDto addedSetlistEntry)
    {
        AddedSetlistEntry = addedSetlistEntry;
    }

    /// <summary>
    /// The new entry that has been added
    /// </summary>
    [JsonPropertyName("addedSetlistEntry")]
    public SetlistEntryDto AddedSetlistEntry { get; set; }
}