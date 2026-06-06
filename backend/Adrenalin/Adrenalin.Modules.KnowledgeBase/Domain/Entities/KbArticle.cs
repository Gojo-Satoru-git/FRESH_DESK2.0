using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.KnowledgeBase.Domain.Entities;

public sealed class KbArticle
{
    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string? Content { get; private set; }

    public string ArticleType { get; private set; } = null!;

    public string Status { get; private set; } = null!;

    public Guid? AuthorId { get; private set; }

    public Guid? FolderId { get; private set; }

    public bool IsPublished { get; private set; }

    public bool IsDeleted { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public bool AutoResolve { get; private set; }

    public decimal ConfidenceThreshold { get; private set; }

    public List<string>? Keywords { get; private set; }

    public string? ResolutionText { get; private set; }

    public bool GuardrailExcluded { get; private set; }

    public int TimesMatched { get; private set; }

    public int TimesReopened { get; private set; }

    public KbFolder? Folder { get; private set; }

    public ICollection<KbAttachment> KbAttachments { get; private set; } = new List<KbAttachment>();
}
