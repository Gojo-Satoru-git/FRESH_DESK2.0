using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Application.DTOs;
using Adrenalin.Modules.KB.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/kb/banners")]
[Produces("application/json")]
public sealed class PortalBannersController : ControllerBase
{
    private readonly IDispatcher _sender;

    public PortalBannersController(IDispatcher sender) => _sender = sender;

    // ── GET /api/kb/banners/active ────────────────────────────────────────────
    // Customer-facing — no auth required

    /// <summary>Returns banners currently visible on the portal (schedule + is_active check).</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IReadOnlyList<ActivePortalBannerDto>), 200)]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var result = await _sender.Send(new GetActivePortalBannersQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ── GET /api/kb/banners ───────────────────────────────────────────────────
    // Admin only

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<PortalBannerDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAllPortalBannersQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ── GET /api/kb/banners/{id} ──────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PortalBannerDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetPortalBannerByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    // ── POST /api/kb/banners ──────────────────────────────────────────────────

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateBannerRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(
            new CreatePortalBannerCommand(
                req.Title, req.Message, req.ActiveFrom, req.ActiveTo, GetActorId()), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    // ── PUT /api/kb/banners/{id} ──────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateBannerRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdatePortalBannerCommand(
                id, req.Title, req.Message, req.ActiveFrom, req.ActiveTo, GetActorId()), ct);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/banners/{id}/activate ────────────────────────────────────

    [HttpPost("{id:guid}/activate")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new ActivatePortalBannerCommand(id, GetActorId()), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/banners/{id}/deactivate ──────────────────────────────────

    [HttpPost("{id:guid}/deactivate")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeactivatePortalBannerCommand(id, GetActorId()), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

// ── Request models ────────────────────────────────────────────────────────────

public sealed record CreateBannerRequest(
    string Title,
    string Message,
    DateTimeOffset? ActiveFrom,
    DateTimeOffset? ActiveTo);

public sealed record UpdateBannerRequest(
    string Title,
    string Message,
    DateTimeOffset? ActiveFrom,
    DateTimeOffset? ActiveTo);
