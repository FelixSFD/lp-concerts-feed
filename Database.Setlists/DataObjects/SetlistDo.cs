using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

[Table("Setlist")]
public class SetlistDo : BaseDo, ILinkinpediaLinkable
{
    /// <summary>
    /// Unique ID of the setlist
    /// </summary>
    [Key]
    public uint Id { get; set; }

    /// <summary>
    /// ID of the Concert
    /// </summary>
    [MaxLength(63)]
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// Title for the setlist
    /// </summary>
    [MaxLength(63)]
    public required string Title { get; set; }
    
    /// <inheritdoc/>
    [MaxLength(63)]
    public string? LinkinpediaUrl { get; set; }
    
    /// <summary>
    /// Setlist entries for this act
    /// </summary>
    public virtual ICollection<SetlistEntryDo> Entries { get; set; }
}