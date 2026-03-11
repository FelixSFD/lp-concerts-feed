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
}