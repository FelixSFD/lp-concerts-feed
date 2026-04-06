using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists.Import;

public class ImportSetlistPreviewDto
{
    [JsonPropertyName("concertId")]
    public string? ConcertId { get; set; }

    [JsonPropertyName("entries")]
    public required List<ImportSetlistEntryPreviewDto> Entries { get; set; }
}


public class ImportSetlistEntryPreviewDto
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("extraNotes")]
    public string? ExtraNotes { get; set; }

    /// <summary>
    /// If a song was uniquely identified by title, its ID will be added here
    /// </summary>
    [JsonPropertyName("foundSongId")]
    public uint? FoundSongId { get; set; }

    /// <summary>
    /// If multiple songs were found, this is the list of IDs that could match the song
    /// </summary>
    [JsonPropertyName("foundSongIds")]
    public uint[] FoundSongIds { get; set; } = [];
    
    [JsonPropertyName("foundSongVariantId")]
    public uint? FoundSongVariantId { get; set; }
    
    [JsonPropertyName("foundMashupId")]
    public uint? FoundMashupId { get; set; }
}