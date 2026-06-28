using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers.Auth;

[ApiController]
[Route("api/rbac/roles")]
[Produces("application/json")]
public sealed class RolesController : ControllerBase
{
    private readonly IDispatcher _dispatcher;
    public RolesController(IDispatcher dispatcher) => _dispatcher = dispatcher;

    [HttpGet]
    [Authorize(Policy = "role:read")]
    [ProducesResponseType(typeof(IReadOnlyList<RoleDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetAllRolesQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "role:read")]
    [ProducesResponseType(typeof(RoleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetRoleByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpGet("{id:guid}/permissions")]
    [Authorize(Policy = "role:read")]
    [ProducesResponseType(typeof(RoleWithPermissionsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWithPermissions(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetRoleWithPermissionsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "role:write")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new CreateRoleCommand(req.Name, req.Description, actorId.Value), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "role:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new UpdateRoleCommand(id, req.Name, req.Description, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "role:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new DeleteRoleCommand(id, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/permissions/grant")]
    [Authorize(Policy = "role:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GrantPermission(Guid id, [FromBody] PermissionIdRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new GrantPermissionToRoleCommand(id, req.PermissionId, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/permissions/revoke")]
    [Authorize(Policy = "role:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RevokePermission(Guid id, [FromBody] PermissionIdRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new RevokePermissionFromRoleCommand(id, req.PermissionId, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = "role:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetPermissions(Guid id, [FromBody] SetPermissionsRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new SetRolePermissionsCommand(id, req.PermissionIds, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    // FILE: Adrenalin/Adrenalin.unify.API/Controllers/RolesController.cs
    // EDIT EXISTING FILE — ADD these three endpoints inside the existing RolesController
    // class, e.g. right after the existing SetPermissions action. Do not remove anything
    // already there; this is additive only.

    /// <summary>FR-RP-023 / BR-RP-010 — clone an Access Level's entire Permission Matrix.</summary>
    [HttpPost("{id:guid}/clone")]
    [Authorize(Policy = "role:write")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Clone(Guid id, [FromBody] CloneRoleRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new CloneRoleCommand(id, req.NewRoleName, req.NewRoleDescription, actorId.Value), ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// FR-RP-033 — overwrites {id}'s permission set with {req.SourceRoleId}'s. Destructive;
    /// the FSD requires a confirmation step before this is called — that confirmation
    /// happens client-side (the UI shows "this will overwrite X's permissions, continue?"),
    /// this endpoint assumes the user already confirmed.
    /// </summary>
    [HttpPost("{id:guid}/copy-permissions-from")]
    [Authorize(Policy = "role:write")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CopyPermissionsFrom(Guid id, [FromBody] CopyPermissionsFromRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new CopyPermissionsFromRoleCommand(req.SourceRoleId, id, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    /// <summary>FR-RP-035 / FR-RP-043 — plain-English summary grouped by Module.</summary>
    [HttpGet("{id:guid}/effective-permissions-summary")]
    [Authorize(Policy = "role:read")]
    [ProducesResponseType(typeof(Adrenalin.Modules.Auth.Application.Queries.EffectivePermissionsSummaryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> EffectivePermissionsSummary(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new Adrenalin.Modules.Auth.Application.Queries.GetEffectivePermissionsPreviewQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

}

public sealed record CreateRoleRequest(string Name, string? Description);
public sealed record UpdateRoleRequest(string Name, string? Description);
public sealed record PermissionIdRequest(Guid PermissionId);
public sealed record SetPermissionsRequest(IReadOnlyList<Guid> PermissionIds);
public sealed record CloneRoleRequest(string NewRoleName, string? NewRoleDescription);
public sealed record CopyPermissionsFromRequest(Guid SourceRoleId);
