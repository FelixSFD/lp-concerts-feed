using Common.Contracts.Generated.Models;
using Common.Server.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Quartz;
using Common.Jobs.Jobs;
using Server.Api.Auth;
using Server.Api.Models;
using Service.Users;
using Service.Users.Exceptions;

namespace Server.Api.Controllers;

/// <summary>
/// Manages calendar subscription settings. Unowned subscriptions are accessible by ID;
/// owned subscriptions are accessible only to their authenticated owner.
/// </summary>
/// <param name="service">Calendar subscription service.</param>
[ApiController]
[Route("v3/[controller]")]
[OptionalBearerAuthentication]
[HttpLogging(HttpLoggingFields.None)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CalendarSubscriptionsController(
    CalendarSubscriptionService service,
    ISchedulerFactory schedulerFactory,
    ILogger<CalendarSubscriptionsController> logger) : ControllerBase
{
    /// <summary>Creates an anonymous or owned subscription.</summary>
    /// <param name="request">Required calendar settings.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The created subscription, or a bad request if the caller has no local profile.</returns>
    [HttpPost]
    public async Task<ActionResult<CalendarSubscriptionDto>> CreateCalendarSubscription([FromBody] CalendarSubscriptionSettingsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await service.CreateAsync(HttpContext.GetUserId(), request.IncludeDoorsTime!.Value, request.IncludeLpStageTime!.Value, cancellationToken);
            await TriggerCalendarGenerationAsync(subscription.Id, cancellationToken);
            return CreatedAtAction(nameof(GetCalendarSubscription), new { subscriptionId = subscription.Id }, subscription.ToDto());
        }
        catch (MissingUserProfileException exception)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = exception.Message });
        }
    }

    /// <summary>Lists the current user's subscriptions.</summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The authenticated user's owned subscriptions.</returns>
    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<CalendarSubscriptionDto[]>> GetMyCalendarSubscriptions(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();
        return Ok((await service.GetForUserAsync(userId, cancellationToken)).Select(DtoMapper.ToDto).ToArray());
    }

    /// <summary>Retrieves a subscription by ID.</summary>
    /// <param name="subscriptionId">UUID of the subscription.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The subscription, or not found if it is missing or inaccessible.</returns>
    [HttpGet("{subscriptionId:guid}")]
    public async Task<ActionResult<CalendarSubscriptionDto>> GetCalendarSubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var subscription = await service.GetAsync(subscriptionId.ToString(), HttpContext.GetUserId(), cancellationToken);
        return subscription is null ? NotFound() : Ok(subscription.ToDto());
    }

    /// <summary>Replaces a subscription's settings.</summary>
    /// <param name="subscriptionId">UUID of the subscription.</param>
    /// <param name="request">Replacement calendar settings.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>No content on success, or not found if the subscription is missing or inaccessible.</returns>
    [HttpPut("{subscriptionId:guid}")]
    public async Task<IActionResult> UpdateCalendarSubscription(Guid subscriptionId, [FromBody] CalendarSubscriptionSettingsRequest request, CancellationToken cancellationToken)
    {
        var wasUpdated = await service.UpdateAsync(subscriptionId.ToString(), HttpContext.GetUserId(), request.IncludeDoorsTime!.Value,
            request.IncludeLpStageTime!.Value, cancellationToken);
        if (!wasUpdated)
            return NotFound();

        await TriggerCalendarGenerationAsync(subscriptionId.ToString(), cancellationToken);
        return NoContent();
    }

    /// <summary>Deletes a subscription.</summary>
    /// <param name="subscriptionId">UUID of the subscription.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>No content on success, or not found if the subscription is missing or inaccessible.</returns>
    [HttpDelete("{subscriptionId:guid}")]
    public async Task<IActionResult> DeleteCalendarSubscription(Guid subscriptionId, CancellationToken cancellationToken) =>
        await service.DeleteAsync(subscriptionId.ToString(), HttpContext.GetUserId(), cancellationToken)
            ? NoContent() : NotFound();

    /// <summary>Claims an unowned subscription for the current user; an existing claim by that user is unchanged.</summary>
    /// <param name="subscriptionId">UUID of the subscription.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The owned subscription, not found if missing or owned by another user, or a bad request if the caller has no local profile.</returns>
    [Authorize]
    [HttpPost("{subscriptionId:guid}/claim")]
    public async Task<ActionResult<CalendarSubscriptionDto>> ClaimCalendarSubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId is null) return Unauthorized();
        try
        {
            var subscription = await service.ClaimAsync(subscriptionId.ToString(), userId, cancellationToken);
            return subscription is null ? NotFound() : Ok(subscription.ToDto());
        }
        catch (MissingUserProfileException exception)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = exception.Message });
        }
    }

    private async Task TriggerCalendarGenerationAsync(string subscriptionId, CancellationToken cancellationToken)
    {
        try
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            await scheduler.TriggerJob(
                new JobKey(CalendarSubscriptionJobConstants.GenerateSingleJobName, CalendarSubscriptionJobConstants.GroupName),
                new JobDataMap { [CalendarSubscriptionJobConstants.SubscriptionIdJobDataKey] = subscriptionId },
                cancellationToken);
        }
        catch (Exception exception)
        {
            Log.FailedToTriggerCalendarGenerationJob(logger, subscriptionId, exception);
        }
    }
}
