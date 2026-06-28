// FILE: Adrenalin/Adrenalin.unify.API/Controllers/WorkflowRolesController.cs
// NEW FILE
//
// IMPORTANT — authorization policy names below ("workflowrole:read", "workflowrole:write",
// "workflowrole:delete") are SUGGESTED strings following the resource:action convention
// confirmed in RolesController.cs (which uses "role:read"/"role:write" for the equivalent
// Access Level actions). Per backend-permission-audit.md, policy strings have drifted from
// seed data before in this codebase — confirm before relying on this:
//   SELECT resource, action FROM auth.permissions WHERE resource = 'workflowrole';
// and INSERT the rows if missing (see 00-README.md section E for the exact INSERT).

using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/workflow-roles")]
public sealed class WorkflowRolesController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    public WorkflowRolesController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpGet]
    [Authorize(Policy = "workflowrole:read")]
    [ProducesResponseType(typeof(IReadOnlyList<Adrenalin.Modules.Auth.Application.DTOs.WorkflowRoleDto>), 200)]
    public async Task<IActionResult> List([FromQuery] bool? isActive, [FromQuery] string? search, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetWorkflowRolesQuery(isActive, search), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "workflowrole:write")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateWorkflowRoleRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new CreateWorkflowRoleCommand(req.Name, req.Description, actorId.Value), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(List), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "workflowrole:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Rename(Guid id, [FromBody] RenameWorkflowRoleRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new RenameWorkflowRoleCommand(id, req.Name, req.Description, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "workflowrole:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new DeactivateWorkflowRoleCommand(id, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = "workflowrole:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new ReactivateWorkflowRoleCommand(id, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// FR-RP-006 — returns 409 with the blocking counts (not a generic error string) when
    /// the role is in use, so the frontend can render "12 agents, 3 stages reference this role".
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "workflowrole:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(409)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new DeleteWorkflowRoleCommand(id, actorId.Value), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });

        if (result.Value is not null)
        {
            return Conflict(new
            {
                message = "Cannot delete: this Workflow Role is currently in use.",
                assignedAgentCount = result.Value.AssignedAgentCount,
                referencingStageCount = result.Value.ReferencingStageCount,
            });
        }

        return NoContent();
    }

    /// <summary>FS-05 §7 / FS-03 §3.2 — sets an agent's Primary + Additional Workflow Roles.</summary>
    [HttpPut("assignments/{userId:guid}")]
    [Authorize(Policy = "workflowrole:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetAgentAssignments(
        Guid userId, [FromBody] SetAgentWorkflowRolesRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(
            new SetAgentWorkflowRolesCommand(userId, req.PrimaryWorkflowRoleId, req.AdditionalWorkflowRoleIds, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>FR-RP-044 — Agent Editor live "Stage Eligibility" preview.</summary>
    [HttpPost("stage-eligibility-preview")]
    [Authorize(Policy = "workflowrole:read")]
    [ProducesResponseType(typeof(StageEligibilitySummaryDto), 200)]
    public async Task<IActionResult> StageEligibilityPreview(
        [FromBody] StageEligibilityPreviewRequest req, CancellationToken ct)
    {
        var result = await _dispatcher.Send(
            new GetStageEligibilityPreviewQuery(req.PrimaryWorkflowRoleId, req.AdditionalWorkflowRoleIds), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public sealed record CreateWorkflowRoleRequest(string Name, string? Description);
public sealed record RenameWorkflowRoleRequest(string Name, string? Description);
public sealed record SetAgentWorkflowRolesRequest(Guid PrimaryWorkflowRoleId, IReadOnlyList<Guid> AdditionalWorkflowRoleIds);
public sealed record StageEligibilityPreviewRequest(Guid PrimaryWorkflowRoleId, IReadOnlyList<Guid> AdditionalWorkflowRoleIds);