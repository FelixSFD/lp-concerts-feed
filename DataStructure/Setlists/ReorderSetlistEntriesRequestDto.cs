using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Request to reorder all entries of a setlist
/// </summary>
public class ReorderSetlistEntriesRequestDto
{
    /// <summary>
    /// All IDs of the entries in the preferred order
    /// </summary>
    [JsonPropertyName("entryIds")]
    public string[] EntryIds { get; set; } = [];
}