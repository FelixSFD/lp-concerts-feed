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
    [Column("Id")]
    public uint Id { get; set; }

    /// <summary>
    /// ID of the Concert
    /// </summary>
    [MaxLength(63)]
    [Column("ConcertId")]
    public required string ConcertId { get; set; }
    
    /// <summary>
    /// Title of the concert
    /// Example: Hamburg 2024
    /// </summary>
    [MaxLength(63)]
    [Column("ConcertTitle")]
    public required string ConcertTitle { get; set; }
    
    /// <summary>
    /// Tour Name of the concert.
    /// Is read from the concerts-database.
    /// </summary>
    [MaxLength(63)]
    [Column("ConcertTourName")]
    public string? ConcertTourName { get; set; }
    
    /// <summary>
    /// Type of the concert.
    /// Is read from the concerts-database.
    /// </summary>
    [MaxLength(63)]
    [Column("ConcertType")]
    public required string ConcertType { get; set; }

    /// <summary>
    /// Date of the concert happened (in venue's timezone)
    /// Is read from the concerts-database.
    /// </summary>
    [Column("ConcertDate")]
    public required DateTime ConcertDate { get; set; }
    
    /// <summary>
    /// Name of the set.
    /// Example: Set A
    /// </summary>
    [MaxLength(63)]
    [Column("SetName")]
    public string? SetName { get; set; }
    
    /// <inheritdoc/>
    [MaxLength(63)]
    [Column("LinkinpediaUrl")]
    public string? LinkinpediaUrl { get; set; }
    
    /// <summary>
    /// Setlist entries for this act
    /// </summary>
    public virtual ICollection<SetlistEntryDo> Entries { get; set; }
}