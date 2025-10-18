using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Responses;

/// <summary>
/// Holds information about the previous and next concert
/// </summary>
public class AdjacentConcertsResponse
{
    /// <summary>
    /// ID of current concert
    /// </summary>
    [JsonPropertyName("current")]
    public required string Current { get; set; }

    
    /// <summary>
    /// ID of previous concert
    /// </summary>
    [JsonPropertyName("previous")]
    public string? Previous { get; set; }
    
    /// <summary>
    /// ID of next concert
    /// </summary>
    [JsonPropertyName("next")]
    public string? Next { get; set; }
}