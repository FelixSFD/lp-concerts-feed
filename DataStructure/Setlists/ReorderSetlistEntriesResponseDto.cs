using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Response when the setlist was reordered
/// </summary>
public class ReorderSetlistEntriesResponseDto
{
    /// <summary>
    /// The newly reordered entries
    /// </summary>
    [JsonPropertyName("reorderedEntries")]
    public List<SetlistEntryDto> ReorderedEntries
    {
        get;
        set;
    } = [];
}