using Common.Contracts.Generated.Models;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Setlists;
using LPCalendar.DataStructure.Setlists.Import;
using LPCalendar.DataStructure.Setlists.Parameters;
using LPCalendar.DataStructure.Tours;
using LPCalendar.DataStructure.Tours.Locations;
using Service.Tours.DataStructure;
using Service.Users.DataStructure;

namespace Server.Api;

internal static class DtoMapper
{
    #region ConcertType
    
    /// <summary>
    /// Maps the DTO to the BusinessObject
    /// </summary>
    /// <param name="dto">DTO to map</param>
    /// <returns>the mapped Business Object</returns>
    public static CreateConcertTypeRequest ToBo(this CreateConcertTypeRequestDto dto)
    {
        return new CreateConcertTypeRequest
        {
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Maps the DTO to the BusinessObject
    /// </summary>
    /// <param name="dto">DTO to map</param>
    /// <returns>the mapped Business Object</returns>
    public static UpdateConcertTypeRequest ToBo(this UpdateConcertTypeRequestDto dto)
    {
        return new UpdateConcertTypeRequest
        {
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static ConcertTypeDto ToDto(this ConcertTypeBo bo)
    {
        return new ConcertTypeDto
        {
            Id = (int)bo.Id,
            Name = bo.Name,
        };
    }
    
    #endregion

    #region Tours

    /// <summary>
    /// Maps the DTO to the BusinessObject
    /// </summary>
    /// <param name="dto">DTO to map</param>
    /// <returns>the mapped Business Object</returns>
    public static CreateTourRequest ToBo(this CreateTourRequestDto dto)
    {
        return new CreateTourRequest
        {
            Id = dto.Id,
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Maps the DTO to the BusinessObject
    /// </summary>
    /// <param name="dto">DTO to map</param>
    /// <returns>the mapped Business Object</returns>
    public static AddTourLegRequest ToBo(this AddTourLegRequestDto dto)
    {
        return new AddTourLegRequest
        {
            Id = dto.Id,
            Name = dto.Name,
        };
    }
    
    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static TourDto ToDto(this TourBo bo)
    {
        return new TourDto
        {
            Id = bo.Id,
            Name = bo.Name,
            Legs = [.. bo.Legs.Select(ToDto)],
        };
    }
    
    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static TourLegDto ToDto(this TourLegBo bo)
    {
        return new TourLegDto
        {
            TourId = bo.TourId,
            Id = bo.Id,
            Name = bo.Name,
        };
    }

    #endregion

    #region Locations

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static CountryDto ToDto(this CountryBo bo)
    {
        return new CountryDto
        {
            IsoCode = bo.IsoCode,
            Name = bo.Name,
            NativeName = bo.NativeName,
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static StateDto ToDto(this StateBo bo)
    {
        return new StateDto
        {
            CountryCode = bo.CountryCode,
            Code = bo.Code,
            Name = bo.Name,
            NativeName = bo.NativeName,
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static StateWithCountryDto ToDto(this StateWithCountryBo bo)
    {
        return new StateWithCountryDto
        {
            CountryCode = bo.CountryCode,
            Code = bo.Code,
            Name = bo.Name,
            NativeName = bo.NativeName,
            Country = bo.Country.ToDto(),
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static CityWithCountryDto ToDto(this CityWithCountryBo bo)
    {
        return new CityWithCountryDto
        {
            Id = (int)bo.Id,
            CountryCode = bo.CountryCode,
            StateCode = bo.StateCode,
            Name = bo.Name,
            NativeName = bo.NativeName,
            Country = bo.Country.ToDto(),
            State = bo.State?.ToDto(),
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static VenueDto ToDto(this VenueBo bo)
    {
        return new VenueDto
        {
            Id = bo.Id,
            CountryCode = bo.CountryCode,
            StateCode = bo.StateCode,
            CityId = bo.CityId,
            CurrentName = bo.CurrentName,
            TimeZoneId = bo.TimeZone,
            Latitude = bo.Latitude,
            Longitude = bo.Longitude,
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static VenueWithCityDto ToDto(this VenueWithCityBo bo)
    {
        return new VenueWithCityDto
        {
            Id = bo.Id,
            CountryCode = bo.CountryCode,
            StateCode = bo.StateCode,
            CityId = bo.CityId,
            CurrentName = bo.CurrentName,
            TimeZoneId = bo.TimeZone,
            Latitude = bo.Latitude,
            Longitude = bo.Longitude,
            City = bo.City.ToDto(),
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static VenueWithDetailsDto ToDto(this VenueWithDetailsBo bo)
    {
        return new VenueWithDetailsDto
        {
            Id = bo.Id,
            CountryCode = bo.CountryCode,
            StateCode = bo.StateCode,
            CityId = bo.CityId,
            CurrentName = bo.CurrentName,
            TimeZoneId = bo.TimeZone,
            Latitude = bo.Latitude,
            Longitude = bo.Longitude,
            City = bo.City.ToDto(),
            VenueNames = [.. bo.VenueNames.Select(ToDto)],
        };
    }
    
    public static CreateVenueRequestBo ToBo(this CreateVenueRequestDto dto)
    {
        return new CreateVenueRequestBo
        {
            CountryCode = dto.CountryCode,
            StateCode = dto.StateCode,
            CityId = (uint)dto.CityId,
            CurrentName = dto.CurrentName,
            TimeZone = dto.TimeZoneId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
        };
    }

    public static UpdateVenueRequestBo ToBo(this UpdateVenueRequestDto dto)
    {
        return new UpdateVenueRequestBo
        {
            CountryCode = dto.CountryCode,
            StateCode = dto.StateCode,
            CityId = (uint)dto.CityId,
            TimeZone = dto.TimeZoneId,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static PreviousVenueNameDto ToDto(this PreviousVenueNameBo bo)
    {
        return new PreviousVenueNameDto
        {
            Id = bo.Id,
            VenueId = bo.VenueId,
            Name = bo.Name,
            UsedFrom = bo.UsedFrom,
            UsedUntil = bo.UsedUntil,
        };
    }

    #endregion

    #region Concerts
    
    public static CreateConcertRequestBo ToBo(this CreateConcertRequestDto dto)
    {
        return new CreateConcertRequestBo
        {
            CustomTitle = dto.CustomTitle,
            PostedStartTime = dto.PostedStartTime ?? throw new ArgumentNullException(nameof(dto.PostedStartTime)),
            TimeIsPlaceholder = dto.TimeIsPlaceholder ?? false,
            MainStageTime = dto.MainStageTime?.UtcDateTime,
            DoorsTime = dto.DoorsTime?.UtcDateTime,
            LpuEarlyEntryTime = dto.LpuEarlyEntryTime?.UtcDateTime,
            LpuEarlyEntryConfirmed = dto.LpuEarlyEntryConfirmed ?? false,
            ExpectedSetDurationMinutes = (uint)(dto.ExpectedSetDurationMinutes ?? 0),
            ScheduleImageFile = dto.ScheduleImageFile,
            Status = dto.Status.ToBo(),
            ConcertTypeId = (uint)(dto.ConcertTypeId ?? 0),
            VenueId = (uint)(dto.VenueId ?? 0),
            TourId = dto.TourId,
            TourLegId = dto.TourLegId,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }
    
    public static UpdateConcertRequestBo ToBo(this UpdateConcertRequestDto dto)
    {
        return new UpdateConcertRequestBo
        {
            CustomTitle = dto.CustomTitle,
            PostedStartTime = dto.PostedStartTime ?? throw new ArgumentNullException(nameof(dto.PostedStartTime)),
            TimeIsPlaceholder = dto.TimeIsPlaceholder ?? false,
            MainStageTime = dto.MainStageTime?.UtcDateTime,
            DoorsTime = dto.DoorsTime?.UtcDateTime,
            LpuEarlyEntryTime = dto.LpuEarlyEntryTime?.UtcDateTime,
            LpuEarlyEntryConfirmed = dto.LpuEarlyEntryConfirmed ?? false,
            ExpectedSetDurationMinutes = (uint)(dto.ExpectedSetDurationMinutes ?? 0),
            Status = dto.Status.ToBo(),
            ConcertTypeId = (uint)(dto.ConcertTypeId ?? 0),
            VenueId = (uint)(dto.VenueId ?? 0),
            TourId = dto.TourId,
            TourLegId = dto.TourLegId,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static ConcertDetailsDto ToDto(this ConcertDetailsBo bo)
    {
        return new ConcertDetailsDto
        {
            Id = bo.Id,
            ConcertType = bo.ConcertType.ToDto(),
            Tour = bo.Tour?.ToDto(),
            TourLeg = bo.TourLeg?.ToDto(),
            CustomTitle = bo.CustomTitle,
            Venue = bo.Venue.ToDto(),
            PostedStartTime = bo.PostedStartTime,
            TimeIsPlaceholder = bo.TimeIsPlaceholder,
            MainStageTime = bo.MainStageTime,
            DoorsTime = bo.DoorsTime,
            LpuEarlyEntryTime = bo.LpuEarlyEntryTime,
            LpuEarlyEntryConfirmed = bo.LpuEarlyEntryConfirmed,
            ExpectedSetDurationMinutes = bo.ExpectedSetDurationMinutes.ToString(),
            ScheduleImageFile = bo.ScheduleImageFile,
            DeletedAt = bo.DeletedAt,
            Status = bo.Status.ToDto(),
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }
    
    public static ConcertStatusValueDto ToDto(this ConcertDto.ConcertStatusValue data)
    {
        return data switch
        {
            ConcertDto.ConcertStatusValue.Planned => ConcertStatusValueDto.Planned,
            ConcertDto.ConcertStatusValue.Running => ConcertStatusValueDto.Running,
            ConcertDto.ConcertStatusValue.Past => ConcertStatusValueDto.Past,
            ConcertDto.ConcertStatusValue.Cancelled => ConcertStatusValueDto.Cancelled,
            _ => ConcertStatusValueDto.Past
        };
    }
    
    public static ConcertDto.ConcertStatusValue ToBo(this ConcertStatusValueDto data)
    {
        return data switch
        {
            ConcertStatusValueDto.Planned => ConcertDto.ConcertStatusValue.Planned,
            ConcertStatusValueDto.Running => ConcertDto.ConcertStatusValue.Running,
            ConcertStatusValueDto.Past => ConcertDto.ConcertStatusValue.Past,
            ConcertStatusValueDto.Cancelled => ConcertDto.ConcertStatusValue.Cancelled,
            _ => ConcertDto.ConcertStatusValue.Past
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static ImportConcertPreviewDto ToDto(this ImportConcertPreviewBo bo)
    {
        return new ImportConcertPreviewDto
        {
            ConcertStatus = bo.ConcertStatus?.ToDto() ?? ConcertStatusValueDto.Planned,
            ConcertType = bo.ConcertType?.ToDto(),
            PostedStartTime = bo.PostedStartTime,
            FoundCities = [.. bo.FoundCities.Select(ToDto)],
            FoundVenues = [.. bo.FoundVenues.Select(ToDto)],
            FoundCountries = [.. bo.FoundCountries.Select(ToDto)],
            FoundStates = [.. bo.FoundStates.Select(ToDto)],
            CountryName = bo.CountryName,
            StateName = bo.StateName,
            CityName = bo.CityName,
            CityNativeName = bo.CityNativeName,
            VenueName = bo.VenueName,
            FoundTours = [.. bo.FoundTours.Select(ToDto)],
            FoundTourLegs = [.. bo.FoundTourLegs.Select(ToDto)],
            TourName = bo.TourName,
            TourLegName = bo.TourLegName,
            ProposedCustomTitle = bo.ProposedCustomTitle,
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static LinkinpediaImportConcertStatusDto ToDto(this ConcertImportStatusBo bo)
    {
        return new LinkinpediaImportConcertStatusDto
        {
            WikiPageId = bo.WikiPageId,
            ConcertTitle = bo.ConcertTitle,
            ImportStatus = bo.ImportStatus.ToDto(),
            Concert = bo.Concert?.ToDto()
        };
    }

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static LinkinpediaImportConcertStatusDto.ImportStatusEnum ToDto(this ConcertImportStatusBo.Status bo)
    {
        return bo switch
        {
            ConcertImportStatusBo.Status.Imported => LinkinpediaImportConcertStatusDto.ImportStatusEnum.ImportedNoSetlist,
            ConcertImportStatusBo.Status.NotImported => LinkinpediaImportConcertStatusDto.ImportStatusEnum.NotImported,
            ConcertImportStatusBo.Status.ImportedWithSetlists => LinkinpediaImportConcertStatusDto.ImportStatusEnum.Imported,
            _ => throw new ArgumentOutOfRangeException(nameof(bo), bo, null)
        };
    }

    #endregion

    #region Users

    /// <summary>
    /// Maps the BusinessObject to a DTO
    /// </summary>
    /// <param name="bo">BusinessObject to map</param>
    /// <returns>the mapped DTO</returns>
    public static UserDto ToDto(this UserBo bo)
    {
        return new UserDto
        {
            Id = bo.Id,
            Username = bo.Username,
            OriginCountryCode = bo.OriginCountryCode,
            CreatedAt = bo.CreatedAt,
            UpdatedAt = bo.UpdatedAt,
        };
    }

    #endregion

    #region Concert Bookmarks

    /// <summary>
    /// Maps the DTO to a BusinessObject
    /// </summary>
    /// <param name="dto">DTO to map</param>
    /// <returns>the mapped BO</returns>
    public static ConcertBookmark.BookmarkStatus ToBo(this ConcertBookmarkUpdateRequestDto.StatusEnum dto)
    {
        return dto switch
        {
            ConcertBookmarkUpdateRequestDto.StatusEnum.Bookmarked => ConcertBookmark.BookmarkStatus.Bookmarked,
            ConcertBookmarkUpdateRequestDto.StatusEnum.Attending => ConcertBookmark.BookmarkStatus.Attending,
            ConcertBookmarkUpdateRequestDto.StatusEnum.None => ConcertBookmark.BookmarkStatus.None,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, null)
        };
    }
    
    /// <summary>
    /// Maps the DTO to a BusinessObject
    /// </summary>
    /// <param name="bo">object to map</param>
    /// <returns>the mapped BO</returns>
    public static GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum ToDto(this ConcertBookmark.BookmarkStatus bo)
    {
        return bo switch
        {
            ConcertBookmark.BookmarkStatus.Bookmarked => GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.Bookmarked,
            ConcertBookmark.BookmarkStatus.Attending => GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.Attending,
            ConcertBookmark.BookmarkStatus.None => GetConcertBookmarkCountsResponseDto.CurrentUserStatusEnum.None,
            _ => throw new ArgumentOutOfRangeException(nameof(bo), bo, null)
        };
    }

    #endregion

    #region Setlists

    // Album
    public static AlbumBo ToBo(this AlbumDto dto)
    {
        return new AlbumBo
        {
            Id = (uint)(dto.Id ?? 0),
            Title = dto.Title ?? string.Empty,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static AlbumDto ToDto(this AlbumBo bo)
    {
        return new AlbumDto
        {
            Id = bo.Id,
            Title = bo.Title,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static CreateAlbumRequestBo ToBo(this CreateAlbumRequestDto dto)
    {
        return new CreateAlbumRequestBo
        {
            Title = dto.Title ?? string.Empty,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static CreateAlbumRequestDto ToDto(this CreateAlbumRequestBo bo)
    {
        return new CreateAlbumRequestDto
        {
            Title = bo.Title,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static UpdateAlbumRequestBo ToBo(this UpdateAlbumRequestDto dto)
    {
        return new UpdateAlbumRequestBo
        {
            Title = dto.Title ?? string.Empty,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static UpdateAlbumRequestDto ToDto(this UpdateAlbumRequestBo bo)
    {
        return new UpdateAlbumRequestDto
        {
            Title = bo.Title,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    // Songs
    public static SongBo ToBo(this SongDto dto)
    {
        return new SongBo
        {
            Id = (uint)(dto.Id ?? 0),
            Title = dto.Title,
            Album = dto.Album?.ToBo(),
            Isrc = dto.Isrc,
            AppleMusicId = dto.AppleMusicId,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static SongDto ToDto(this SongBo bo)
    {
        return new SongDto
        {
            Id = bo.Id,
            Title = bo.Title,
            Album = bo.Album?.ToDto(),
            Isrc = bo.Isrc,
            AppleMusicId = bo.AppleMusicId,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static CreateSongRequestBo ToBo(this CreateSongRequestDto dto)
    {
        return new CreateSongRequestBo
        {
            Title = dto.Title ?? string.Empty,
            AlbumId = (uint?)dto.AlbumId,
            Isrc = dto.Isrc,
            AppleMusicId = dto.AppleMusicId,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static CreateSongRequestDto ToDto(this CreateSongRequestBo bo)
    {
        return new CreateSongRequestDto
        {
            Title = bo.Title,
            AlbumId = bo.AlbumId,
            Isrc = bo.Isrc,
            AppleMusicId = bo.AppleMusicId,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static UpdateSongRequestBo ToBo(this UpdateSongRequestDto dto)
    {
        return new UpdateSongRequestBo
        {
            Title = dto.Title ?? string.Empty,
            AlbumId = (uint?)dto.AlbumId,
            Isrc = dto.Isrc,
            AppleMusicId = dto.AppleMusicId,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static UpdateSongRequestDto ToDto(this UpdateSongRequestBo bo)
    {
        return new UpdateSongRequestDto
        {
            Title = bo.Title,
            AlbumId = bo.AlbumId,
            Isrc = bo.Isrc,
            AppleMusicId = bo.AppleMusicId,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static SongVariantBo ToBo(this SongVariantDto dto)
    {
        return new SongVariantBo
        {
            Id = (uint)(dto.Id ?? 0),
            SongId = (uint)(dto.SongId ?? 0),
            IsrcOverride = dto.IsrcOverride,
            AppleMusicIdOverride = dto.AppleMusicIdOverride,
            VariantName = dto.VariantName,
            Description = dto.Description,
        };
    }

    public static SongVariantDto ToDto(this SongVariantBo bo)
    {
        return new SongVariantDto
        {
            Id = bo.Id,
            SongId = bo.SongId,
            IsrcOverride = bo.IsrcOverride,
            AppleMusicIdOverride = bo.AppleMusicIdOverride,
            VariantName = bo.VariantName,
            Description = bo.Description,
        };
    }

    public static SongMashupBo ToBo(this SongMashupDto dto)
    {
        return new SongMashupBo
        {
            Id = (uint)(dto.Id ?? 0),
            Title = dto.Title ?? string.Empty,
            LinkinpediaUrl = dto.LinkinpediaUrl,
            Songs = dto.Songs?.Select(ToBo).ToList() ?? [],
        };
    }

    public static SongMashupDto ToDto(this SongMashupBo bo)
    {
        return new SongMashupDto
        {
            Id = bo.Id,
            Title = bo.Title,
            LinkinpediaUrl = bo.LinkinpediaUrl,
            Songs = bo.Songs.Select(ToDto).ToList(),
        };
    }

    public static CreateSongMashupRequestBo ToBo(this CreateSongMashupRequestDto dto)
    {
        return new CreateSongMashupRequestBo
        {
            Title = dto.Title ?? string.Empty,
            LinkinpediaUrl = dto.LinkinpediaUrl,
            SongIds = dto.SongIds?.Select(id => (uint)id).ToArray() ?? [],
        };
    }

    public static CreateSongMashupRequestDto ToDto(this CreateSongMashupRequestBo bo)
    {
        return new CreateSongMashupRequestDto
        {
            Title = bo.Title,
            LinkinpediaUrl = bo.LinkinpediaUrl,
            SongIds = bo.SongIds.Select(id => (decimal)id).ToList(),
        };
    }

    public static UpdateSongMashupRequestBo ToBo(this UpdateSongMashupRequestDto dto)
    {
        return new UpdateSongMashupRequestBo
        {
            Title = dto.Title ?? string.Empty,
            LinkinpediaUrl = dto.LinkinpediaUrl,
            SongIds = dto.SongIds?.Select(id => (uint)id).ToArray() ?? [],
        };
    }

    public static UpdateSongMashupRequestDto ToDto(this UpdateSongMashupRequestBo bo)
    {
        return new UpdateSongMashupRequestDto
        {
            Title = bo.Title,
            LinkinpediaUrl = bo.LinkinpediaUrl,
            SongIds = bo.SongIds.Select(id => (decimal)id).ToList(),
        };
    }

    // Parameters
    public static ActParametersBo ToBo(this ActParametersDto dto)
    {
        return new ActParametersBo
        {
            SetlistId = (uint?)dto.SetlistId,
            ActNumber = (uint)(dto.ActNumber ?? 0),
            Title = dto.Title,
        };
    }

    public static ActParametersDto ToDto(this ActParametersBo bo)
    {
        return new ActParametersDto
        {
            SetlistId = bo.SetlistId,
            ActNumber = bo.ActNumber,
            Title = bo.Title,
        };
    }

    public static SetlistEntryParametersBo ToBo(this SetlistEntryParametersDto dto)
    {
        return new SetlistEntryParametersBo
        {
            SortNumber = (uint)(dto.SortNumber ?? 0),
            SongNumber = (ushort)(dto.SongNumber ?? 0),
            TitleOverride = dto.TitleOverride,
            ExtraNotes = dto.ExtraNotes,
            IsRotationSong = dto.IsRotationSong ?? false,
            IsPlayedFromRecording = dto.IsPlayedFromRecording ?? false,
            IsWorldPremiere = dto.IsWorldPremiere ?? false,
            IsLivePremiere = dto.IsLivePremiere ?? false,
        };
    }

    public static SetlistEntryParametersDto ToDto(this SetlistEntryParametersBo bo)
    {
        return new SetlistEntryParametersDto
        {
            SortNumber = bo.SortNumber,
            SongNumber = bo.SongNumber,
            TitleOverride = bo.TitleOverride,
            ExtraNotes = bo.ExtraNotes,
            IsRotationSong = bo.IsRotationSong,
            IsPlayedFromRecording = bo.IsPlayedFromRecording,
            IsWorldPremiere = bo.IsWorldPremiere,
            IsLivePremiere = bo.IsLivePremiere,
        };
    }

    public static SongParametersBo ToBo(this SongParametersDto dto)
    {
        return new SongParametersBo
        {
            SongId = (uint?)dto.Id,
            SongTitle = dto.Title,
            Isrc = dto.Isrc,
            AppleMusicId = dto.AppleMusicId,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static SongParametersDto ToDto(this SongParametersBo bo)
    {
        return new SongParametersDto
        {
            Id = bo.SongId,
            Title = bo.SongTitle,
            Isrc = bo.Isrc,
            AppleMusicId = bo.AppleMusicId,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static SongVariantParametersBo ToBo(this SongVariantParametersDto dto)
    {
        return new SongVariantParametersBo
        {
            SongVariantId = (uint?)dto.Id,
            SongId = (uint?)dto.SongId,
            VariantName = dto.VariantName,
            Description = dto.Description,
            IsrcOverride = dto.IsrcOverride,
            AppleMusicIdOverride = dto.AppleMusicIdOverride,
        };
    }

    public static SongVariantParametersDto ToDto(this SongVariantParametersBo bo)
    {
        return new SongVariantParametersDto
        {
            Id = bo.SongVariantId,
            SongId = bo.SongId,
            VariantName = bo.VariantName,
            Description = bo.Description,
            IsrcOverride = bo.IsrcOverride,
            AppleMusicIdOverride = bo.AppleMusicIdOverride,
        };
    }

    public static SongMashupParametersBo ToBo(this SongMashupParametersDto dto)
    {
        return new SongMashupParametersBo
        {
            SongMashupId = (uint)(dto.Id ?? 0),
        };
    }

    public static SongMashupParametersDto ToDto(this SongMashupParametersBo bo)
    {
        return new SongMashupParametersDto
        {
            Id = bo.SongMashupId,
        };
    }

    // Setlists, Acts, Entries, Extras
    public static SetlistHeaderBo ToBo(this SetlistHeaderDto dto)
    {
        return new SetlistHeaderBo
        {
            Id = (uint)(dto.Id ?? 0),
            ConcertId = dto.ConcertId ?? string.Empty,
            ConcertTitle = dto.ConcertTitle ?? string.Empty,
            SetName = dto.SetName,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static SetlistHeaderDto ToDto(this SetlistHeaderBo bo)
    {
        return new SetlistHeaderDto
        {
            Id = bo.Id,
            ConcertId = bo.ConcertId,
            ConcertTitle = bo.ConcertTitle,
            SetName = bo.SetName,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static SetlistActBo ToBo(this SetlistActDto dto)
    {
        return new SetlistActBo
        {
            SetlistId = (uint)(dto.SetlistId ?? 0),
            ActNumber = (uint)(dto.ActNumber ?? 0),
            Title = dto.Title,
        };
    }

    public static SetlistActDto ToDto(this SetlistActBo bo)
    {
        return new SetlistActDto
        {
            SetlistId = bo.SetlistId,
            ActNumber = bo.ActNumber,
            Title = bo.Title,
        };
    }

    public static SetlistEntrySongExtraBo.ExtraType ToBo(this SetlistEntrySongExtraDto.TypeEnum dto)
    {
        return dto switch
        {
            SetlistEntrySongExtraDto.TypeEnum.ExtendedBridge => SetlistEntrySongExtraBo.ExtraType.ExtendedBridge,
            SetlistEntrySongExtraDto.TypeEnum.ExtraVerse => SetlistEntrySongExtraBo.ExtraType.ExtraVerse,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, null)
        };
    }

    public static SetlistEntrySongExtraDto.TypeEnum ToDto(this SetlistEntrySongExtraBo.ExtraType bo)
    {
        return bo switch
        {
            SetlistEntrySongExtraBo.ExtraType.ExtendedBridge => SetlistEntrySongExtraDto.TypeEnum.ExtendedBridge,
            SetlistEntrySongExtraBo.ExtraType.ExtraVerse => SetlistEntrySongExtraDto.TypeEnum.ExtraVerse,
            _ => throw new ArgumentOutOfRangeException(nameof(bo), bo, null)
        };
    }

    public static SetlistEntrySongExtraBo.ExtraType ToBo(this AddSongExtraToSetlistEntryRequestDto.TypeEnum dto)
    {
        return dto switch
        {
            AddSongExtraToSetlistEntryRequestDto.TypeEnum.ExtendedBridge => SetlistEntrySongExtraBo.ExtraType.ExtendedBridge,
            AddSongExtraToSetlistEntryRequestDto.TypeEnum.ExtraVerse => SetlistEntrySongExtraBo.ExtraType.ExtraVerse,
            _ => throw new ArgumentOutOfRangeException(nameof(dto), dto, null)
        };
    }

    public static AddSongExtraToSetlistEntryRequestDto.TypeEnum ToDtoExtraRequestType(this SetlistEntrySongExtraBo.ExtraType bo)
    {
        return bo switch
        {
            SetlistEntrySongExtraBo.ExtraType.ExtendedBridge => AddSongExtraToSetlistEntryRequestDto.TypeEnum.ExtendedBridge,
            SetlistEntrySongExtraBo.ExtraType.ExtraVerse => AddSongExtraToSetlistEntryRequestDto.TypeEnum.ExtraVerse,
            _ => throw new ArgumentOutOfRangeException(nameof(bo), bo, null)
        };
    }

    public static SetlistEntrySongExtraBo ToBo(this SetlistEntrySongExtraDto dto)
    {
        return new SetlistEntrySongExtraBo
        {
            Id = dto.Id ?? string.Empty,
            Song = dto.Song?.ToBo(),
            Type = dto.Type.ToBo(),
            Description = dto.Description ?? string.Empty,
        };
    }

    public static SetlistEntrySongExtraDto ToDto(this SetlistEntrySongExtraBo bo)
    {
        return new SetlistEntrySongExtraDto
        {
            Id = bo.Id,
            Song = bo.Song?.ToDto(),
            Type = bo.Type.ToDto(),
            Description = bo.Description,
        };
    }

    public static SetlistEntryBo ToBo(this SetlistEntryDto dto)
    {
        return new SetlistEntryBo
        {
            Id = dto.Id ?? string.Empty,
            ActNumber = (uint?)dto.ActNumber,
            SortNumber = (uint)(dto.SortNumber ?? 0),
            SongNumber = (ushort)(dto.SongNumber ?? 0),
            PlayedSongVariant = dto.PlayedSongVariant?.ToBo(),
            PlayedSong = dto.PlayedSong?.ToBo(),
            PlayedSongMashup = dto.PlayedSongMashup?.ToBo(),
            Title = dto.Title ?? string.Empty,
            ExtraNotes = dto.ExtraNotes,
            Isrc = dto.Isrc,
            AppleMusicId = dto.AppleMusicId,
            LinkinpediaUrl = dto.LinkinpediaUrl,
            IsRotationSong = dto.IsRotationSong ?? false,
            IsPlayedFromRecording = dto.IsPlayedFromRecording ?? false,
            IsWorldPremiere = dto.IsWorldPremiere ?? false,
            IsLivePremiere = dto.IsLivePremiere ?? false,
            SongExtras = dto.Extras?.Select(ToBo).ToList() ?? [],
        };
    }

    public static SetlistEntryDto ToDto(this SetlistEntryBo bo)
    {
        return new SetlistEntryDto
        {
            Id = bo.Id,
            ActNumber = bo.ActNumber,
            SortNumber = bo.SortNumber,
            SongNumber = bo.SongNumber,
            PlayedSongVariant = bo.PlayedSongVariant?.ToDto(),
            PlayedSong = bo.PlayedSong?.ToDto(),
            PlayedSongMashup = bo.PlayedSongMashup?.ToDto(),
            Title = bo.Title,
            ExtraNotes = bo.ExtraNotes,
            Isrc = bo.Isrc,
            AppleMusicId = bo.AppleMusicId,
            LinkinpediaUrl = bo.LinkinpediaUrl,
            IsRotationSong = bo.IsRotationSong,
            IsPlayedFromRecording = bo.IsPlayedFromRecording,
            IsWorldPremiere = bo.IsWorldPremiere,
            IsLivePremiere = bo.IsLivePremiere,
            Extras = bo.SongExtras.Select(ToDto).ToList(),
        };
    }

    public static RawSetlistEntryBo ToBo(this RawSetlistEntryDto dto)
    {
        return new RawSetlistEntryBo
        {
            Id = dto.Id ?? string.Empty,
            ActNumber = (uint?)dto.ActNumber,
            SortNumber = (uint)(dto.SortNumber ?? 0),
            SongNumber = (ushort)(dto.SongNumber ?? 0),
            PlayedSong = dto.SongId != null ? new SongBo { Id = (uint)dto.SongId.Value, Title = dto.Title ?? string.Empty } : null,
            PlayedSongVariant = dto.SongVariantId != null ? new SongVariantBo { Id = (uint)dto.SongVariantId.Value, SongId = (uint)(dto.SongId ?? 0), VariantName = dto.Title ?? string.Empty } : null,
            PlayedSongMashup = dto.SongMashupId != null ? new SongMashupBo { Id = (uint)dto.SongMashupId.Value, Title = dto.Title ?? string.Empty, Songs = [] } : null,
            TitleOverride = dto.Title,
            ExtraNotes = dto.ExtraNotes,
            LinkinpediaUrl = dto.LinkinpediaUrl,
            IsRotationSong = dto.IsRotationSong ?? false,
            IsPlayedFromRecording = dto.IsPlayedFromRecording ?? false,
            IsWorldPremiere = dto.IsWorldPremiere ?? false,
            IsLivePremiere = dto.IsLivePremiere ?? false,
            SongExtras = dto.Extras?.Select(ToBo).ToList() ?? [],
        };
    }

    public static RawSetlistEntryDto ToDto(this RawSetlistEntryBo bo)
    {
        return new RawSetlistEntryDto
        {
            Id = bo.Id,
            ActNumber = bo.ActNumber,
            SortNumber = bo.SortNumber,
            SongNumber = bo.SongNumber,
            SongId = bo.PlayedSong?.Id,
            SongVariantId = bo.PlayedSongVariant?.Id,
            SongMashupId = bo.PlayedSongMashup?.Id,
            Title = bo.TitleOverride,
            ExtraNotes = bo.ExtraNotes,
            Isrc = bo.PlayedSong?.Isrc ?? bo.PlayedSongVariant?.IsrcOverride,
            AppleMusicId = bo.PlayedSong?.AppleMusicId ?? bo.PlayedSongVariant?.AppleMusicIdOverride,
            LinkinpediaUrl = bo.LinkinpediaUrl,
            IsRotationSong = bo.IsRotationSong,
            IsPlayedFromRecording = bo.IsPlayedFromRecording,
            IsWorldPremiere = bo.IsWorldPremiere,
            IsLivePremiere = bo.IsLivePremiere,
            Extras = bo.SongExtras.Select(ToDto).ToList(),
        };
    }

    public static SetlistBo ToBo(this SetlistDto dto)
    {
        return new SetlistBo
        {
            Id = (uint)(dto.Id ?? 0),
            ConcertId = dto.ConcertId ?? string.Empty,
            ConcertTitle = dto.ConcertTitle ?? string.Empty,
            SetName = dto.SetName,
            LinkinpediaUrl = dto.LinkinpediaUrl,
            Acts = dto.Acts?.Select(ToBo).ToList() ?? [],
            Entries = dto.Entries?.Select(ToBo).ToList() ?? [],
        };
    }

    public static SetlistDto ToDto(this SetlistBo bo)
    {
        return new SetlistDto
        {
            Id = bo.Id,
            ConcertId = bo.ConcertId,
            ConcertTitle = bo.ConcertTitle,
            SetName = bo.SetName,
            LinkinpediaUrl = bo.LinkinpediaUrl,
            Acts = bo.Acts.Select(ToDto).ToList(),
            Entries = bo.Entries.Select(ToDto).ToList(),
        };
    }

    public static CreateSetlistRequestBo ToBo(this CreateSetlistRequestDto dto)
    {
        return new CreateSetlistRequestBo
        {
            ConcertId = dto.ConcertId ?? string.Empty,
            ConcertTitle = dto.ConcertTitle ?? string.Empty,
            SetName = dto.SetName,
            LinkinpediaUrl = dto.LinkinpediaUrl,
            SongEntries = dto.AddSongs?.Select(ToBo).ToList(),
            SongVariantEntries = dto.AddSongVariants?.Select(ToBo).ToList(),
            MashupEntries = dto.AddSongMashups?.Select(ToBo).ToList(),
        };
    }

    public static CreateSetlistRequestDto ToDto(this CreateSetlistRequestBo bo)
    {
        return new CreateSetlistRequestDto
        {
            ConcertId = bo.ConcertId,
            ConcertTitle = bo.ConcertTitle,
            SetName = bo.SetName,
            LinkinpediaUrl = bo.LinkinpediaUrl,
            AddSongs = bo.SongEntries?.Select(ToDto).ToList(),
            AddSongVariants = bo.SongVariantEntries?.Select(ToDto).ToList(),
            AddSongMashups = bo.MashupEntries?.Select(ToDto).ToList(),
        };
    }

    public static CreateSetlistResponseBo ToBo(this CreateSetlistResponseDto dto)
    {
        return new CreateSetlistResponseBo
        {
            Id = uint.TryParse(dto.Id, out var id) ? id : 0,
            ConcertId = dto.ConcertId ?? string.Empty,
            ConcertTitle = dto.ConcertTitle ?? string.Empty,
            SetName = dto.SetName,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static CreateSetlistResponseDto ToDto(this CreateSetlistResponseBo bo)
    {
        return new CreateSetlistResponseDto
        {
            Id = bo.Id.ToString(),
            ConcertId = bo.ConcertId,
            ConcertTitle = bo.ConcertTitle,
            SetName = bo.SetName,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static UpdateSetlistHeaderRequestBo ToBo(this UpdateSetlistHeaderRequestDto dto)
    {
        return new UpdateSetlistHeaderRequestBo
        {
            ConcertId = dto.ConcertId ?? string.Empty,
            SetName = dto.SetName,
            LinkinpediaUrl = dto.LinkinpediaUrl,
        };
    }

    public static UpdateSetlistHeaderRequestDto ToDto(this UpdateSetlistHeaderRequestBo bo)
    {
        return new UpdateSetlistHeaderRequestDto
        {
            ConcertId = bo.ConcertId,
            SetName = bo.SetName,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    public static UpdateSetlistEntryRequestBo ToBo(this UpdateSetlistEntryRequestDto dto)
    {
        return new UpdateSetlistEntryRequestBo
        {
            ActParameters = dto.ActParameters?.ToBo(),
            EntryParameters = dto.EntryParameters?.ToBo() ?? throw new ArgumentNullException(nameof(dto.EntryParameters)),
            SongParameters = dto.SongParameters?.ToBo(),
            SongVariantParameters = dto.SongVariantParameters?.ToBo(),
            SongMashupParameters = dto.SongMashupParameters?.ToBo(),
        };
    }

    public static UpdateSetlistEntryRequestDto ToDto(this UpdateSetlistEntryRequestBo bo)
    {
        return new UpdateSetlistEntryRequestDto
        {
            ActParameters = bo.ActParameters?.ToDto(),
            EntryParameters = bo.EntryParameters.ToDto(),
            SongParameters = bo.SongParameters?.ToDto(),
            SongVariantParameters = bo.SongVariantParameters?.ToDto(),
            SongMashupParameters = bo.SongMashupParameters?.ToDto(),
        };
    }

    public static ReorderSetlistEntriesRequestBo ToBo(this ReorderSetlistEntriesRequestDto dto)
    {
        return new ReorderSetlistEntriesRequestBo
        {
            EntryIds = dto.EntryIds?.ToArray() ?? [],
        };
    }

    public static ReorderSetlistEntriesRequestDto ToDto(this ReorderSetlistEntriesRequestBo bo)
    {
        return new ReorderSetlistEntriesRequestDto
        {
            EntryIds = bo.EntryIds.ToList(),
        };
    }

    public static ReorderSetlistEntriesResponseBo ToBo(this ReorderSetlistEntriesResponseDto dto)
    {
        return new ReorderSetlistEntriesResponseBo
        {
            ReorderedEntries = dto.ReorderedEntries?.Select(ToBo).ToList() ?? [],
        };
    }

    public static ReorderSetlistEntriesResponseDto ToDto(this ReorderSetlistEntriesResponseBo bo)
    {
        return new ReorderSetlistEntriesResponseDto
        {
            ReorderedEntries = bo.ReorderedEntries.Select(ToDto).ToList(),
        };
    }

    public static AddSongToSetlistRequestBo ToBo(this AddSongToSetlistRequestDto dto)
    {
        return new AddSongToSetlistRequestBo
        {
            Act = dto.ActParameters?.ToBo(),
            EntryParameters = dto.EntryParameters?.ToBo() ?? throw new ArgumentNullException(nameof(dto.EntryParameters)),
            SongParameters = dto.SongParameters?.ToBo() ?? throw new ArgumentNullException(nameof(dto.SongParameters)),
        };
    }

    public static AddSongToSetlistRequestDto ToDto(this AddSongToSetlistRequestBo bo)
    {
        return new AddSongToSetlistRequestDto
        {
            ActParameters = bo.Act?.ToDto(),
            EntryParameters = bo.EntryParameters.ToDto(),
            SongParameters = bo.SongParameters.ToDto(),
        };
    }

    public static AddSongToSetlistResponseBo ToBo(this AddSongToSetlistResponseDto dto)
    {
        return new AddSongToSetlistResponseBo(dto.AddedSetlistEntry?.ToBo() ?? throw new ArgumentNullException(nameof(dto.AddedSetlistEntry)));
    }

    public static AddSongToSetlistResponseDto ToDto(this AddSongToSetlistResponseBo bo)
    {
        return new AddSongToSetlistResponseDto
        {
            AddedSetlistEntry = bo.AddedSetlistEntry.ToDto(),
        };
    }

    public static AddSongVariantToSetlistRequestBo ToBo(this AddSongVariantToSetlistRequestDto dto)
    {
        return new AddSongVariantToSetlistRequestBo
        {
            Act = dto.ActParameters?.ToBo(),
            EntryParameters = dto.EntryParameters?.ToBo() ?? throw new ArgumentNullException(nameof(dto.EntryParameters)),
            SongVariantParameters = dto.SongVariantParameters?.ToBo() ?? throw new ArgumentNullException(nameof(dto.SongVariantParameters)),
        };
    }

    public static AddSongVariantToSetlistRequestDto ToDto(this AddSongVariantToSetlistRequestBo bo)
    {
        return new AddSongVariantToSetlistRequestDto
        {
            ActParameters = bo.Act?.ToDto(),
            EntryParameters = bo.EntryParameters.ToDto(),
            SongVariantParameters = bo.SongVariantParameters.ToDto(),
        };
    }

    public static AddSongVariantToSetlistResponseBo ToBo(this AddSongVariantToSetlistResponseDto dto)
    {
        return new AddSongVariantToSetlistResponseBo(dto.AddedSetlistEntry?.ToBo() ?? throw new ArgumentNullException(nameof(dto.AddedSetlistEntry)));
    }

    public static AddSongVariantToSetlistResponseDto ToDto(this AddSongVariantToSetlistResponseBo bo)
    {
        return new AddSongVariantToSetlistResponseDto
        {
            AddedSetlistEntry = bo.AddedSetlistEntry.ToDto(),
        };
    }

    public static AddSongMashupToSetlistRequestBo ToBo(this AddSongMashupToSetlistRequestDto dto)
    {
        return new AddSongMashupToSetlistRequestBo
        {
            Act = dto.ActParameters?.ToBo(),
            EntryParameters = dto.EntryParameters?.ToBo() ?? throw new ArgumentNullException(nameof(dto.EntryParameters)),
            SongMashupParameters = dto.SongMashupParameters?.ToBo() ?? throw new ArgumentNullException(nameof(dto.SongMashupParameters)),
        };
    }

    public static AddSongMashupToSetlistRequestDto ToDto(this AddSongMashupToSetlistRequestBo bo)
    {
        return new AddSongMashupToSetlistRequestDto
        {
            ActParameters = bo.Act?.ToDto(),
            EntryParameters = bo.EntryParameters.ToDto(),
            SongMashupParameters = bo.SongMashupParameters.ToDto(),
        };
    }

    public static AddSongMashupToSetlistResponseBo ToBo(this AddSongMashupToSetlistResponseDto dto)
    {
        return new AddSongMashupToSetlistResponseBo(dto.AddedSetlistEntry?.ToBo() ?? throw new ArgumentNullException(nameof(dto.AddedSetlistEntry)));
    }

    public static AddSongMashupToSetlistResponseDto ToDto(this AddSongMashupToSetlistResponseBo bo)
    {
        return new AddSongMashupToSetlistResponseDto
        {
            AddedSetlistEntry = bo.AddedSetlistEntry.ToDto(),
        };
    }

    public static AddCustomEntryToSetlistRequestBo ToBo(this AddCustomEntryToSetlistRequestDto dto)
    {
        return new AddCustomEntryToSetlistRequestBo
        {
            Act = dto.ActParameters?.ToBo(),
            EntryParameters = dto.EntryParameters?.ToBo() ?? throw new ArgumentNullException(nameof(dto.EntryParameters)),
        };
    }

    public static AddCustomEntryToSetlistRequestDto ToDto(this AddCustomEntryToSetlistRequestBo bo)
    {
        return new AddCustomEntryToSetlistRequestDto
        {
            ActParameters = bo.Act?.ToDto(),
            EntryParameters = bo.EntryParameters.ToDto(),
        };
    }

    public static AddCustomEntryToSetlistResponseBo ToBo(this AddCustomEntryToSetlistResponseDto dto)
    {
        return new AddCustomEntryToSetlistResponseBo(dto.AddedSetlistEntry?.ToBo() ?? throw new ArgumentNullException(nameof(dto.AddedSetlistEntry)));
    }

    public static AddCustomEntryToSetlistResponseDto ToDto(this AddCustomEntryToSetlistResponseBo bo)
    {
        return new AddCustomEntryToSetlistResponseDto
        {
            AddedSetlistEntry = bo.AddedSetlistEntry.ToDto(),
        };
    }

    public static AddSongExtraToSetlistEntryRequestBo ToBo(this AddSongExtraToSetlistEntryRequestDto dto)
    {
        return new AddSongExtraToSetlistEntryRequestBo
        {
            SongId = (uint?)dto.SongId,
            Type = dto.Type.ToBo(),
            Description = dto.Description ?? string.Empty,
        };
    }

    public static AddSongExtraToSetlistEntryRequestDto ToDto(this AddSongExtraToSetlistEntryRequestBo bo)
    {
        return new AddSongExtraToSetlistEntryRequestDto
        {
            SongId = bo.SongId,
            Type = bo.Type.ToDtoExtraRequestType(),
            Description = bo.Description,
        };
    }

    public static AddSongExtraToSetlistEntryResponseBo ToBo(this AddSongExtraToSetlistEntryResponseDto dto)
    {
        return new AddSongExtraToSetlistEntryResponseBo
        {
            Entry = dto.Entry?.ToBo() ?? throw new ArgumentNullException(nameof(dto.Entry)),
        };
    }

    public static AddSongExtraToSetlistEntryResponseDto ToDto(this AddSongExtraToSetlistEntryResponseBo bo)
    {
        return new AddSongExtraToSetlistEntryResponseDto
        {
            Entry = bo.Entry.ToDto(),
        };
    }

    // Import
    public static ImportSetlistPreviewBo ToBo(this ImportSetlistPreviewDto dto)
    {
        return new ImportSetlistPreviewBo
        {
            ConcertId = dto.ConcertId,
            Acts = dto.Acts?.Select(ToBo).ToList() ?? [],
            Entries = dto.Entries?.Select(ToBo).ToList() ?? [],
        };
    }

    public static ImportSetlistPreviewDto ToDto(this ImportSetlistPreviewBo bo)
    {
        return new ImportSetlistPreviewDto
        {
            ConcertId = bo.ConcertId,
            Acts = bo.Acts.Select(ToDto).ToList(),
            Entries = bo.Entries.Select(ToDto).ToList(),
        };
    }

    public static ImportSetlistActPreviewBo ToBo(this ImportSetlistActPreviewDto dto)
    {
        return new ImportSetlistActPreviewBo
        {
            Name = dto.Name ?? string.Empty,
            ActNumber = (uint)(dto.ActNumber ?? 0),
            ExtraNotes = dto.ExtraNotes,
        };
    }

    public static ImportSetlistActPreviewDto ToDto(this ImportSetlistActPreviewBo bo)
    {
        return new ImportSetlistActPreviewDto
        {
            Name = bo.Name,
            ActNumber = bo.ActNumber,
            ExtraNotes = bo.ExtraNotes,
        };
    }

    public static ImportSetlistEntryPreviewBo ToBo(this ImportSetlistEntryPreviewDto dto)
    {
        return new ImportSetlistEntryPreviewBo
        {
            Title = dto.Title ?? string.Empty,
            ExtraNotes = dto.ExtraNotes,
            SongNumber = (uint)(dto.SongNumber ?? 0),
            ActNumber = (uint?)dto.ActNumber,
            FoundSongId = (uint?)dto.FoundSongId,
            PossibleSongVariants = dto.PossibleSongVariants?.Select(ToBo).ToArray() ?? [],
            FoundSongIds = dto.FoundSongIds?.Select(id => (uint)id).ToArray() ?? [],
            FoundSongVariantId = (uint?)dto.FoundSongVariantId,
            FoundMashupId = (uint?)dto.FoundMashupId,
        };
    }

    public static ImportSetlistEntryPreviewDto ToDto(this ImportSetlistEntryPreviewBo bo)
    {
        return new ImportSetlistEntryPreviewDto
        {
            Title = bo.Title,
            ExtraNotes = bo.ExtraNotes,
            SongNumber = bo.SongNumber,
            ActNumber = bo.ActNumber,
            FoundSongId = bo.FoundSongId,
            PossibleSongVariants = bo.PossibleSongVariants.Select(ToDto).ToList(),
            FoundSongIds = bo.FoundSongIds.Select(id => (decimal)id).ToList(),
            FoundSongVariantId = bo.FoundSongVariantId,
            FoundMashupId = bo.FoundMashupId,
        };
    }

    public static ImportSetlistRequestBo ToBo(this ImportSetlistRequestDto dto)
    {
        return new ImportSetlistRequestBo
        {
            ConcertId = dto.ConcertId ?? string.Empty,
            LinkinpediaUrl = dto.LinkinpediaUrl ?? string.Empty,
        };
    }

    public static ImportSetlistRequestDto ToDto(this ImportSetlistRequestBo bo)
    {
        return new ImportSetlistRequestDto
        {
            ConcertId = bo.ConcertId,
            LinkinpediaUrl = bo.LinkinpediaUrl,
        };
    }

    #endregion
}