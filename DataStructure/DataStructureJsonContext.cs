using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Entities;
using LPCalendar.DataStructure.Events;
using LPCalendar.DataStructure.Events.PushNotifications;
using LPCalendar.DataStructure.Requests;
using LPCalendar.DataStructure.Responses;

namespace LPCalendar.DataStructure;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(ConcertDto))]
[JsonSerializable(typeof(ConcertWithSetlistsDto))]
[JsonSerializable(typeof(ConcertWithBookmarkStatusResponse))]
[JsonSerializable(typeof(ConcertBookmark.BookmarkStatus))]
[JsonSerializable(typeof(IEnumerable<ConcertBookmark.BookmarkStatus>))]
[JsonSerializable(typeof(List<ConcertDto>))]
[JsonSerializable(typeof(List<ConcertWithBookmarkStatusResponse>))]
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
[JsonSerializable(typeof(NotificationUserEndpoint))]
[JsonSerializable(typeof(RegisterNotificationDeviceRequest))]
[JsonSerializable(typeof(PushNotificationEvent))]
[JsonSerializable(typeof(SetlistSongPremiereNotificationEvent))]
[JsonSerializable(typeof(ConcertRelatedPushNotificationEvent))]
[JsonSerializable(typeof(ConcertEventType))]
[JsonSerializable(typeof(ConcertNotificationHistory))]
[JsonSerializable(typeof(UserNotificationSettings))]
[JsonSerializable(typeof(SyncConcertsRequest))]
[JsonSerializable(typeof(SyncConcertsResponse))]
public partial class DataStructureJsonContext : JsonSerializerContext
{
}