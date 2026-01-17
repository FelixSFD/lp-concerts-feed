using System.Text.Json.Serialization;

namespace Lambda.Push.Sender;

[JsonSerializable(typeof(NotificationWrapper))]
[JsonSerializable(typeof(AppleNotificationAlert))]
[JsonSerializable(typeof(AppleNotificationPayload))]
[JsonSerializable(typeof(SnsMessage))]
public partial class NotificationJsonSerializer: JsonSerializerContext
{
}