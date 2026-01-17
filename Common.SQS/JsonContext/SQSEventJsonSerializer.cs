using System.Text.Json.Serialization;
using Amazon.Lambda.SQSEvents;

namespace Common.SQS.JsonContext;

[JsonSerializable(typeof(SQSEvent))]
[JsonSerializable(typeof(SQSEvent.SQSMessage))]
[JsonSerializable(typeof(List<SQSEvent.SQSMessage>))]
public partial class SqsEventJsonSerializer: JsonSerializerContext
{
}