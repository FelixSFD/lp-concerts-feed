namespace Database.Setlists.DataObjects;

/// <summary>
/// Variation of a <see cref="Song"/>
/// </summary>
public class SongVariant
{
    /// <summary>
    /// Unique ID of the variant
    /// </summary>
    public uint Id { get; set; }
    
    
    /// <summary>
    /// ID of the <see cref="Song"/> that is the base for this variation
    /// </summary>
    public uint SongId { get; set; }
    
    
    /// <summary>
    /// Overrides the <see cref="Song.Isrc"/> code which helps to find the song on Apple Music or Spotify.
    /// </summary>
    public string? IsrcOverride { get; set; }
    
    
    /// <summary>
    /// Name of this variant. This will be visible as the Song-Title in the setlist
    /// </summary>
    public string? VariantName { get; set; }
    
    
    /// <summary>
    /// Optional description of the variant. What makes this variant different from the original?
    /// </summary>
    public string? Description { get; set; }
}