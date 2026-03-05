namespace Database.Setlists.DataObjects;

public class Song
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
    
    /// <summary>
    /// Optional to the wiki page on Linkinpedia
    /// </summary>
    public string? LinkinpediaUrl { get; set; }
}