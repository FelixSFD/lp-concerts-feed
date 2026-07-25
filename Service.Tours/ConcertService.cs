using Database.Tours.Repositories;
using LPCalendar.DataStructure.Tours;
using Microsoft.Extensions.Logging;
using Service.Tours.Exceptions;

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
    public async Task<ConcertTypeDto> CreateConcertTypeAsync(CreateConcertTypeRequestDto request)
    {
        logger.LogDebug("Creating concert type with name: {typeName}", request.Name);
        var typeDo = request.ToDo();
        concertTypeRepository.Add(typeDo);
        await concertTypeRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created concert type with name: {typeName} (ID: {id})", request.Name, typeDo.Id);
        return typeDo.ToDto();
    }

    /// <summary>
    /// Returns the <see cref="ConcertTypeDto"/> for a given ID
    /// </summary>
    /// <param name="id">ID of the concert type</param>
    /// <returns>Information about the concert type</returns>
    /// <exception cref="ConcertTypeNotFoundException">if the type does not exist</exception>
    public async Task<ConcertTypeDto> GetConcertTypeAsync(uint id)
    {
        logger.LogDebug("Read concert type with ID: {id}", id);
        var type = await concertTypeRepository.GetByPrimaryKeyAsync(id) ?? throw new ConcertTypeNotFoundException(id);
        logger.LogDebug("Found concert type: {name}", type.Name);
        return type.ToDto();
    }
    
    /// <summary>
    /// Returns all <see cref="ConcertTypeDto"/>s
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>Information about all concert types</returns>
    public IAsyncEnumerable<ConcertTypeDto> GetConcertTypesAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Read all concert types");
        return concertTypeRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToDto);
    }
}