using Database.Setlists.DataObjects;
using Database.Tours.DataObjects;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Setlists;

namespace Service.Setlists;

/// <summary>
/// Collection of functions to map DOs and DTOs
/// </summary>
public static class DtoMapper
{
    /// <summary>
    /// Maps a <see cref="AlbumDo"/> to <see cref="AlbumBo"/>
    /// </summary>
    /// <param name="albumDo">Song to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static AlbumBo ToDto(AlbumDo albumDo)
    {
        return new AlbumBo
        {
            Id = albumDo.Id,
            Title = albumDo.Title,
            LinkinpediaUrl = albumDo.LinkinpediaUrl
        };
    }
    
    /// <summary>
    /// Maps a <see cref="AlbumDo"/> to <see cref="AlbumBo"/>
    /// </summary>
    /// <param name="albumDo">Song to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static AlbumBo? ToDtoNullable(AlbumDo? albumDo) 
        => albumDo == null ? null : ToDto(albumDo);

    /// <summary>
    /// Maps a <see cref="SongDo"/> to <see cref="SongBo"/>
    /// </summary>
    /// <param name="songDo">Song to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SongBo ToDto(SongDo songDo)
    {
        return new SongBo
        {
            Id = songDo.Id,
            Title = songDo.Title,
            Album = ToDtoNullable(songDo.Album),
            Isrc = songDo.Isrc,
            AppleMusicId = songDo.AppleMusicId,
            LinkinpediaUrl = songDo.LinkinpediaUrl
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongDo"/> to <see cref="SongBo"/>
    /// </summary>
    /// <param name="songDo">Song to map to its DTO</param>
    /// <returns>the DTO</returns>
    private static SongBo? ToDtoNullable(SongDo? songDo)
    {
        if (songDo == null)
            return null;
        
        return new SongBo
        {
            Id = songDo.Id,
            Title = songDo.Title,
            Album = ToDtoNullable(songDo.Album),
            Isrc = songDo.Isrc,
            AppleMusicId = songDo.AppleMusicId,
            LinkinpediaUrl = songDo.LinkinpediaUrl
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongVariantDo"/> to <see cref="SongVariantBo"/>
    /// </summary>
    /// <param name="songVariantDo">Song variant to map to its DTO</param>
    /// <returns>the DTO</returns>
    private static SongVariantBo? ToDtoNullable(SongVariantDo? songVariantDo)
    {
        if (songVariantDo == null)
            return null;
        
        return new SongVariantBo
        {
            Id = songVariantDo.Id,
            SongId = songVariantDo.SongId,
            VariantName = songVariantDo.VariantName,
            IsrcOverride = songVariantDo.IsrcOverride,
            AppleMusicIdOverride = songVariantDo.AppleMusicIdOverride,
            Description = songVariantDo.Description
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongVariantDo"/> to <see cref="SongVariantBo"/>
    /// </summary>
    /// <param name="songVariantDo">Song variant to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SongVariantBo ToDto(SongVariantDo songVariantDo)
    {
        return new SongVariantBo
        {
            Id = songVariantDo.Id,
            SongId = songVariantDo.SongId,
            VariantName = songVariantDo.VariantName,
            IsrcOverride = songVariantDo.IsrcOverride,
            AppleMusicIdOverride = songVariantDo.AppleMusicIdOverride,
            Description = songVariantDo.Description
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongMashupDo"/> to <see cref="SongMashupBo"/>
    /// </summary>
    /// <param name="songMashupDo">Song mashup to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SongMashupBo ToDto(SongMashupDo songMashupDo)
    {
        return new SongMashupBo
        {
            Id = songMashupDo.Id,
            Title = songMashupDo.Title,
            LinkinpediaUrl = songMashupDo.LinkinpediaUrl,
            Songs = songMashupDo
                .Songs
                .Select(s => s.Song)
                .Select(ToDto)
                .ToList()
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SongMashupDo"/> to <see cref="SongMashupBo"/>
    /// </summary>
    /// <param name="songMashupDo">Song mashup to map to its DTO</param>
    /// <returns>the DTO</returns>
    private static SongMashupBo? ToDtoNullable(SongMashupDo? songMashupDo)
    {
        return songMashupDo == null ? null : ToDto(songMashupDo);
    }
    
    /// <summary>
    /// Maps a <see cref="SetlistActDo"/> to <see cref="SetlistActBo"/>
    /// </summary>
    /// <param name="actDo">Setlist act to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SetlistActBo ToDto(SetlistActDo actDo)
    {
        return new SetlistActBo
        {
            SetlistId = actDo.SetlistId,
            ActNumber = actDo.ActNumber,
            Title = actDo.Title
        };
    }

    /// <summary>
    /// Maps a <see cref="SetlistDo"/> to <see cref="SetlistBo"/>
    /// </summary>
    /// <param name="setlistDo">Setlist to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SetlistBo ToDto(SetlistDo setlistDo)
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
        
        return new SetlistBo
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
    /// Maps a <see cref="SetlistDo"/> to <see cref="SetlistHeaderBo"/>
    /// </summary>
    /// <param name="setlistDo">Setlist to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SetlistHeaderBo ToHeaderOnlyDto(SetlistDo setlistDo)
    {
        return new SetlistHeaderBo
        {
            Id = setlistDo.Id,
            ConcertId = setlistDo.ConcertId,
            ConcertTitle = setlistDo.ConcertTitle,
            SetName = setlistDo.SetName,
            LinkinpediaUrl = setlistDo.LinkinpediaUrl
        };
    }
    
    /// <summary>
    /// Maps a <see cref="SetlistEntryDo"/> to <see cref="SetlistEntryBo"/>
    /// </summary>
    /// <param name="setlistEntryDo">Setlist entry to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static SetlistEntryBo ToDto(SetlistEntryDo setlistEntryDo)
    {
        return new SetlistEntryBo
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
            Isrc = GetIsrcForSongVariant(setlistEntryDo.PlayedSong, setlistEntryDo.PlayedSongVariant),
            AppleMusicId = setlistEntryDo.PlayedSongVariant?.AppleMusicIdOverride ?? setlistEntryDo.PlayedSongVariant?.Song.AppleMusicId ?? setlistEntryDo.PlayedSong?.AppleMusicId,
            LinkinpediaUrl = setlistEntryDo.PlayedSong?.LinkinpediaUrl ?? setlistEntryDo.PlayedSongVariant?.Song.LinkinpediaUrl ?? setlistEntryDo.PlayedMashup?.LinkinpediaUrl,
            IsPlayedFromRecording = setlistEntryDo.IsPlayedFromRecording,
            IsRotationSong = setlistEntryDo.IsRotationSong,
            IsWorldPremiere = setlistEntryDo.IsWorldPremiere,
            IsLivePremiere = setlistEntryDo.IsLivePremiere,
            SongExtras = setlistEntryDo.SongExtras?.Select(ToDto).ToList() ?? [],
        };
    }
    
    private static string? GetEntryTitleForSongVariant(SongDo? songDo, SongVariantDo? songVariantDo)
    {
        if (songDo == null || songVariantDo == null)
            return null;

        return $"{songDo.Title} ({songVariantDo.VariantName})";
    }


    private static string? GetIsrcForSongVariant(SongDo? songDo, SongVariantDo? songVariantDo)
    {
        return songVariantDo?.IsrcOverride ?? songDo?.Isrc;
    }
    
    /// <summary>
    /// Maps a <see cref="SetlistEntryDo"/> to <see cref="SetlistEntryBo"/>
    /// </summary>
    /// <param name="setlistEntryDo">Setlist entry to map to its DTO</param>
    /// <returns>the DTO</returns>
    public static RawSetlistEntryBo ToRawDto(SetlistEntryDo setlistEntryDo)
    {
        return new RawSetlistEntryBo
        {
            Id = setlistEntryDo.Id,
            ActNumber = setlistEntryDo.ActNumber,
            SongNumber = setlistEntryDo.SongNumber,
            SortNumber = setlistEntryDo.SortNumber,
            PlayedSong = ToDtoNullable(setlistEntryDo.PlayedSong),
            PlayedSongVariant = ToDtoNullable(setlistEntryDo.PlayedSongVariant),
            PlayedSongMashup = ToDtoNullable(setlistEntryDo.PlayedMashup),
            TitleOverride = setlistEntryDo.TitleOverride,
            ExtraNotes = setlistEntryDo.ExtraNotes,
            LinkinpediaUrl = setlistEntryDo.PlayedSong?.LinkinpediaUrl ?? setlistEntryDo.PlayedSongVariant?.Song.LinkinpediaUrl ?? setlistEntryDo.PlayedMashup?.LinkinpediaUrl,
            IsPlayedFromRecording = setlistEntryDo.IsPlayedFromRecording,
            IsRotationSong = setlistEntryDo.IsRotationSong,
            IsWorldPremiere = setlistEntryDo.IsWorldPremiere,
            IsLivePremiere = setlistEntryDo.IsLivePremiere,
            SongExtras = setlistEntryDo.SongExtras?.Select(ToDto).ToList() ?? []
        };
    }

    public static SetlistEntrySongExtraDo.ExtraType FromDto(SetlistEntrySongExtraBo.ExtraType dto)
    {
        return dto switch
        {
            SetlistEntrySongExtraBo.ExtraType.ExtendedBridge => SetlistEntrySongExtraDo.ExtraType.ExtendedBridge,
            SetlistEntrySongExtraBo.ExtraType.ExtraVerse => SetlistEntrySongExtraDo.ExtraType.ExtraVerse,
            _ => throw new NotImplementedException()
        };
    }
    
    public static SetlistEntrySongExtraBo.ExtraType ToDto(SetlistEntrySongExtraDo.ExtraType dto)
    {
        return dto switch
        {
            SetlistEntrySongExtraDo.ExtraType.ExtendedBridge => SetlistEntrySongExtraBo.ExtraType.ExtendedBridge,
            SetlistEntrySongExtraDo.ExtraType.ExtraVerse => SetlistEntrySongExtraBo.ExtraType.ExtraVerse,
            _ => throw new NotImplementedException()
        };
    }
    
    /// <summary>
    /// Converts a <see cref="SetlistEntrySongExtraDo"/> to its DTO
    /// </summary>
    /// <param name="songExtraDo"></param>
    /// <returns></returns>
    public static SetlistEntrySongExtraBo ToDto(SetlistEntrySongExtraDo songExtraDo)
    {
        return new SetlistEntrySongExtraBo
        {
            Id = songExtraDo.Id,
            Type = ToDto(songExtraDo.Type),
            Song = ToDtoNullable(songExtraDo.Song),
            Description = songExtraDo.Description,
        };
    }

    /// <summary>
    /// Maps a <see cref="ConcertDo"/> to <see cref="ConcertDto"/>
    /// </summary>
    /// <param name="concert"></param>
    /// <returns></returns>
    public static ConcertDto ToDto(ConcertDo concert)
    {
        return new ConcertDto
        {
            Id = concert.Id,
            ShowType = concert.Type?.Name ?? string.Empty,
            CustomTitle = concert.CustomTitle,
            TourName = concert.Tour?.Name,
            Status = concert.Status.ToString(),
            ConcertStatus = concert.Status switch
            {
                ConcertDo.ConcertStatus.Planned => ConcertDto.ConcertStatusValue.Planned,
                ConcertDo.ConcertStatus.LiveRightNow => ConcertDto.ConcertStatusValue.Running,
                ConcertDo.ConcertStatus.Past => ConcertDto.ConcertStatusValue.Past,
                ConcertDo.ConcertStatus.Cancelled => ConcertDto.ConcertStatusValue.Cancelled,
                _ => ConcertDto.ConcertStatusValue.Past
            },
            PostedStartTime = concert.PostedStartTime,
            LpuEarlyEntryConfirmed = concert.LpuEarlyEntryConfirmed,
            LpuEarlyEntryTime = concert.LpuEarlyEntryTime,
            DoorsTime = concert.DoorsTime,
            MainStageTime = concert.MainStageTime,
            ExpectedSetDuration = (short?)concert.ExpectedSetDurationMinutes,
            Venue = concert.Venue?.CurrentName,
            City = concert.Venue?.City?.Name ?? string.Empty,
            State = concert.Venue?.State?.Name,
            Country = concert.Venue?.Country?.Name ?? string.Empty,
            TimeZoneId = concert.Venue?.TimeZone ?? string.Empty,
            VenueLatitude = concert.Venue?.Latitude ?? 0,
            VenueLongitude = concert.Venue?.Longitude ?? 0,
            ScheduleImageFile = concert.ScheduleImageFile,
            DeletedAt = concert.DeletedAt
        };
    }
}