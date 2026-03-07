using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

/// <summary>
/// A mashup is used when the band plays a special mix of two or more <see cref="SongDo"/>s.
/// Example: "Remember The Name" and "When They Come For Me" during the FROM ZERO WORLD TOUR 2024-2026.
/// </summary>
[Table("SongMashup")]
public class SongMashupDo: BaseDo, ILinkinpediaLinkable
{
    /// <summary>
    /// ID of this mashup
    /// </summary>
    [Key]
    public uint Id { get; set; }

    /// <summary>
    /// Name of this mashup
    /// </summary>
    public required string Title { get; set; }
    
    /// <inheritdoc/>
    public string? LinkinpediaUrl { get; set; }
    
    /// <summary>
    /// Songs that are included in this mashup
    /// </summary>
    public virtual ICollection<SongDo> Songs { get; set; }
}