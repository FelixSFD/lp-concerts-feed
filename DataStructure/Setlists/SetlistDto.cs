namespace LPCalendar.DataStructure.Setlists;

public class SetlistDto
{
    /// <summary>
    /// ID of the setlist
    /// </summary>
    public uint Id { get; set; }
    
    /// <summary>
    /// ID of the <see cref="Concert"/> where this set was played
    /// </summary>
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// URL to the wiki page on Linkinpedia
    /// </summary>
    public string? LinkinpediaUrl { get; set; }
}