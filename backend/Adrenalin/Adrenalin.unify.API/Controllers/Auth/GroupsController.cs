using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;

using Adrenalin.Modules.Company.Application.Queries;
using Adrenalin.Modules.Company.Application.DTOs;
// Add these two lines (keep existing usings):
using Adrenalin.Modules.Ticketing.Application.Queries.Groups;
using Adrenalin.Modules.Ticketing.Application.Queries.Routing;
using Adrenalin.Modules.Ticketing.Application.DTOs.Groups;
using Adrenalin.Modules.Ticketing.Application.DTOs.Routing;

using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers.Auth;

[ApiController]
[Route("api/rbac/groups")]
[Produces("application/json")]
public sealed class GroupsController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    public GroupsController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpGet]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(IReadOnlyList<GroupDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetAllGroupsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(GroupDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetGroupByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }



    /// <summary>
    /// Returns the groups the calling user belongs to (any authenticated user — no admin required).
    /// </summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<GroupDto>), 200)]
    public async Task<IActionResult> GetMyGroups(CancellationToken ct)
    {
        var callerId = GetActorId();
        if (!callerId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new GetUserGroupsQuery(callerId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Returns members of a group the calling user belongs to.
    /// Admins/managers can query any group; agents/team_leads can only query their own group.
    /// </summary>
    [HttpGet("{id:guid}/my-members")]
    [Authorize]
    [ProducesResponseType(typeof(GroupWithMembersDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMyGroupMembers(Guid id, CancellationToken ct)
    {
        var callerId = GetActorId();
        if (!callerId.HasValue) return Unauthorized();

        // Verify the caller is actually a member of this group (unless admin)
        var callerRoles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value.ToLower()).ToHashSet();
        var isPrivileged = callerRoles.Contains("admin") || callerRoles.Contains("manager");

        if (!isPrivileged)
        {
            // Check the caller belongs to this group
            var membershipResult = await _dispatcher.Send(new GetUserGroupsQuery(callerId.Value), ct);
            if (!membershipResult.IsSuccess) return Forbid();
            var belongsToGroup = membershipResult.Value?.Any(g => g.Id == id) ?? false;
            if (!belongsToGroup) return Forbid();
        }

        var result = await _dispatcher.Send(new GetGroupWithMembersQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(
            new CreateGroupCommand(req.Name, req.RegionCode, req.TierCode, req.UnattendedAlertMinutes, req.AssignmentStrategy, req.FallbackGroupId, actorId.Value), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(
            new UpdateGroupCommand(id, req.Name, req.RegionCode, req.TierCode, req.UnattendedAlertMinutes, req.AssignmentStrategy, req.FallbackGroupId, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new DeleteGroupCommand(id, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/members/add")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(
            new AddUserToGroupCommand(req.UserId, id, req.IsLead, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/members/remove")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RemoveMember(Guid id, [FromBody] UserIdRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(
            new RemoveUserFromGroupCommand(req.UserId, id, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPatch("{id:guid}/members/{userId:guid}/lead")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetLead(Guid id, Guid userId, [FromBody] SetLeadRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(
            new SetGroupLeadCommand(userId, id, req.IsLead, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ───────────────────────────────────────────── Group Queues & Dashboards ────

    /// <summary>
    /// Gets the operational dashboard for a specific group (ticket counts, workloads).
    /// Requires group membership or admin role.
    /// </summary>
    [HttpGet("{id:guid}/dashboard")]
    [Authorize]
    [ProducesResponseType(typeof(GroupDashboardDto), 200)]
    public async Task<IActionResult> GetDashboard(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new GetGroupDashboardQuery(id, actorId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets the ticket queue for a specific group.
    /// QueueType: all, unassigned, assigned, overdue, critical.
    /// Requires group membership or admin role.
    /// </summary>
    [HttpGet("{id:guid}/queue")]
    [Authorize]
    [ProducesResponseType(typeof(GroupQueueResultDto), 200)]
    public async Task<IActionResult> GetQueue(
        Guid id,
        [FromQuery] string queueType = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new GetGroupDashboardQueueQuery(id, queueType, actorId.Value, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets the aggregated dashboard for all groups where the caller is a Team Lead.
    /// </summary>
    [HttpGet("lead-dashboard")]
    [Authorize]
    [ProducesResponseType(typeof(LeadDashboardDto), 200)]
    public async Task<IActionResult> GetLeadDashboard(CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new GetLeadDashboardQuery(actorId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ───────────────────────────────────────────── Group Members ───────────────────

    /// <summary>
    /// Gets all members of a group.
    /// Requires group.members.view permission.
    /// </summary>
    [HttpGet("{id:guid}/members")]
    [Authorize(Policy = "group.members.view")]
    [ProducesResponseType(typeof(IReadOnlyList<EnterpriseGroupMemberDto>), 200)]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetEnterpriseGroupMembersQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets all leaders of a group (Primary, Secondary, Regional, Escalation).
    /// Requires group.leads.view permission.
    /// </summary>
    [HttpGet("{id:guid}/leaders")]
    [Authorize(Policy = "group.leads.view")]
    [ProducesResponseType(typeof(IReadOnlyList<EnterpriseGroupLeaderDto>), 200)]
    public async Task<IActionResult> GetLeaders(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetEnterpriseGroupLeadersQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ───────────────────────────────────────────── Group Workload ──────────────────

    /// <summary>
    /// Gets routing preview for a group.
    /// </summary>
    [HttpGet("{id:guid}/routing-preview")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(GroupRoutingPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoutingPreview(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetGroupRoutingPreviewQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Gets agent workload statistics for a specific group.
    /// Requires ticket.workload.view permission.
    /// </summary>
    [HttpGet("{id:guid}/workload")]
    [Authorize(Policy = "ticket.workload.view")]
    [ProducesResponseType(typeof(IReadOnlyList<GroupAgentWorkloadDto>), 200)]
    public async Task<IActionResult> GetWorkload(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new GetGroupWorkloadQuery(id, actorId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ───────────────────────────────────────────── Companies Assigned ───────────

    /// <summary>
    /// Gets all companies assigned to this support group.
    /// </summary>
    [HttpGet("{id:guid}/companies")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyGroupDto>), 200)]
    public async Task<IActionResult> GetAssignedCompanies(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetGroupCompaniesQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public sealed record CreateGroupRequest(string Name, string? RegionCode, string? TierCode, int UnattendedAlertMinutes = 30, int AssignmentStrategy = 0, Guid? FallbackGroupId = null);
public sealed record UpdateGroupRequest(string Name, string? RegionCode, string? TierCode, int UnattendedAlertMinutes = 30, int AssignmentStrategy = 0, Guid? FallbackGroupId = null);
public sealed record AddMemberRequest(Guid UserId, bool IsLead = false);
public sealed record UserIdRequest(Guid UserId);
public sealed record SetLeadRequest(bool IsLead);
