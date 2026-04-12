using System.Text.Json.Serialization;
using Common.WikiMedia.DTOs;

namespace Common.WikiMedia;

[JsonSerializable(typeof(WikiPageDto))]
[JsonSerializable(typeof(LicenseDto))]
[JsonSerializable(typeof(LatestRevisionDto))]
public partial class WikiMediaJsonContext : JsonSerializerContext
{
    
}