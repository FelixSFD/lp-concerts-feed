namespace Database.Setlists.DataObjects;

/// <summary>
/// A mashup is used when the band plays a special mix of two or more <see cref="Song"/>s.
/// Example: "Remember The Name" and "When They Come For Me" during the FROM ZERO WORLD TOUR 2024-2026.
/// </summary>
public class SongMashup
{
    /// <summary>
    /// ID of this mashup
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// Name of this mashup
    /// </summary>
    public required string Title { get; set; }
}