using System.Text.Json.Serialization;
using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;
using Amazon.SQS.Model;

namespace Lambda.ConcertReminder;

[JsonSerializable(typeof(ScheduledEvent))]
[JsonSerializable(typeof(SendMessageRequest))]
public partial class LocalJsonSerializer : JsonSerializerContext
{
}