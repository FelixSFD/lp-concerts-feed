using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Response after a new entry was added to the setlist
/// </summary>
public class AddCustomEntryToSetlistResponseDto
{
    /// <summary>
    /// Response after a new entry was added to the setlist
    /// </summary>
    public AddCustomEntryToSetlistResponseDto(SetlistEntryDto addedSetlistEntry)
    {
        AddedSetlistEntry = addedSetlistEntry;
    }

    /// <summary>
    /// The new entry that has been added
    /// </summary>
    [JsonPropertyName("addedSetlistEntry")]
    public SetlistEntryDto AddedSetlistEntry { get; set; }
}