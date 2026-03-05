namespace Database.Setlists.DataObjects;

public class Album
{
    /// <summary>
    /// Unique ID of the album
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Name of the album
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Optional to the wiki page on Linkinpedia
    /// </summary>
    public string? LinkinpediaUrl { get; set; }
}