using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Events;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

namespace LPCalendar.DataStructure;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Concert))]
[JsonSerializable(typeof(List<Concert>))]
[JsonSerializable(typeof(AdjacentConcertsResponse))]
[JsonSerializable(typeof(AuditLogEvent))]
[JsonSerializable(typeof(ConcertBookmark))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(InvalidFieldsErrorResponse))]
[JsonSerializable(typeof(GetConcertBookmarkCountsResponse))]
[JsonSerializable(typeof(GetS3UploadUrlRequest))]
[JsonSerializable(typeof(GetS3UploadUrlResponse))]
[JsonSerializable(typeof(GetTimeZoneByCoordinatesResponse))]
[JsonSerializable(typeof(ConcertBookmarkUpdateRequest))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(IEnumerable<User>))]
public partial class DataStructureJsonContext : JsonSerializerContext
{
}