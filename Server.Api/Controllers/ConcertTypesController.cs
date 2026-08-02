using Common.Contracts.Generated.Controllers;
using Common.Contracts.Generated.Models;
using Microsoft.AspNetCore.Mvc;
using Server.Api.Auth;
using Server.Api.ExceptionHandling;
using Service.Tours;

namespace Server.Api.Controllers;

/// <summary>
/// Controller to manage concert types
/// </summary>
/// <param name="concertService"></param>
/// <param name="logger"></param>
/// <param name="cancellationToken">Token to cancel the request</param>
[ApiController]
[Route("v3/[controller]")]
public class ConcertTypesController(ConcertService concertService, ILogger<ConcertTypesController> logger, CancellationToken cancellationToken) : ConcertTypesApiController
{
    /// <summary>
    /// Creates a new type of concert
    /// </summary>
    /// <param name="createConcertTypeRequestDto"></param>
    /// <returns></returns>
    [HttpPost]
    [AuthorizeRoles]
    public override async Task<IActionResult> CreateConcertType([FromBody] CreateConcertTypeRequestDto createConcertTypeRequestDto)
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
    public override async Task<IActionResult> GetConcertTypes()
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
    /// <returns>information about the concert type</returns>
    [HttpGet("{concertTypeId:int}")]
    public override async Task<IActionResult> GetTypeById(int concertTypeId)
    {
        var type = await concertService.GetConcertTypeAsync(concertTypeId.ConvertToUnsigned());
        return Ok(type);
    }

    /// <summary>
    /// Updates information of a concert type
    /// </summary>
    /// <param name="concertTypeId">ID of the concert type</param>
    /// <param name="request">new data for the type</param>
    /// <returns>updated information about the concert type</returns>
    [HttpPut("{concertTypeId:int}")]
    public override async Task<IActionResult> UpdateType([FromRoute] int concertTypeId, [FromBody] UpdateConcertTypeRequestDto request)
    {
        var type = await concertService.UpdateConcertTypeAsync(request.ToBo(), concertTypeId.ConvertToUnsigned());
        return Ok(type);
    }
}