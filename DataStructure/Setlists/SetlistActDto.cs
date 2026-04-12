using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

/// <summary>
/// Information about an act within a setlist
/// </summary>
public class SetlistActDto
{
    /// <summary>
    /// ID of the setlist
    /// </summary>
    [JsonPropertyName("setlistId")]
    public uint SetlistId { get; set; }
    
    
    /// <summary>
    /// Number of the act within the setlist
    /// </summary>
    [JsonPropertyName("actNumber")]
    public uint ActNumber { get; set; }


    /// <summary>
    /// Title of this act
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}