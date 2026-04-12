using System.Text.Json.Serialization;

namespace Common.WikiMedia.DTOs;

/// <summary>
/// Data of a Wiki Page
/// </summary>
public class WikiPageDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// Title of the page
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("latest")]
    public LatestRevisionDto? Latest { get; set; }

    /// <summary>
    /// Type of the content (Example: wikitext)
    /// </summary>
    [JsonPropertyName("content_model")]
    public string? ContentModel { get; set; }

    [JsonPropertyName("license")]
    public LicenseDto? License { get; set; }

    /// <summary>
    /// Source of the page. This is in the format specified in <see cref="ContentModel"/>
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
