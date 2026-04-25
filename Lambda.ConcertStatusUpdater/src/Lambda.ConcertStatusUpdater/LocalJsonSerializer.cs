using System.Text.Json.Serialization;
using Amazon.Lambda.CloudWatchEvents.ScheduledEvents;

namespace Lambda.ConcertStatusUpdater;

[JsonSerializable(typeof(ScheduledEvent))]
public partial class LocalJsonSerializer : JsonSerializerContext
{
}