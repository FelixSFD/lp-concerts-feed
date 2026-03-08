using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists.Parameters;

namespace LPCalendar.DataStructure.Setlists;

[JsonSerializable(typeof(CreateSetlistRequestDto))]
[JsonSerializable(typeof(CreateSetlistResponseDto))]
[JsonSerializable(typeof(AddSongToSetlistRequestDto))]
[JsonSerializable(typeof(AddSongToSetlistResponseDto))]
[JsonSerializable(typeof(SetlistEntryDto))]
[JsonSerializable(typeof(SetlistEntryParametersDto))]
[JsonSerializable(typeof(ActParametersDto))]
[JsonSerializable(typeof(SetlistDto))]
public partial class SetlistDtoJsonContext : JsonSerializerContext
{
}