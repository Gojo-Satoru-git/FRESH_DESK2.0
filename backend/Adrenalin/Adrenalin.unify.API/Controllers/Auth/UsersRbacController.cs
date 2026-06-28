using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers.Auth;

[ApiController]
[Route("api/rbac/users")]
[Produces("application/json")]
public sealed class UsersRbacController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    public UsersRbacController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpGet]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(PagedResultDto<UserSummaryDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? email = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _dispatcher.Send(new GetUsersQuery(email, isActive, pageNumber, pageSize), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}/roles")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(UserWithRolesDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWithRoles(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetUserWithRolesQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("{id:guid}/permissions")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), 200)]
    public async Task<IActionResult> GetEffectivePermissions(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetUserEffectivePermissionsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("{id:guid}/groups")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(typeof(IReadOnlyList<GroupDto>), 200)]
    public async Task<IActionResult> GetGroups(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetUserGroupsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/roles/assign")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] RoleIdRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new AssignRoleToUserCommand(id, req.RoleId, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/roles/remove")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RemoveRole(Guid id, [FromBody] RoleIdRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new RemoveRoleFromUserCommand(id, req.RoleId, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }
    [HttpPut("{id:guid}/access-level")]
    [Authorize(Policy = "user:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetAccessLevel(Guid id, [FromBody] SetRolesRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new SetUserAccessLevelCommand(id, req.AccessLevelId, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });

    }
    private Guid? GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

public sealed record RoleIdRequest(Guid RoleId);
public sealed record SetRolesRequest(Guid AccessLevelId);