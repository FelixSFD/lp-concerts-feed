using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists.Parameters;

namespace LPCalendar.DataStructure.Setlists;

[JsonSerializable(typeof(CreateSetlistRequestDto))]
[JsonSerializable(typeof(CreateSetlistResponseDto))]
[JsonSerializable(typeof(CreateSongMashupRequestDto))]
[JsonSerializable(typeof(AddSongVariantToSetlistRequestDto))]
[JsonSerializable(typeof(AddSongVariantToSetlistResponseDto))]
[JsonSerializable(typeof(AddSongToSetlistRequestDto))]
[JsonSerializable(typeof(AddSongToSetlistResponseDto))]
[JsonSerializable(typeof(AddSongMashupToSetlistRequestDto))]
[JsonSerializable(typeof(AddSongMashupToSetlistResponseDto))]
[JsonSerializable(typeof(SetlistEntryDto))]
[JsonSerializable(typeof(SetlistEntryParametersDto))]
[JsonSerializable(typeof(ActParametersDto))]
[JsonSerializable(typeof(SetlistDto))]
[JsonSerializable(typeof(SongDto))]
[JsonSerializable(typeof(SongMashupDto))]
[JsonSerializable(typeof(List<SetlistDto>))]
[JsonSerializable(typeof(List<SongDto>))]
[JsonSerializable(typeof(List<SongVariantDto>))]
[JsonSerializable(typeof(List<SongMashupDto>))]
public partial class SetlistDtoJsonContext : JsonSerializerContext
{
}