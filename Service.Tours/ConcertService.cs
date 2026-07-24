using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;

namespace Service.Tours;

/// <summary>
/// Service to manage concert data
/// </summary>
/// <param name="concertRepository"></param>
/// <param name="concertTypeRepository"></param>
/// <param name="logger"></param>
public class ConcertService(IConcertRepository concertRepository, IConcertTypeRepository concertTypeRepository, ILogger<ConcertService> logger)
{
    /// <summary>
    /// Creates a new type of concert
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ConcertTypeDto> CreateConcertType(CreateConcertTypeRequestDto request)
    {
        logger.LogDebug("Creating concert type with name: {typeName}", request.Name);
        var typeDo = request.ToDo();
        concertTypeRepository.Add(typeDo);
        await concertTypeRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created concert type with name: {typeName} (ID: {id})", request.Name, typeDo.Id);
        return typeDo.ToDto();
    }
}