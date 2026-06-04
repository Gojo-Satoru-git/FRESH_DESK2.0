using Adrenalin.Modules.KB.Domain.Enums;

namespace Adrenalin.Modules.KB.Application.DTOs;

// ─── Folder ───────────────────────────────────────────────────────────────────

public sealed record KbFolderDto(
    Guid Id,
    string Name,
    Guid? ParentId,
    int DisplayOrder,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>Recursive tree node returned by the GET /kb/folders/tree endpoint.</summary>
public sealed record KbFolderTreeNodeDto(
    Guid Id,
    string Name,
    Guid? ParentId,
    int DisplayOrder,
    IReadOnlyList<KbFolderTreeNodeDto> Children);

// ─── Article ──────────────────────────────────────────────────────────────────

public sealed record KbArticleDto(
    Guid Id,
    string Title,
    string? Content,
    ArticleType ArticleType,
    ArticleStatus Status,
    bool IsPublished,
    Guid? AuthorId,
    Guid? FolderId,
    // Auto-resolve
    bool AutoResolve,
    decimal ConfidenceThreshold,
    string[]? Keywords,
    string? ResolutionText,
    bool GuardrailExcluded,
    int TimesMatched,
    int TimesReopened,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>Lightweight card for lists and search results — no content body.</summary>
public sealed record KbArticleSummaryDto(
    Guid Id,
    string Title,
    ArticleType ArticleType,
    ArticleStatus Status,
    bool IsPublished,
    bool AutoResolve,
    bool GuardrailExcluded,
    Guid? FolderId,
    DateTimeOffset? UpdatedAt);

public sealed record KbArticleSearchResultDto(
    IReadOnlyList<KbArticleSummaryDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public sealed record KbAttachmentDto(
    Guid Id,
    Guid ArticleId,
    string FileUrl,
    string FileName,
    long? FileSizeBytes,
    string? MimeType,
    DateTimeOffset CreatedAt);

public sealed record KbArticleWithAttachmentsDto(
    KbArticleDto Article,
    IReadOnlyList<KbAttachmentDto> Attachments);

// ─── Portal Banner ────────────────────────────────────────────────────────────

public sealed record PortalBannerDto(
    Guid Id,
    string Title,
    string Message,
    DateTimeOffset? ActiveFrom,
    DateTimeOffset? ActiveTo,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

/// <summary>Minimal projection for the customer portal banner strip.</summary>
public sealed record ActivePortalBannerDto(
    Guid Id,
    string Title,
    string Message);
