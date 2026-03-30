using System.Text.Json.Serialization;
using LPCalendar.DataStructure.Setlists.Parameters;

namespace LPCalendar.DataStructure.Setlists;

[JsonSerializable(typeof(CreateAlbumRequestDto))]
[JsonSerializable(typeof(CreateSetlistRequestDto))]
[JsonSerializable(typeof(CreateSetlistResponseDto))]
[JsonSerializable(typeof(CreateSongMashupRequestDto))]
[JsonSerializable(typeof(CreateSongRequestDto))]
[JsonSerializable(typeof(UpdateAlbumRequestDto))]
[JsonSerializable(typeof(UpdateSongRequestDto))]
[JsonSerializable(typeof(UpdateSongMashupRequestDto))]
[JsonSerializable(typeof(UpdateSetlistHeaderRequestDto))]
[JsonSerializable(typeof(UpdateSetlistEntryRequestDto))]
[JsonSerializable(typeof(ReorderSetlistEntriesRequestDto))]
[JsonSerializable(typeof(ReorderSetlistEntriesResponseDto))]
[JsonSerializable(typeof(AddSongVariantToSetlistRequestDto))]
[JsonSerializable(typeof(AddSongVariantToSetlistResponseDto))]
[JsonSerializable(typeof(AddSongToSetlistRequestDto))]
[JsonSerializable(typeof(AddSongToSetlistResponseDto))]
[JsonSerializable(typeof(AddSongMashupToSetlistRequestDto))]
[JsonSerializable(typeof(AddSongMashupToSetlistResponseDto))]
[JsonSerializable(typeof(SetlistEntryDto))]
[JsonSerializable(typeof(RawSetlistEntryDto))]
[JsonSerializable(typeof(SetlistEntryParametersDto))]
[JsonSerializable(typeof(ActParametersDto))]
[JsonSerializable(typeof(SetlistDto))]
[JsonSerializable(typeof(SetlistHeaderDto))]
[JsonSerializable(typeof(AlbumDto))]
[JsonSerializable(typeof(SongDto))]
[JsonSerializable(typeof(SongMashupDto))]
[JsonSerializable(typeof(List<AlbumDto>))]
[JsonSerializable(typeof(List<SetlistActDto>))]
[JsonSerializable(typeof(List<SetlistDto>))]
[JsonSerializable(typeof(List<SetlistHeaderDto>))]
[JsonSerializable(typeof(List<SongDto>))]
[JsonSerializable(typeof(List<SongVariantDto>))]
[JsonSerializable(typeof(List<SongMashupDto>))]
public partial class SetlistDtoJsonContext : JsonSerializerContext
{
}