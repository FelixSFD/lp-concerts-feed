using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Information about a setlist with all its entries
/// </summary>
public class SetlistBo : SetlistHeaderBo
{
    /// <summary>
    /// Acts of the setlist. The full objects are not included in the <see cref="Entries"/>, only the IDs that you can find in this property.
    /// </summary>
    [JsonPropertyName("acts")]
    public List<SetlistActBo> Acts { get; set; } = [];

    /// <summary>
    /// Entries of the setlist
    /// </summary>
    [JsonPropertyName("entries")]
    public List<SetlistEntryBo> Entries { get; set; } = [];
}