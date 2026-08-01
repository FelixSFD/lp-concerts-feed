using Common.Contracts.Generated.Models;
using LPCalendar.DataStructure.Tours;

namespace Server.Api;

internal static class DtoMapper
{
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
}