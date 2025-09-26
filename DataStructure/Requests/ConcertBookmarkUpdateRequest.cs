using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Requests;

public class ConcertBookmarkUpdateRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    [JsonPropertyName("status")]
    public required ConcertBookmark.BookmarkStatus Status { get; set; }
}