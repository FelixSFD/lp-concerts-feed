namespace Database.Setlists.DataObjects;

public class SongInMashup
{
    /// <summary>
    /// ID of the <see cref="Song"/> in the <see cref="MashupId"/>.
    /// </summary>
    public uint SongId { get; set; }

    /// <summary>
    /// ID of the <see cref="SongMashup"/>
    /// </summary>
    public uint MashupId { get; set; }
}