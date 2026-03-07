using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists;

[JsonSerializable(typeof(CreateSetlistRequestDto))]
[JsonSerializable(typeof(CreateSetlistResponseDto))]
[JsonSerializable(typeof(SetlistDto))]
public partial class SetlistDtoJsonContext : JsonSerializerContext
{
}