using System.Text.Json.Serialization;

namespace Lambda.GetTimeZone;

[JsonSerializable(typeof(TimeZoneDbResponse))]
public partial class LocalJsonContext: JsonSerializerContext
{
    
}