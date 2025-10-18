using System.Text.Json.Serialization;
using Amazon.SQS.Model;

namespace Lambda.DeleteConcert;

[JsonSerializable(typeof(SendMessageRequest))]
public partial class LocalJsonContext: JsonSerializerContext
{
    
}