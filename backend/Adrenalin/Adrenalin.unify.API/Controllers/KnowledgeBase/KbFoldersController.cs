using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Application.DTOs;
using Adrenalin.Modules.KB.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers;

/// <summary>
/// Manages the hierarchical KB folder tree.
///
/// All write endpoints require [Authorize] — ActorId is resolved from the JWT claim.
/// Replace HttpContext.User.GetUserId() with your ICurrentUserService once auth is ready.
/// </summary>
[ApiController]
[Route("api/kb/folders")]
[Produces("application/json")]
public sealed class KbFoldersController : ControllerBase
{
    private readonly IDispatcher _sender;

    public KbFoldersController(IDispatcher sender) => _sender = sender;

    // ── GET /api/kb/folders/tree ──────────────────────────────────────────────

    /// <summary>Returns the full recursive folder tree (root folders with nested children).</summary>
    [HttpGet("tree")]
    [ProducesResponseType(typeof(IReadOnlyList<KbFolderTreeNodeDto>), 200)]
    public async Task<IActionResult> GetTree(CancellationToken ct)
    {
        var result = await _sender.Send(new GetKbFolderTreeQuery(), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error });
    }

    // ── GET /api/kb/folders/{id} ──────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(KbFolderDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetKbFolderByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    // ── GET /api/kb/folders/{id}/children ─────────────────────────────────────

    [HttpGet("{id:guid}/children")]
    [ProducesResponseType(typeof(IReadOnlyList<KbFolderDto>), 200)]
    public async Task<IActionResult> GetChildren(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetKbFolderChildrenQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    // ── POST /api/kb/folders ──────────────────────────────────────────────────

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFolderRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        var result  = await _sender.Send(
            new CreateKbFolderCommand(req.Name, req.ParentId, req.DisplayOrder, actorId), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value },
                new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    // ── PUT /api/kb/folders/{id}/rename ───────────────────────────────────────

    [HttpPut("{id:guid}/rename")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Rename(
        Guid id, [FromBody] RenameFolderRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        //if (!actorId.HasValue) return Unauthorized();

        var result = await _sender.Send(
            new RenameKbFolderCommand(id, req.NewName, actorId.Value), ct);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── PUT /api/kb/folders/{id}/reorder ──────────────────────────────────────

    [HttpPut("{id:guid}/reorder")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Reorder(
        Guid id, [FromBody] ReorderFolderRequest req, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _sender.Send(
            new ReorderKbFolderCommand(id, req.NewDisplayOrder, actorId.Value), ct);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── DELETE /api/kb/folders/{id} ───────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _sender.Send(new DeleteKbFolderCommand(id, actorId.Value), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the authenticated user's Id from the JWT "sub" claim.
    /// Replace with ICurrentUserService.GetUserId() once auth module is wired in.
    /// </summary>
    private Guid? GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

// ── Request models ────────────────────────────────────────────────────────────

public sealed record CreateFolderRequest(
    string Name,
    Guid? ParentId,
    int DisplayOrder = 0);

public sealed record RenameFolderRequest(string NewName);
public sealed record ReorderFolderRequest(int NewDisplayOrder);
