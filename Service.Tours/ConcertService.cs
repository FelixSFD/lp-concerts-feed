using Common.Database;
using Common.Database.Repositories;
using Database.Tours.DataObjects;
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
    #region Concert Types
    
    /// <summary>
    /// Creates a new type of concert
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ConcertTypeBo> CreateConcertTypeAsync(CreateConcertTypeRequest request)
    {
        logger.LogDebug("Creating concert type with name: {typeName}", request.Name);
        var typeDo = request.ToDo();
        concertTypeRepository.Add(typeDo);
        await concertTypeRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created concert type with name: {typeName} (ID: {id})", request.Name, typeDo.Id);
        return typeDo.ToDto();
    }
    
    /// <summary>
    /// Updates a type of concert
    /// </summary>
    /// <param name="request"></param>
    /// <param name="id">ID of the concert type</param>
    /// <returns></returns>
    public async Task<ConcertTypeBo> UpdateConcertTypeAsync(UpdateConcertTypeRequest request, uint id)
    {
        logger.LogDebug("Updating concert type with ID: {id}", id);
        var typeDo = await concertTypeRepository.GetByPrimaryKeyWithoutReferencesAsync(id) ?? throw new ConcertTypeNotFoundException(id);
        typeDo.UpdateFromRequestDto(request);
        concertTypeRepository.Update(typeDo);
        await concertTypeRepository.SaveChangesAsync();
        logger.LogDebug("Successfully updated concert type with name: {typeName} (ID: {id})", request.Name, typeDo.Id);
        return typeDo.ToDto();
    }

    /// <summary>
    /// Returns the <see cref="ConcertTypeBo"/> for a given ID
    /// </summary>
    /// <param name="id">ID of the concert type</param>
    /// <returns>Information about the concert type</returns>
    /// <exception cref="ConcertTypeNotFoundException">if the type does not exist</exception>
    public async Task<ConcertTypeBo> GetConcertTypeAsync(uint id)
    {
        logger.LogDebug("Read concert type with ID: {id}", id);
        var type = await concertTypeRepository.GetByPrimaryKeyAsync(id) ?? throw new ConcertTypeNotFoundException(id);
        logger.LogDebug("Found concert type: {name}", type.Name);
        return type.ToDto();
    }
    
    /// <summary>
    /// Returns all <see cref="ConcertTypeBo"/>s
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>Information about all concert types</returns>
    public IAsyncEnumerable<ConcertTypeBo> GetConcertTypesAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Read all concert types");
        return concertTypeRepository
            .QueryAsync(cancellationToken)
            .Select(DtoMapper.ToDto);
    }
    
    #endregion

    /// <summary>
    /// Creates a new concert
    /// </summary>
    /// <param name="request"></param>
    public async Task<RawConcertDto> CreateConcertAsync(CreateConcertRequestDto request)
    {
        logger.LogDebug("Requested to create a new concert");
        var concert = request.ToDo();
        concertRepository.Add(concert);
        await concertRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created concert with ID: {concertId}", concert.Id);
        return concert.ToDto();
    }
    
    /// <summary>
    /// Updates an existing concert
    /// </summary>
    /// <param name="request"></param>
    /// <param name="concertId">ID of the concert to update</param>
    public async Task<RawConcertDto> UpdateConcertAsync(string concertId, UpdateConcertRequestDto request)
    {
        logger.LogDebug("Requested to update the concert with ID: {concertId}", concertId);
        var concert = await concertRepository.GetByPrimaryKeyWithoutReferencesAsync(concertId) ?? throw new ConcertNotFoundException(concertId);
        concert.UpdateFromRequestDto(request);
        concertRepository.Update(concert);
        await concertRepository.SaveChangesAsync();
        logger.LogDebug("Successfully updated concert with ID: {concertId}", concert.Id);
        return concert.ToDto();
    }

    /// <summary>
    /// Returns the concert without any of the referenced objects like the venue
    /// </summary>
    /// <param name="id">ID of the concert</param>
    /// <param name="includeDeleted">true, if deleted concerts are allowed to be returned. (Default: false)</param>
    /// <returns></returns>
    /// <exception cref="ConcertNotFoundException">if the concert does not exist</exception>
    public async Task<RawConcertDto> GetConcertWithoutDetailsByIdAsync(string id, bool includeDeleted = false)
    {
        logger.LogDebug("Requested concert without references to other objects. ID: {id}", id);
        var concert = await concertRepository.GetByPrimaryKeyWithoutReferencesAsync(id) ?? throw new ConcertNotFoundException(id);
        if (!includeDeleted)
        {
            ThrowNotFoundExceptionIfConcertDeleted(concert);
        }
        logger.LogDebug("Found concert.");
        return concert.ToDto();
    }
    
    /// <summary>
    /// Returns the concert including all the referenced objects like the venue
    /// </summary>
    /// <param name="id">ID of the concert</param>
    /// <param name="includeDeleted">true, if deleted concerts are allowed to be returned. (Default: false)</param>
    /// <returns></returns>
    /// <exception cref="ConcertNotFoundException">if the concert does not exist</exception>
    public async Task<ConcertDetailsBo> GetConcertByIdAsync(string id, bool includeDeleted = false)
    {
        logger.LogDebug("Requested concert including references to other objects. ID: {id}", id);
        var concert = await concertRepository.GetByPrimaryKeyAsync(id) ?? throw new ConcertNotFoundException(id);
        if (!includeDeleted)
        {
            ThrowNotFoundExceptionIfConcertDeleted(concert);
        }
        logger.LogDebug("Found concert.");
        return concert.ToDtoWithDetails();
    }

    private void ThrowNotFoundExceptionIfConcertDeleted(ConcertDo concert)
    {
        if (concert.DeletedAt != null && concert.DeletedAt <= DateTimeOffset.UtcNow)
        {
            logger.LogInformation("The concert with ID '{concertId}' was found in the database, but it's marked as deleted.", concert.Id);
           throw new ConcertNotFoundException(concert.Id); 
        }
    }

    /// <summary>
    /// Returns a (filtered) list of concerts.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="filter">Filter and sorting</param>
    /// <returns>Details about the concerts matching the filter</returns>
    public IAsyncEnumerable<ConcertDetailsBo> GetConcertsWithDetailsAsync(CancellationToken cancellationToken, GetConcertsFilterDto filter)
    {
        var paginationParams = new PaginationParams(filter.Skip, filter.Limit);
        return concertRepository
            .GetConcerts(cancellationToken, filter.CountryCode, orderBy: filter.OrderBy.Select(SortDescriptor.FromString), paginationParams)
            .Select(DtoMapper.ToDtoWithDetails);
    }

    /// <summary>
    /// Deletes a concert
    /// </summary>
    /// <param name="concertId"></param>
    /// <param name="removeFromDb">true, if the entry should actually be removed from the DB. The default is "false", which only marks the concert as deleted.</param>
    /// <exception cref="ConcertNotFoundException">if the concert doesn't exist</exception>
    public async Task DeleteConcertAsync(string concertId, bool removeFromDb = false)
    {
        logger.LogInformation("Deleting concert with ID: {concertId}", concertId);
        var concert = await concertRepository.GetByPrimaryKeyWithoutReferencesAsync(concertId) ?? throw new ConcertNotFoundException(concertId);
        logger.LogDebug("Found concert.");
        if (removeFromDb)
        {
            logger.LogWarning("Will actually remove the concert '{concertId}' from the database", concertId);
            concertRepository.Delete(concert);
        }
        else
        {
            concert.DeletedAt = DateTime.UtcNow;
            concertRepository.Update(concert);
            logger.LogDebug("Marked concert as deleted.");
        }
        
        await concertRepository.SaveChangesAsync();
    }
}