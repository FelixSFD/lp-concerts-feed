using Common.Contracts.Generated.Controllers;
using Common.Contracts.Generated.Models;
using Microsoft.AspNetCore.Mvc;
using Server.Api.Auth;
using Service.Tours;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to manage concert types
/// </summary>
/// <param name="concertService"></param>
/// <param name="logger"></param>
[ApiController]
[Route("v3/[controller]")]
public class ConcertTypesController(ConcertService concertService, ILogger<ConcertTypesController> logger) : ConcertTypesApiController
{
    /// <summary>
    /// Creates a new type of concert
    /// </summary>
    /// <param name="createConcertTypeRequestDto"></param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles]
    public override async Task<IActionResult> CreateConcertType([FromBody] CreateConcertTypeRequestDto createConcertTypeRequestDto, CancellationToken cancellationToken)
    {
        var createdType = await concertService.CreateConcertTypeAsync(createConcertTypeRequestDto.ToBo());
        return CreatedAtAction(nameof(GetTypeById), new { concertTypeId = createdType.Id }, createdType);
    }
    
    /// <summary>
    /// Returns all available concert types
    /// </summary>
    /// <returns>information about the concert types</returns>
    [HttpGet]
    [AuthorizeRoles]
    public override async Task<IActionResult> GetConcertTypes(CancellationToken cancellationToken)
    {
        var types = await concertService
            .GetConcertTypesAsync(cancellationToken)
            .ToArrayAsync(cancellationToken);
        return Ok(types);
    }
    
    /// <summary>
    /// Returns information about a concert type
    /// </summary>
    /// <param name="concertTypeId">ID of the concert type</param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>information about the concert type</returns>
    [HttpGet("{concertTypeId:int}")]
    public override async Task<IActionResult> GetTypeById(int concertTypeId, CancellationToken cancellationToken)
    {
        var type = await concertService.GetConcertTypeAsync((uint)concertTypeId);
        return Ok(type);
    }

    /// <summary>
    /// Updates information of a concert type
    /// </summary>
    /// <param name="concertTypeId">ID of the concert type</param>
    /// <param name="request">new data for the type</param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>updated information about the concert type</returns>
    [HttpPut("{concertTypeId:int}")]
    public override async Task<IActionResult> UpdateType([FromRoute] int concertTypeId, [FromBody] UpdateConcertTypeRequestDto request, CancellationToken cancellationToken)
    {
        var type = await concertService.UpdateConcertTypeAsync(request.ToBo(), (uint)concertTypeId);
        return Ok(type);
    }
}