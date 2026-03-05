namespace Database.Setlists.DataObjects;

public class Setlist : ILinkinpediaLinkable
{
    /// <summary>
    /// Unique ID of the setlist
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// ID of the Concert
    /// </summary>
    public required string ConcertId { get; set; }
    
    /// <inheritdoc/>
    public string? LinkinpediaUrl { get; set; }
}