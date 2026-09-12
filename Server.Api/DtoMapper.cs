using Common.Contracts.Generated.Models;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Tours;
using LPCalendar.DataStructure.Tours.Locations;
using Service.Tours.DataStructure;

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
            Id = bo.Id.ToString(),
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
            MainStageTime = dto.MainStageTime?.DateTime,
            DoorsTime = dto.DoorsTime?.DateTime,
            LpuEarlyEntryTime = dto.LpuEarlyEntryTime?.DateTime,
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
            MainStageTime = dto.MainStageTime?.DateTime,
            DoorsTime = dto.DoorsTime?.DateTime,
            LpuEarlyEntryTime = dto.LpuEarlyEntryTime?.DateTime,
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
            ConcertType = bo.ConcertType?.ToDto(),
            PostedStartTime = bo.PostedStartTime,
            FoundCities = [.. bo.FoundCities.Select(ToDto)],
            FoundVenues = [.. bo.FoundVenues.Select(ToDto)],
            FoundCountries = [.. bo.FoundCountries.Select(ToDto)],
            FoundStates = [.. bo.FoundStates.Select(ToDto)],
            CountryName = bo.CountryName,
            StateName = bo.StateName,
            CityName = bo.CityName,
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
}