using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Requests;

/// <summary>
/// Request to sync concerts from your local database with the server
/// </summary>
public class SyncConcertsRequest
{
    /// <summary>
    /// List of all IDs of concerts currently stored locally
    /// </summary>
    [JsonPropertyName("localConcertIds")]
    public required string[] LocalConcertIds { get; set; }

    /// <summary>
    /// Date and time of the last sync
    /// </summary>
    [JsonPropertyName("lastSync")]
    public required DateTimeOffset LastSync { get; set; }
}