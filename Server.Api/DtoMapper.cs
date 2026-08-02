using Common.Contracts.Generated.Models;
using LPCalendar.DataStructure.Tours;

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
}