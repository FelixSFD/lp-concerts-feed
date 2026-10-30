namespace LPCalendar.DataStructure.Setlists.Import;

/// <summary>
/// Import a song before adding it to the setlist
/// </summary>
public class AddSongImportActionBo() : ImportActionBo(ImportActionType.AddSong)
{
    /// <summary>
    /// Title of the song
    /// </summary>
    public required string SongTitle { get; set; }
}