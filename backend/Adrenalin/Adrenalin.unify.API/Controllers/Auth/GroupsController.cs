using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;
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
    [Authorize(Policy = "rbac:group:read")]
    [ProducesResponseType(typeof(IReadOnlyList<GroupDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetAllGroupsQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "rbac:group:read")]
    [ProducesResponseType(typeof(GroupDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetGroupByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("{id:guid}/members")]
    [Authorize(Policy = "rbac:group:read")]
    [ProducesResponseType(typeof(GroupWithMembersDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWithMembers(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetGroupWithMembersQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "rbac:group:manage")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateGroupRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(
            new CreateGroupCommand(req.Name, req.RegionCode, req.TierCode, req.UnattendedAlertMinutes, actorId.Value), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "rbac:group:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(
            new UpdateGroupCommand(id, req.Name, req.RegionCode, req.TierCode, req.UnattendedAlertMinutes, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "rbac:group:manage")]
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
    [Authorize(Policy = "rbac:group:manage")]
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
    [Authorize(Policy = "rbac:group:manage")]
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
    [Authorize(Policy = "rbac:group:manage")]
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

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public sealed record CreateGroupRequest(string Name, string? RegionCode, string? TierCode,
    int UnattendedAlertMinutes = 30);
public sealed record UpdateGroupRequest(string Name, string? RegionCode, string? TierCode,
    int UnattendedAlertMinutes = 30);
public sealed record AddMemberRequest(Guid UserId, bool IsLead = false);
public sealed record UserIdRequest(Guid UserId);
public sealed record SetLeadRequest(bool IsLead);
