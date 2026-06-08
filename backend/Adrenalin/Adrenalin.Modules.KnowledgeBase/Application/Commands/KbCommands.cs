using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.KB.Application.Commands;

// ─── Folder Commands ──────────────────────────────────────────────────────────

public sealed record CreateKbFolderCommand(
    string Name,
    Guid? ParentId,
    int DisplayOrder,
    Guid? ActorId) : IRequest<Result<Guid>>;

public sealed record RenameKbFolderCommand(
    Guid FolderId,
    string NewName,
    Guid ActorId) : IRequest<Result>;

public sealed record ReorderKbFolderCommand(
    Guid FolderId,
    int NewDisplayOrder,
    Guid ActorId) : IRequest<Result>;

public sealed record DeleteKbFolderCommand(
    Guid FolderId,
    Guid ActorId) : IRequest<Result>;

// ─── Article Commands ─────────────────────────────────────────────────────────

public sealed record CreateKbArticleCommand(
    string Title,
    string? Content,
    ArticleType ArticleType,
    Guid? FolderId,
    Guid? ActorId) : IRequest<Result<Guid>>;

public sealed record UpdateKbArticleCommand(
    Guid ArticleId,
    string NewTitle,
    string? NewContent,
    Guid? ActorId) : IRequest<Result>;

public sealed record MoveKbArticleCommand(
    Guid ArticleId,
    Guid? TargetFolderId,
    Guid? ActorId) : IRequest<Result>;

public sealed record PublishKbArticleCommand(
    Guid ArticleId,
    Guid? ActorId) : IRequest<Result>;

public sealed record ArchiveKbArticleCommand(
    Guid ArticleId,
    Guid? ActorId) : IRequest<Result>;

public sealed record RestoreKbArticleToDraftCommand(
    Guid ArticleId,
    Guid? ActorId) : IRequest<Result>;

public sealed record DeleteKbArticleCommand(
    Guid ArticleId,
    Guid? ActorId) : IRequest<Result>;

// ─── Auto-resolve Commands ────────────────────────────────────────────────────

public sealed record EnableAutoResolveCommand(
    Guid ArticleId,
    IReadOnlyList<string> Keywords,
    string ResolutionText,
    decimal ConfidenceThreshold,
    Guid? ActorId) : IRequest<Result>;

public sealed record DisableAutoResolveCommand(
    Guid ArticleId,
    Guid? ActorId) : IRequest<Result>;

public sealed record MarkArticleAsGuardrailExcludedCommand(
    Guid ArticleId,
    Guid? ActorId) : IRequest<Result>;

/// <summary>
/// Called by the learning loop background job when a ticket
/// auto-resolved via this article is later reopened.
/// </summary>
public sealed record RecordArticleReopenedMatchCommand(
    Guid ArticleId,
    decimal ThresholdRaiseDelta = 0.025m) : IRequest<Result>;

/// <summary>Called by the auto-resolution engine after a successful match.</summary>
public sealed record RecordArticleMatchCommand(
    Guid ArticleId) : IRequest<Result>;

// ─── Attachment Commands ──────────────────────────────────────────────────────

/// <summary>
/// Carries the raw file upload. The handler calls <c>IKbFileStorageService.SaveAsync</c>
/// to persist the stream to the server and stores the resulting URL in the DB.
/// The caller (controller) is responsible for disposing the stream after dispatch.
/// </summary>
public sealed record AddAttachmentCommand(
    Guid ArticleId,
    string FileName,
    long? FileSizeBytes,
    string? MimeType,
    Stream FileStream) : IRequest<Result<Guid>>;

public sealed record RemoveAttachmentCommand(
    Guid ArticleId,
    Guid AttachmentId) : IRequest<Result>;

// ─── Portal Banner Commands ───────────────────────────────────────────────────

public sealed record CreatePortalBannerCommand(
    string Title,
    string Message,
    DateTimeOffset? ActiveFrom,
    DateTimeOffset? ActiveTo,
    Guid? ActorId) : IRequest<Result<Guid>>;

public sealed record UpdatePortalBannerCommand(
    Guid BannerId,
    string Title,
    string Message,
    DateTimeOffset? ActiveFrom,
    DateTimeOffset? ActiveTo,
    Guid? ActorId) : IRequest<Result>;

public sealed record ActivatePortalBannerCommand(
    Guid BannerId,
    Guid? ActorId) : IRequest<Result>;

public sealed record DeactivatePortalBannerCommand(
    Guid BannerId,
    Guid? ActorId) : IRequest<Result>;
