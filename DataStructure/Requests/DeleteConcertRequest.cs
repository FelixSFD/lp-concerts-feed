using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Requests;

/// <summary>
/// Request to delete a concert
/// </summary>
public class DeleteConcertRequest
{
    /// <summary>
    /// ID of the concert to delete
    /// </summary>
    [JsonPropertyName("concertId")]
    public required string ConcertId { get; set; }
}