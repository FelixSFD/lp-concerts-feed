using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Response after a new song mashup was added to the setlist
/// </summary>
public class AddSongMashupToSetlistResponseBo
{
    /// <summary>
    /// Response after a new song was added to the setlist
    /// </summary>
    public AddSongMashupToSetlistResponseBo(SetlistEntryBo addedSetlistEntry)
    {
        AddedSetlistEntry = addedSetlistEntry;
    }

    /// <summary>
    /// The new entry that has been added
    /// </summary>
    [JsonPropertyName("addedSetlistEntry")]
    public SetlistEntryBo AddedSetlistEntry { get; set; }
}