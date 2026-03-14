using Database.Setlists.DataObjects;
using LPCalendar.DataStructure.Setlists;

namespace Service.Setlists;

/// <summary>
/// Collection of functions to map DOs and DTOs
/// </summary>
public static class DtoMapper
{
    /// <summary>
    /// Maps a <see cref="SongDo"/> to <see cref="SongDto"/>
    /// </summary>
    /// <param name="songDo">Song to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SongDto ToDto(SongDo songDo)
    {
        return new SongDto
        {
            Id = songDo.Id,
            Title = songDo.Title,
            Isrc = songDo.Isrc
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongVariantDo"/> to <see cref="SongVariantDto"/>
    /// </summary>
    /// <param name="songVariantDo">Song variant to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SongVariantDto? ToDtoNullable(SongVariantDo? songVariantDo)
    {
        if (songVariantDo == null)
            return null;
        
        return new SongVariantDto
        {
            Id = songVariantDo.Id,
            SongId = songVariantDo.SongId,
            VariantName = songVariantDo.VariantName,
            IsrcOverride = songVariantDo.IsrcOverride,
            Description = songVariantDo.Description
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongVariantDo"/> to <see cref="SongVariantDto"/>
    /// </summary>
    /// <param name="songVariantDo">Song variant to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SongVariantDto ToDto(SongVariantDo songVariantDo)
    {
        return new SongVariantDto
        {
            Id = songVariantDo.Id,
            SongId = songVariantDo.SongId,
            VariantName = songVariantDo.VariantName,
            IsrcOverride = songVariantDo.IsrcOverride,
            Description = songVariantDo.Description
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongMashupDo"/> to <see cref="SongMashupDto"/>
    /// </summary>
    /// <param name="songMashupDo">Song mashup to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SongMashupDto ToDto(SongMashupDo songMashupDo)
    {
        return new SongMashupDto
        {
            Id = songMashupDo.Id,
            Title = songMashupDo.Title,
            LinkinpediaUrl = songMashupDo.LinkinpediaUrl,
            Songs = songMashupDo.Songs.Select(ToDto).ToList()
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongMashupDo"/> to <see cref="SongMashupDto"/>
    /// </summary>
    /// <param name="songMashupDo">Song mashup to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SongMashupDto? ToDtoNullable(SongMashupDo? songMashupDo)
    {
        return songMashupDo == null ? null : ToDto(songMashupDo);
    }
    
    /// <summary>
    /// Maps a <see cref="SetlistActDo"/> to <see cref="SetlistActDto"/>
    /// </summary>
    /// <param name="actDo">Setlist act to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SetlistActDto ToDto(SetlistActDo actDo)
    {
        return new SetlistActDto
        {
            SetlistId = actDo.SetlistId,
            ActNumber = actDo.ActNumber,
            Title = actDo.Title
        };
    }

    /// <summary>
    /// Maps a <see cref="SetlistDo"/> to <see cref="SetlistDto"/>
    /// </summary>
    /// <param name="setlistDo">Setlist to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SetlistDto ToDto(SetlistDo setlistDo)
    {
        var entries = setlistDo.Entries?
            .OrderBy(se => se.SortNumber)
            .ToList() ?? [];

        var acts = entries
            .Where(entry => entry.Act != null)
            .Select(entry => entry.Act!)
            .DistinctBy(entry => entry.ActNumber)
            .OrderBy(entry => entry.ActNumber)
            .Select(ToDto)
            .ToList();
        
        return new SetlistDto
        {
            Id = setlistDo.Id,
            ConcertId = setlistDo.ConcertId,
            ConcertTitle = setlistDo.ConcertTitle,
            SetName = setlistDo.SetName,
            LinkinpediaUrl = setlistDo.LinkinpediaUrl,
            Entries = entries.Select(ToDto).ToList(),
            Acts = acts
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SetlistDo"/> to <see cref="SetlistHeaderDto"/>
    /// </summary>
    /// <param name="setlistDo">Setlist to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SetlistHeaderDto ToHeaderOnlyDto(SetlistDo setlistDo)
    {
        return new SetlistHeaderDto
        {
            Id = setlistDo.Id,
            ConcertId = setlistDo.ConcertId,
            ConcertTitle = setlistDo.ConcertTitle,
            SetName = setlistDo.SetName,
            LinkinpediaUrl = setlistDo.LinkinpediaUrl
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SetlistEntryDo"/> to <see cref="SetlistEntryDto"/>
    /// </summary>
    /// <param name="setlistEntryDo">Setlist entry to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SetlistEntryDto ToDto(SetlistEntryDo setlistEntryDo)
    {
        return new SetlistEntryDto
        {
            Id = setlistEntryDo.Id,
            ActNumber = setlistEntryDo.ActNumber,
            SongNumber = setlistEntryDo.SongNumber,
            SortNumber = setlistEntryDo.SortNumber,
            PlayedSong = setlistEntryDo.PlayedSong != null ? ToDto(setlistEntryDo.PlayedSong) : null,
            PlayedSongVariant = ToDtoNullable(setlistEntryDo.PlayedSongVariant),
            PlayedSongMashup = ToDtoNullable(setlistEntryDo.PlayedMashup),
            Title = setlistEntryDo.TitleOverride ?? GetEntryTitleForSongVariant(setlistEntryDo.PlayedSongVariant?.Song, setlistEntryDo.PlayedSongVariant) ?? setlistEntryDo.PlayedSong?.Title ?? setlistEntryDo.PlayedMashup?.Title ?? "unknown",
            ExtraNotes = setlistEntryDo.ExtraNotes,
            IsPlayedFromRecording = setlistEntryDo.IsPlayedFromRecording,
            IsRotationSong = setlistEntryDo.IsRotationSong,
            IsWorldPremiere = setlistEntryDo.IsWorldPremiere
        };
    }
    
    private static string? GetEntryTitleForSongVariant(SongDo? songDo, SongVariantDo? songVariantDo)
    {
        if (songDo == null || songVariantDo == null)
            return null;

        return $"{songDo.Title} ({songVariantDo.VariantName})";
    }
}