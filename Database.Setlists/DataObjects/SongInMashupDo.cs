using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Setlists.DataObjects;

public class SongInMashupDo
{
    /// <summary>
    /// ID of the <see cref="SongDo"/> in the <see cref="MashupId"/>.
    /// </summary>
    [Column("SongId")]
    public uint SongId { get; set; }

    /// <summary>
    /// ID of the <see cref="SongMashupDo"/>
    /// </summary>
    [Column("MashupId")]
    public uint MashupId { get; set; }
}