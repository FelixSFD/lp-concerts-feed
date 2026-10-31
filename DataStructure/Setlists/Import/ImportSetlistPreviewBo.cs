using System.Text.Json.Serialization;

namespace LPCalendar.DataStructure.Setlists.Import;

public class ImportSetlistPreviewBo
{
    [JsonPropertyName("concertId")]
    public string? ConcertId { get; set; }
    
    [JsonPropertyName("acts")]
    public required List<ImportSetlistActPreviewBo> Acts { get; set; }

    [JsonPropertyName("entries")]
    public required List<ImportSetlistEntryPreviewBo> Entries { get; set; }
}


public class ImportSetlistActPreviewBo
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Act number
    /// </summary>
    [JsonPropertyName("actNumber")]
    public required uint ActNumber { get; set; }
    
    [JsonPropertyName("extraNotes")]
    public string? ExtraNotes { get; set; }
}


public class ImportSetlistEntryPreviewBo
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("extraNotes")]
    public string? ExtraNotes { get; set; }

    /// <summary>
    /// Number of the song
    /// </summary>
    [JsonPropertyName("songNumber")]
    public uint SongNumber { get; set; }
    
    /// <summary>
    /// Act number
    /// </summary>
    [JsonPropertyName("actNumber")]
    public uint? ActNumber { get; set; }

    /// <summary>
    /// If a song was uniquely identified by title, its ID will be added here
    /// </summary>
    [JsonPropertyName("foundSongId")]
    public uint? FoundSongId { get; set; }

    /// <summary>
    /// If <see cref="FoundSongId"/> was found and the song has variants, all possible variants are listed here
    /// </summary>
    [JsonPropertyName("possibleSongVariants")]
    public SongVariantBo[] PossibleSongVariants { get; set; } = [];

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