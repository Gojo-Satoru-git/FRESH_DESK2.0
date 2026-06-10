using Adrenalin.Modules.KB.Domain.Enums;
using Adrenalin.Modules.KB.Domain.Events;
using Adrenalin.Modules.KB.Domain.ValueObjects;
using Adrenalin.SharedKernel.Entities;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.KB.Domain.Entities;

/// <summary>
/// Aggregate root for a knowledge base article. Table: kb.kb_articles
/// Inherits SoftDeleteEntity → AuditableEntity → BaseEntity
///   giving: Id, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt, RowVersion, IsDeleted
/// Note: kb.kb_articles has no is_active column per schema.
/// </summary>
public sealed class KbArticle : SoftDeleteEntity
{
    public string Title { get; private set; } = default!;
    public string? Content { get; private set; }
    public ArticleType ArticleType { get; private set; }
    public ArticleStatus Status { get; private set; }
    public bool IsPublished { get; private set; }
    public Guid? AuthorId { get; private set; }
    public Guid? FolderId { get; private set; }

    // ── Auto-resolution (addendum v8) ─────────────────────────────────────────
    public bool AutoResolve { get; private set; }
    public decimal ConfidenceThresholdValue { get; private set; } = ConfidenceThreshold.Default;
    public string[]? Keywords { get; private set; }
    public string? ResolutionText { get; private set; }
    public bool GuardrailExcluded { get; private set; }
    public int TimesMatched { get; private set; }
    public int TimesReopened { get; private set; }

    // Value-object accessor — not mapped by EF
    public ConfidenceThreshold ConfidenceThreshold =>
        ConfidenceThreshold.Create(ConfidenceThresholdValue);

    public KbFolder? Folder { get; private set; }

    private readonly List<KbAttachment> _attachments = [];
    public IReadOnlyList<KbAttachment> Attachments => _attachments;

    private readonly List<INotification> _domainEvents = [];
    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

    private KbArticle() { }

    public static KbArticle Create(
        ArticleTitle title, string? content, ArticleType articleType,
        Guid? authorId, Guid? folderId, Guid? createdBy)
    {
        var now = DateTimeOffset.UtcNow;
        var article = new KbArticle
        {
            Id = Guid.NewGuid(),
            Title = title.Value,
            Content = content,
            ArticleType = articleType,
            Status = ArticleStatus.Draft,
            IsPublished = false,
            AuthorId = authorId,
            FolderId = folderId,
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = now,
            AutoResolve = false,
            ConfidenceThresholdValue = ConfidenceThreshold.Default,
            Keywords = null,
            ResolutionText = null,
            GuardrailExcluded = false,
            TimesMatched = 0,
            TimesReopened = 0
        };
        article._domainEvents.Add(
            new KbArticleCreatedDomainEvent(article.Id, article.Title, article.ArticleType));
        return article;
    }

    public void UpdateContent(ArticleTitle newTitle, string? newContent, Guid? updatedBy)
    {
        EnsureNotDeleted();
        if (Status == ArticleStatus.Archived)
            throw new InvalidOperationException("Cannot edit an archived article. Restore it to Draft first.");
        Title = newTitle.Value;
        Content = newContent;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MoveToFolder(Guid? newFolderId, Guid? updatedBy)
    {
        EnsureNotDeleted();
        FolderId = newFolderId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish(Guid? updatedBy)
    {
        EnsureNotDeleted();
        if (Status == ArticleStatus.Published)
            throw new InvalidOperationException("Article is already published.");
        if (Status == ArticleStatus.Archived)
            throw new InvalidOperationException("Cannot publish an archived article. Restore to Draft first.");
        if (AutoResolve)
        {
            if (Keywords == null || Keywords.Length == 0)
                throw new InvalidOperationException("Auto-resolve articles must have at least one keyword before publishing.");
            if (string.IsNullOrWhiteSpace(ResolutionText))
                throw new InvalidOperationException("Auto-resolve articles must have resolution_text before publishing.");
        }
        Status = ArticleStatus.Published;
        IsPublished = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new KbArticlePublishedDomainEvent(Id, Title));
    }

    public void Archive(Guid? updatedBy)
    {
        EnsureNotDeleted();
        if (Status == ArticleStatus.Archived)
            throw new InvalidOperationException("Article is already archived.");
        Status = ArticleStatus.Archived;
        IsPublished = false;
        AutoResolve = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RestoreToDraft(Guid? updatedBy)
    {
        if (Status != ArticleStatus.Archived)
            throw new InvalidOperationException("Only archived articles can be restored to Draft.");
        Status = ArticleStatus.Draft;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid? updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Article is already deleted.");
        IsDeleted = true;
        IsPublished = false;
        AutoResolve = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new KbArticleDeletedDomainEvent(Id));
    }

    public void EnableAutoResolve(string[] keywords, string resolutionText,
        ConfidenceThreshold threshold, Guid? updatedBy)
    {
        EnsureNotDeleted();
        if (GuardrailExcluded)
            throw new InvalidOperationException("Guardrail-excluded articles cannot be auto-resolved.");
        if (keywords == null || keywords.Length == 0)
            throw new ArgumentException("At least one keyword is required.", nameof(keywords));
        if (string.IsNullOrWhiteSpace(resolutionText))
            throw new ArgumentException("Resolution text cannot be blank.", nameof(resolutionText));
        AutoResolve = true;
        Keywords = keywords;
        ResolutionText = resolutionText.Trim();
        ConfidenceThresholdValue = threshold.Value;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void DisableAutoResolve(Guid? updatedBy)
    {
        AutoResolve = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsGuardrailExcluded(Guid? updatedBy)
    {
        GuardrailExcluded = true;
        AutoResolve = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordReopenedMatch(decimal thresholdRaiseDelta = 0.025m)
    {
        TimesReopened++;
        ConfidenceThresholdValue = ConfidenceThreshold.Raise(thresholdRaiseDelta).Value;
        UpdatedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new KbArticleReopenRateUpdatedDomainEvent(
            Id, TimesMatched, TimesReopened, ConfidenceThresholdValue));
    }

    public void RecordMatch() { TimesMatched++; UpdatedAt = DateTimeOffset.UtcNow; }

    public KbAttachment AddAttachment(string fileUrl, string fileName, long? fileSizeBytes, string? mimeType)
    {
        EnsureNotDeleted();
        var attachment = KbAttachment.Create(Id, fileUrl, fileName, fileSizeBytes, mimeType);
        _attachments.Add(attachment);
        return attachment;
    }

    public void RemoveAttachment(Guid attachmentId)
    {
        var attachment = _attachments.FirstOrDefault(a => a.Id == attachmentId)
            ?? throw new InvalidOperationException($"Attachment {attachmentId} not found on this article.");
        attachment.SoftDelete();
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureNotDeleted()
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot modify a deleted article.");
    }
}
