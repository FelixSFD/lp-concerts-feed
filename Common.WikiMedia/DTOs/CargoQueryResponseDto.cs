using System.Text.Json.Serialization;

namespace Common.WikiMedia.DTOs;

/// <summary>
/// Result of a Cargo query through the MediaWiki API
/// </summary>
public class CargoQueryResponseDto<T>
{
    [JsonPropertyName("cargoquery")]
    public CargoQueryResponseItemDto<T>[] Results { get; set; }
}

/// <summary>
/// Item in the list of results of a Cargo query
/// </summary>
/// <typeparam name="T"></typeparam>
public class CargoQueryResponseItemDto<T>
{
    [JsonPropertyName("title")]
    public required T Value { get; set; }
}