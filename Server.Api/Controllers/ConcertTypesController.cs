using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts.Generated.Models;
using Common.Utils.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using Server.Api.Auth;
using Server.Api.Cache;
using Server.Api.ExceptionHandling;
using Service.Tours;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to manage concert types
/// </summary>
/// <param name="concertService"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/[controller]")]
public class ConcertTypesController(ConcertService concertService, ILogger<ConcertTypesController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a new type of concert
    /// </summary>
    /// <param name="createConcertTypeRequestDto"></param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles]
    public async Task<CreatedAtActionResult> CreateConcertType([FromBody] CreateConcertTypeRequestDto createConcertTypeRequestDto)
    {
        var createdType = await concertService.CreateConcertTypeAsync(createConcertTypeRequestDto.ToBo());
        return CreatedAtAction(nameof(GetTypeById), new { concertTypeId = createdType.Id }, createdType.ToDto());
    }
    
    /// <summary>
    /// Returns all available concert types
    /// </summary>
    /// <returns>information about the concert types</returns>
    [HttpGet]
    [AuthorizeRoles]
    [OutputCache(PolicyName = CachePolicyNames.VeryLong)]
    [CustomResponseCache(Duration = CacheExpiration.VeryLong)]
    public async Task<ActionResult<ConcertTypeDto[]>> GetConcertTypes(CancellationToken cancellationToken)
    {
        var types = await concertService
            .GetConcertTypesAsync(cancellationToken)
            .Select(DtoMapper.ToDto)
            .ToArrayAsync(cancellationToken);
        return Ok(types);
    }
    
    /// <summary>
    /// Returns information about a concert type
    /// </summary>
    /// <param name="concertTypeId">ID of the concert type</param>
    /// <returns>information about the concert type</returns>
    [HttpGet("{concertTypeId:int}")]
    [OutputCache(PolicyName = CachePolicyNames.VeryLong)]
    [CustomResponseCache(Duration = CacheExpiration.VeryLong)]
    public async Task<ActionResult<ConcertTypeDto>> GetTypeById(int concertTypeId)
    {
        var type = await concertService.GetConcertTypeAsync(concertTypeId.ConvertToUnsigned());
        return Ok(type.ToDto());
    }

    /// <summary>
    /// Updates information of a concert type
    /// </summary>
    /// <param name="concertTypeId">ID of the concert type</param>
    /// <param name="request">new data for the type</param>
    /// <returns>updated information about the concert type</returns>
    [HttpPut("{concertTypeId:int}")]
    public async Task<ActionResult<ConcertTypeDto>> UpdateType([FromRoute] int concertTypeId, [FromBody] UpdateConcertTypeRequestDto request)
    {
        var type = await concertService.UpdateConcertTypeAsync(request.ToBo(), concertTypeId.ConvertToUnsigned());
        return Ok(type.ToDto());
    }
}