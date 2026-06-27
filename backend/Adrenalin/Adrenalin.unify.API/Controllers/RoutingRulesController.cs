using Adrenalin.Modules.Ticketing.Application.Commands.Routing;
using Adrenalin.Modules.Ticketing.Application.DTOs.Routing;
using Adrenalin.Modules.Ticketing.Application.Queries.Routing;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/routing-rules")]
[Authorize]
[Tags("Routing Rules")]
public sealed class RoutingRulesController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public RoutingRulesController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Gets all routing rules, optionally filtered by company ID.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "ticket:manage")]
    [ProducesResponseType(typeof(IReadOnlyList<RoutingRuleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetRoutingRulesQuery(companyId), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets a routing rule by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ticket:manage")]
    [ProducesResponseType(typeof(RoutingRuleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetRoutingRuleByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Creates a new routing rule.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "ticket:manage")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRoutingRuleRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new CreateRoutingRuleCommand(
            req.CompanyId, req.GroupId, req.ModuleId, req.RegionCode,
            req.TierCode, req.Priority, req.TicketType, req.Keywords,
            req.RulePriority, req.IsDefault, actorId.Value);

        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Updates an existing routing rule.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ticket:manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoutingRuleRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new UpdateRoutingRuleCommand(
            id, req.GroupId, req.ModuleId, req.RegionCode,
            req.TierCode, req.Priority, req.TicketType, req.Keywords,
            req.RulePriority, req.IsDefault, actorId.Value);

        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { Message = "Routing rule updated successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Deletes a routing rule.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ticket:manage")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new DeleteRoutingRuleCommand(id, actorId.Value), ct);

        return result.IsSuccess
            ? Ok(new { Message = "Routing rule deleted successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets the audit history for a specific routing rule.
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [Authorize(Policy = "ticket:manage")]
    [ProducesResponseType(typeof(IReadOnlyList<RoutingRuleHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetRoutingRuleHistoryQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Simulates a routing rule evaluation without persisting a ticket.
    /// </summary>
    [HttpPost("simulate")]
    [Authorize(Policy = "ticket:manage")]
    [ProducesResponseType(typeof(RoutingSimulationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Simulate([FromBody] SimulateRoutingRequest req, CancellationToken ct)
    {
        var command = new SimulateRoutingCommand(
            req.CompanyId, req.ModuleId, req.Priority, req.Type, req.Title, req.Description);

        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error });
    }

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public sealed record CreateRoutingRuleRequest(
    Guid CompanyId,
    Guid GroupId,
    Guid? ModuleId = null,
    string? RegionCode = null,
    string? TierCode = null,
    TicketPriority? Priority = null,
    TicketType? TicketType = null,
    string? Keywords = null,
    int RulePriority = 100,
    bool IsDefault = false);

public sealed record UpdateRoutingRuleRequest(
    Guid GroupId,
    Guid? ModuleId = null,
    string? RegionCode = null,
    string? TierCode = null,
    TicketPriority? Priority = null,
    TicketType? TicketType = null,
    string? Keywords = null,
    int RulePriority = 100,
    bool IsDefault = false);

public sealed record SimulateRoutingRequest(
    Guid CompanyId,
    Guid? ModuleId = null,
    TicketPriority? Priority = null,
    TicketType? Type = null,
    string? Title = null,
    string? Description = null);
