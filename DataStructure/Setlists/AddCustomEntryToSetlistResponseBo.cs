using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Response after a new entry was added to the setlist
/// </summary>
public class AddCustomEntryToSetlistResponseBo
{
    /// <summary>
    /// Response after a new entry was added to the setlist
    /// </summary>
    public AddCustomEntryToSetlistResponseBo(SetlistEntryBo addedSetlistEntry)
    {
        AddedSetlistEntry = addedSetlistEntry;
    }

    /// <summary>
    /// The new entry that has been added
    /// </summary>
    [JsonPropertyName("addedSetlistEntry")]
    public SetlistEntryBo AddedSetlistEntry { get; set; }
}