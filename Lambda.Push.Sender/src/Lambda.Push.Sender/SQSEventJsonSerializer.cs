using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace Lambda.Push.Sender;

[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(SQSEvent.SQSMessage))]
[JsonSerializable(typeof(List<SQSEvent.SQSMessage>))]
public partial class SqsEventJsonSerializer: JsonSerializerContext
{
}