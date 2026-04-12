using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Database.Setlists.DataObjects;

[Table("SetlistAct")]
[PrimaryKey(nameof(SetlistId), nameof(ActNumber))]
public class SetlistActDo : BaseDo
{
    /// <summary>
    /// ID of the setlist
    /// </summary>
    [Column("SetlistId")]
    public uint SetlistId { get; set; }
    
    
    /// <summary>
    /// Unique ID of the setlist
    /// </summary>
    [Column("ActNumber")]
    public uint ActNumber { get; set; }


    /// <summary>
    /// Title of this act
    /// </summary>
    [MaxLength(31)]
    [Column("Title")]
    public string? Title { get; set; }
    
    
    /// <summary>
    /// Setlist that contains this act
    /// </summary>
    public virtual SetlistDo Setlist { get; set; }

    /// <summary>
    /// Setlist entries for this act
    /// </summary>
    public virtual ICollection<SetlistEntryDo> Entries { get; set; }
}