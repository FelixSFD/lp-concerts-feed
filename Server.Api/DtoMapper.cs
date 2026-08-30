using System.Linq;
using Common.Contracts.Generated.Models;
using Database.Tours.DataObjects;
using LPCalendar.DataStructure;
using LPCalendar.DataStructure.Tours;
using LPCalendar.DataStructure.Tours.Locations;

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
            Id = bo.Id.ToString(),
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
            Id = bo.Id.ToString(),
            CountryCode = bo.CountryCode,
            StateCode = bo.StateCode,
            CityId = bo.CityId.ToString(),
            CurrentName = bo.CurrentName,
            VarTimeZone = bo.TimeZone,
            Latitude = (double?)bo.Latitude,
            Longitude = (double?)bo.Longitude,
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
            Id = bo.Id.ToString(),
            CountryCode = bo.CountryCode,
            StateCode = bo.StateCode,
            CityId = bo.CityId.ToString(),
            CurrentName = bo.CurrentName,
            VarTimeZone = bo.TimeZone,
            Latitude = (double?)bo.Latitude,
            Longitude = (double?)bo.Longitude,
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
            Id = bo.Id.ToString(),
            CountryCode = bo.CountryCode,
            StateCode = bo.StateCode,
            CityId = bo.CityId.ToString(),
            CurrentName = bo.CurrentName,
            VarTimeZone = bo.TimeZone,
            Latitude = (double?)bo.Latitude,
            Longitude = (double?)bo.Longitude,
            City = bo.City.ToDto(),
            VenueNames = [.. bo.VenueNames.Select(ToDto)],
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
            VenueId = bo.VenueId.ToString(),
            Name = bo.Name,
            UsedFrom = bo.UsedFrom,
            UsedUntil = bo.UsedUntil,
        };
    }

    #endregion

    #region Concerts

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
            MainStageTime = bo.MainStageTime,
            DoorsTime = bo.DoorsTime,
            LpuEarlyEntryTime = bo.LpuEarlyEntryTime,
            LpuEarlyEntryConfirmed = bo.LpuEarlyEntryConfirmed,
            ExpectedSetDurationMinutes = bo.ExpectedSetDurationMinutes.ToString(),
            ScheduleImageFile = bo.ScheduleImageFile,
            DeletedAt = bo.DeletedAt,
            Status = bo.Status.ToDto(),
        };
    }
    
    public static ConcertStatusValueDto ToDto(this ConcertDto.ConcertStatusValue data)
    {
        return data switch
        {
            ConcertDto.ConcertStatusValue.Planned => ConcertStatusValueDto.PlannedEnum,
            ConcertDto.ConcertStatusValue.Running => ConcertStatusValueDto.RunningEnum,
            ConcertDto.ConcertStatusValue.Past => ConcertStatusValueDto.PastEnum,
            ConcertDto.ConcertStatusValue.Cancelled => ConcertStatusValueDto.CancelledEnum,
            _ => ConcertStatusValueDto.PastEnum
        };
    }

    #endregion
}