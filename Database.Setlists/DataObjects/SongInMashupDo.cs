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
    /// Song in the mashup
    /// </summary>
    public SongDo Song { get; set; }

    /// <summary>
    /// ID of the <see cref="SongMashupDo"/>
    /// </summary>
    [Column("MashupId")]
    public uint MashupId { get; set; }
    
    /// <summary>
    /// Mashup that contains the song
    /// </summary>
    public SongMashupDo Mashup { get; set; }
}