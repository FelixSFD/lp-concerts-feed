using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.Model;

namespace Lambda.AdjacentConcerts;

[JsonSerializable(typeof(Dictionary<string, AttributeValue>))]
[JsonSerializable(typeof(Dictionary<string, Condition>))]
public partial class LocalJsonContext: JsonSerializerContext
{
    
}