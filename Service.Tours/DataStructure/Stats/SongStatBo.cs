namespace Service.Tours.DataStructure.Stats;

/// <summary>Number of live setlist entries for a song in the selected scope.</summary>
/// <param name="SongId">Stable song identifier.</param>
/// <param name="SongName">Song title.</param>
/// <param name="AlbumId">Stable album identifier, if assigned.</param>
/// <param name="AlbumName">Album title, if assigned.</param>
/// <param name="AppleMusicId">Optional Apple Music identifier.</param>
/// <param name="PlayCount">Number of live setlist entries, including repetitions.</param>
public sealed record SongStatBo(uint SongId, string SongName, uint? AlbumId, string? AlbumName, string? AppleMusicId, uint PlayCount);

