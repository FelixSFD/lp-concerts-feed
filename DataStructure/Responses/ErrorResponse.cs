using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Responses;

/// <summary>
/// Response send by the API, if an error occurred
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Information about the error
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}