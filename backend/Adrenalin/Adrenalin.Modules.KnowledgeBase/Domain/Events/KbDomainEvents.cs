using Adrenalin.Modules.KB.Domain.Enums;
using MediatR;

namespace Adrenalin.Modules.KB.Domain.Events;

// ─── Folder ───────────────────────────────────────────────────────────────────

public sealed record KbFolderCreatedDomainEvent(
    Guid FolderId,
    Guid? ParentId,
    string FolderName) : INotification;

public sealed record KbFolderDeletedDomainEvent(
    Guid FolderId) : INotification;

// ─── Article ──────────────────────────────────────────────────────────────────

public sealed record KbArticleCreatedDomainEvent(
    Guid ArticleId,
    string Title,
    ArticleType ArticleType) : INotification;

/// <summary>
/// Fired on Draft → Published transition.
/// Downstream modules (AI engine) may listen to warm their candidate cache.
/// </summary>
public sealed record KbArticlePublishedDomainEvent(
    Guid ArticleId,
    string Title) : INotification;

public sealed record KbArticleDeletedDomainEvent(
    Guid ArticleId) : INotification;

/// <summary>
/// Fired by the learning loop when a ticket auto-resolved via this article is later reopened.
/// The confidence_threshold on the article is raised automatically.
/// </summary>
public sealed record KbArticleReopenRateUpdatedDomainEvent(
    Guid ArticleId,
    int TotalTimesMatched,
    int TotalTimesReopened,
    decimal NewConfidenceThreshold) : INotification;
