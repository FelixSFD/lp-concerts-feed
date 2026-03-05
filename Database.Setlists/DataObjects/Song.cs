namespace Database.Setlists.DataObjects;

public class Song : ILinkinpediaLinkable
{
    /// <summary>
    /// Unique ID of the song
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Name of the song
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// ISRC code helps to find the song on Apple Music or Spotify
    /// </summary>
    public string? Isrc { get; set; }
    
    /// <inheritdoc/>
    public string? LinkinpediaUrl { get; set; }
}