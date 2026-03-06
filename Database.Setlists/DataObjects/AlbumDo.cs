namespace Database.Setlists.DataObjects;

public class AlbumDo : BaseDo, ILinkinpediaLinkable
{
    /// <summary>
    /// Unique ID of the album
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Name of the album
    /// </summary>
    public required string Title { get; set; }

    /// <inheritdoc/>
    public string? LinkinpediaUrl { get; set; }
}