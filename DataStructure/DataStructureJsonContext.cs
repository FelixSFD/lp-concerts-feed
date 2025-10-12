using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Concert))]
[JsonSerializable(typeof(List<Concert>))]
[JsonSerializable(typeof(ConcertBookmark))]
public partial class DataStructureJsonContext : JsonSerializerContext
{
}