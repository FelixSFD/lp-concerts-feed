using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Response when the setlist was reordered
/// </summary>
public class ReorderSetlistEntriesResponseBo
{
    /// <summary>
    /// The newly reordered entries
    /// </summary>
    [JsonPropertyName("reorderedEntries")]
    public List<SetlistEntryBo> ReorderedEntries
    {
        get;
        set;
    } = [];
}