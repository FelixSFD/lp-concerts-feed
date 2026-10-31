using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Response after a new song variant was added to the setlist
/// </summary>
public class AddSongVariantToSetlistResponseBo
{
    /// <summary>
    /// Response after a new song was added to the setlist
    /// </summary>
    public AddSongVariantToSetlistResponseBo(SetlistEntryBo addedSetlistEntry)
    {
        AddedSetlistEntry = addedSetlistEntry;
    }

    /// <summary>
    /// The new entry that has been added
    /// </summary>
    [JsonPropertyName("addedSetlistEntry")]
    public SetlistEntryBo AddedSetlistEntry { get; set; }
}