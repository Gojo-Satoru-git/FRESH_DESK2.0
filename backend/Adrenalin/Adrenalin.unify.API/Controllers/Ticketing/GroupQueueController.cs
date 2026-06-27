using Adrenalin.Modules.Ticketing.Application.DTOs.Groups;
using Adrenalin.Modules.Ticketing.Application.Queries.Groups;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers.Ticketing;

/// <summary>
/// Dispatch queue for groups — tickets that have a Group but no individual
/// agent yet (FactorBased auto-assignment is stubbed and intentionally fails;
/// LeastLoaded/Manual can also leave a ticket unassigned). Exposes TicketType
/// and Priority explicitly per FR for frontend triage views.
/// </summary>
[ApiController]
[Route("api/groups")]
[Produces("application/json")]
[Authorize]
public sealed class GroupQueueController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    public GroupQueueController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    /// <summary>
    /// The dispatch queue for one group. Caller must be a lead of that group
    /// (UserGroup.IsLead = true) or a platform administrator.
    /// </summary>
    [HttpGet("{groupId:guid}/queue")]
    [ProducesResponseType(typeof(GroupQueueSummaryDto), 200)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetQueue(Guid groupId, CancellationToken ct)
    {
        var callerId = GetActorId();
        if (!callerId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new GetGroupQueueQuery(groupId, callerId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : Forbid();
    }

    /// <summary>
    /// Combined queues across every group the calling user leads. A team lead
    /// can be IsLead = true on more than one group simultaneously — this
    /// returns one summary per group they lead, in a single call.
    /// </summary>
    [HttpGet("my-queues")]
    [ProducesResponseType(typeof(IReadOnlyList<GroupQueueSummaryDto>), 200)]
    public async Task<IActionResult> GetMyQueues(CancellationToken ct)
    {
        var callerId = GetActorId();
        if (!callerId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new GetMyLeadGroupQueuesQuery(callerId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    private Guid? GetActorId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}
