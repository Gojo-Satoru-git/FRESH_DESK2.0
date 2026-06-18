using Adrenalin.Modules.KB.Application.Commands;
using Adrenalin.Modules.KB.Application.DTOs;
using Adrenalin.Modules.KB.Application.Queries;
using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adrenalin.unify.API.Controllers;

[ApiController]
[Route("api/kb/articles")]
[Produces("application/json")]
public sealed class KbArticlesController : ControllerBase
{
    private readonly IDispatcher _sender;

    public KbArticlesController(IDispatcher sender) => _sender = sender;

    // ── GET /api/kb/articles ──────────────────────────────────────────────────

    /// <summary>
    /// Paginated search. Uses GIN trigram index on title when titleQuery is provided.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(KbArticleSearchResultDto), 200)]
    public async Task<IActionResult> Search(
        [FromQuery] string?       titleQuery  = null,
        [FromQuery] ArticleType?  articleType = null,
        [FromQuery] ArticleStatus? status     = null,
        [FromQuery] Guid?         folderId    = null,
        [FromQuery] int           pageNumber  = 1,
        [FromQuery] int           pageSize    = 20,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(
            new SearchKbArticlesQuery(titleQuery, articleType, status, folderId, pageNumber, pageSize), ct);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ── GET /api/kb/articles/{id} ─────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(KbArticleDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetKbArticleByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    // ── GET /api/kb/articles/{id}/attachments ─────────────────────────────────

    [HttpGet("{id:guid}/attachments")]
    [ProducesResponseType(typeof(KbArticleWithAttachmentsDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWithAttachments(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetKbArticleWithAttachmentsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error });
    }

    // ── GET /api/kb/articles/auto-resolve-candidates ──────────────────────────

    /// <summary>Returns all published articles eligible for auto-resolution (engine warm-up).</summary>
    [HttpGet("auto-resolve-candidates")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(typeof(IReadOnlyList<KbArticleDto>), 200)]
    public async Task<IActionResult> GetAutoResolveCandidates(CancellationToken ct)
    {
        var result = await _sender.Send(new GetAutoResolveCandidatesQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/articles ─────────────────────────────────────────────────

    [HttpPost]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create(
        [FromBody] CreateArticleRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(
            new CreateKbArticleCommand(req.Title, req.Content, req.ArticleType, req.FolderId, GetActorId()), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    // ── PUT /api/kb/articles/{id} ─────────────────────────────────────────────

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateArticleRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(
            new UpdateKbArticleCommand(id, req.NewTitle, req.NewContent, GetActorId()), ct);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── PUT /api/kb/articles/{id}/move ────────────────────────────────────────

    [HttpPut("{id:guid}/move")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Move(
        Guid id, [FromBody] MoveArticleRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(
            new MoveKbArticleCommand(id, req.TargetFolderId, GetActorId()), ct);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/articles/{id}/publish ───────────────────────────────────

    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new PublishKbArticleCommand(id, GetActorId()), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/articles/{id}/archive ───────────────────────────────────

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new ArchiveKbArticleCommand(id, GetActorId()), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/articles/{id}/restore-to-draft ───────────────────────────

    [HttpPost("{id:guid}/restore-to-draft")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RestoreToDraft(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new RestoreKbArticleToDraftCommand(id, GetActorId()), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── DELETE /api/kb/articles/{id} ─────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteKbArticleCommand(id, GetActorId()), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/articles/{id}/auto-resolve/enable ───────────────────────

    [HttpPost("{id:guid}/auto-resolve/enable")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> EnableAutoResolve(
        Guid id, [FromBody] EnableAutoResolveRequest req, CancellationToken ct)
    {
        var result = await _sender.Send(
            new EnableAutoResolveCommand(
                id, req.Keywords, req.ResolutionText,
                req.ConfidenceThreshold, GetActorId()), ct);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/articles/{id}/auto-resolve/disable ──────────────────────

    [HttpPost("{id:guid}/auto-resolve/disable")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DisableAutoResolve(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new DisableAutoResolveCommand(id, GetActorId()), ct);
        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/articles/{id}/guardrail-exclude ─────────────────────────

    [HttpPost("{id:guid}/guardrail-exclude")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GuardrailExclude(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(
            new MarkArticleAsGuardrailExcludedCommand(id, GetActorId()), ct);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error });
    }

    // ── POST /api/kb/articles/{id}/attachments ────────────────────────────────
    // Accepts multipart/form-data with a single "file" field.
    // The server saves the file under wwwroot/kb-attachments/{id}/ and
    // stores the resulting server-relative URL in the database.

    [HttpPost("{id:guid}/attachments")]
    [Authorize(Policy = "kb:manage")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    [RequestSizeLimit(52_428_800)] // 50 MB hard cap at the HTTP layer
    public async Task<IActionResult> AddAttachment(
        Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "No file was uploaded." });

        await using var stream = file.OpenReadStream();

        var result = await _sender.Send(
            new AddAttachmentCommand(
                id,
                file.FileName,
                file.Length,
                file.ContentType,
                stream), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetWithAttachments), new { id },
                new { attachmentId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    // ── DELETE /api/kb/articles/{id}/attachments/{attachmentId} ──────────────

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Policy = "kb:manage")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RemoveAttachment(
        Guid id, Guid attachmentId, CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveAttachmentCommand(id, attachmentId), ct);
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

public sealed record CreateArticleRequest(
    string Title,
    string? Content,
    ArticleType ArticleType,
    Guid? FolderId);

public sealed record UpdateArticleRequest(
    string NewTitle,
    string? NewContent);

public sealed record MoveArticleRequest(Guid? TargetFolderId);

public sealed record EnableAutoResolveRequest(
    IReadOnlyList<string> Keywords,
    string ResolutionText,
    decimal ConfidenceThreshold = 0.850m);

