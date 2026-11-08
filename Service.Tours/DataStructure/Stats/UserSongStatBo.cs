namespace Service.Tours.DataStructure.Stats;

/// <summary>
/// Information about how often a user has seen a specific song live in concert
/// </summary>
public class UserSongStatBo
{
    /// <summary>
    /// ID of the song
    /// </summary>
    public uint SongId { get; set; }
    
    /// <summary>
    /// Name of the song
    /// </summary>
    public required string SongName { get; set; }

    /// <summary>
    /// Name of the album
    /// </summary>
    public string? AlbumName { get; set; }
    
    /// <summary>
    /// ID of the song on Apple Music
    /// </summary>
    public string? AppleMusicId { get; set; }
    
    /// <summary>
    /// The number of times the user has seen this song live in concert
    /// </summary>
    public uint PlayCount { get; set; }
}