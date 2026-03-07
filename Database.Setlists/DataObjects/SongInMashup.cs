namespace Database.Setlists.DataObjects;

public class SongInMashup
{
    /// <summary>
    /// ID of the <see cref="SongDo"/> in the <see cref="MashupId"/>.
    /// </summary>
    public uint SongId { get; set; }

    /// <summary>
    /// ID of the <see cref="SongMashupDo"/>
    /// </summary>
    public uint MashupId { get; set; }
}