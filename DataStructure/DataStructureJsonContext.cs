using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Responses;
using ErrorResponse = Amazon.Runtime.Internal.ErrorResponse;

namespace LPCalendar.DataStructure;

[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Concert))]
[JsonSerializable(typeof(List<Concert>))]
[JsonSerializable(typeof(ConcertBookmark))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(GetConcertBookmarkCountsResponse))]
public partial class DataStructureJsonContext : JsonSerializerContext
{
}