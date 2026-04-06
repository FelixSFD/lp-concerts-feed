namespace LPCalendar.DataStructure.Setlists.Import;

public class ImportSetlistPreviewDto
{
    public string? ConcertId { get; set; }

    public required List<ImportSetlistEntryPreviewDto> Entries { get; set; }
}


public class ImportSetlistEntryPreviewDto
{
    public required string Title { get; set; }
    public string? ExtraNotes { get; set; }

    public uint? FoundSongId { get; set; }
    public uint? FoundSongVariantId { get; set; }
    public uint? FoundMashupId { get; set; }
}