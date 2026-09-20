using Common.Database;
using Common.Database.Repositories;
using Common.Utils.Pagination;
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
        return typeDo.ToBo();
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
        return typeDo.ToBo();
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
        return type.ToBo();
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
            .Select(DoMapper.ToBo);
    }
    
    #endregion

    /// <summary>
    /// Creates a new concert
    /// </summary>
    /// <param name="request"></param>
    public async Task<RawConcertBo> CreateConcertAsync(CreateConcertRequestBo request)
    {
        logger.LogDebug("Requested to create a new concert");
        var concert = request.ToDo();
        concertRepository.Add(concert);
        await concertRepository.SaveChangesAsync();
        logger.LogDebug("Successfully created concert with ID: {concertId}", concert.Id);
        var concertDetails = await concertRepository.GetByPrimaryKeyAsync(concert.Id) ?? throw new ConcertNotFoundException("new");
        return concertDetails.ToBo();
    }
    
    /// <summary>
    /// Updates an existing concert
    /// </summary>
    /// <param name="request"></param>
    /// <param name="concertId">ID of the concert to update</param>
    public async Task<RawConcertBo> UpdateConcertAsync(string concertId, UpdateConcertRequestBo request)
    {
        logger.LogDebug("Requested to update the concert with ID: {concertId}", concertId);
        var concert = await concertRepository.GetByPrimaryKeyWithoutReferencesAsync(concertId) ?? throw new ConcertNotFoundException(concertId);
        concert.UpdateFromRequestBo(request);
        concertRepository.Update(concert);
        await concertRepository.SaveChangesAsync();
        logger.LogDebug("Successfully updated concert with ID: {concertId}", concert.Id);
        var concertDetails = await concertRepository.GetByPrimaryKeyAsync(concert.Id) ?? throw new ConcertNotFoundException(concert.Id);
        return concertDetails.ToBo();
    }

    /// <summary>
    /// Returns the concert without any of the referenced objects like the venue
    /// </summary>
    /// <param name="id">ID of the concert</param>
    /// <param name="includeDeleted">true, if deleted concerts are allowed to be returned. (Default: false)</param>
    /// <returns></returns>
    /// <exception cref="ConcertNotFoundException">if the concert does not exist</exception>
    public async Task<RawConcertBo> GetConcertWithoutDetailsByIdAsync(string id, bool includeDeleted = false)
    {
        logger.LogDebug("Requested concert without references to other objects. ID: {id}", id);
        var concert = await concertRepository.GetByPrimaryKeyWithoutReferencesAsync(id) ?? throw new ConcertNotFoundException(id);
        if (!includeDeleted)
        {
            ThrowNotFoundExceptionIfConcertDeleted(concert);
        }
        logger.LogDebug("Found concert.");
        return concert.ToBo();
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
        return concert.ToBoWithDetails();
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
    public async Task<AsyncPaginationResult<ConcertDetailsBo>> GetConcertsWithDetailsAsync(CancellationToken cancellationToken, GetConcertsFilterDto filter)
    {
        logger.LogDebug("Getting concerts with details... Fetching starting with result {offset} and take {limit}", filter.Skip, filter.Limit);
        var paginationParams = new PaginationParams(filter.Skip, filter.Limit);
        var concertFilter = new Database.Tours.Filters.ConcertFilter
        {
            CountryCode = filter.CountryCode
        };
        var paginatedResult = await concertRepository
            .GetConcertsAsync(cancellationToken, concertFilter, orderBy: filter.OrderBy.Select(SortDescriptor.FromString), paginationParams);
        logger.LogDebug("Query would return {count} concerts. A maximum of {limit} will be returned", paginatedResult.TotalCount, filter.Limit);
        return new AsyncPaginationResult<ConcertDetailsBo>
        {
            TotalResults = paginatedResult.TotalCount,
            Limit = (int)filter.Limit,
            Offset = (int)filter.Skip,
            Results = paginatedResult.Results.Select(DoMapper.ToBoWithDetails),
        };
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

    /// <summary>
    /// Returns information about adjacent concerts to the specified concert. Like the one before and after the specified concert.
    /// </summary>
    /// <param name="concertId">ID of the concert to start the search at</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ConcertNotFoundException">if the concert with ID <paramref name="concertId"/> was not found</exception>
    public async Task<AdjacentConcertsBo> GetAdjacentConcerts(string concertId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Load adjacent concerts to: {currentId}", concertId);
        var currentConcert = await concertRepository.GetByPrimaryKeyWithoutReferencesAsync(concertId) ?? throw new ConcertNotFoundException(concertId);
        logger.LogDebug("Found current concert.");
        var pagingPrev = new PaginationParams(0, 1);
        var pagingNext = new PaginationParams(0, 2);
        var getPreviousFilter = new ConcertFilter
        {
            Before = currentConcert.PostedStartTime
        };
        var getNextFilter = new ConcertFilter
        {
            After = currentConcert.PostedStartTime
        };

        var orderByPrev = new SortDescriptor("date", true);
        var orderByNext = new SortDescriptor("date");
        
        var getPreviousTask = concertRepository
            .GetConcerts(cancellationToken, getPreviousFilter, [orderByPrev], pagingPrev)
            .FirstOrDefaultAsync(cancellationToken);
        var getNextTask = concertRepository
            .GetConcerts(cancellationToken, getNextFilter, [orderByNext], pagingNext)
            .FirstOrDefaultAsync(c => c.Id != currentConcert.Id, cancellationToken);

        var previousConcert = await getPreviousTask;
        var nextConcert = await getNextTask;
        
        logger.LogDebug("Loaded adjacent concerts. Before: {before}, After: {after}", previousConcert?.Id, nextConcert?.Id);
        
        return new AdjacentConcertsBo
        {
            Previous = previousConcert?.ToBoWithDetails(),
            Next = nextConcert?.ToBoWithDetails()
        };
    }
}