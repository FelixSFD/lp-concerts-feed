namespace Database.Setlists.DataObjects;

public class Setlist
{
    /// <summary>
    /// Unique ID of the setlist
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// ID of the Concert
    /// </summary>
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// Optional to the wiki page on Linkinpedia
    /// </summary>
    public string? LinkinpediaUrl { get; set; }
}