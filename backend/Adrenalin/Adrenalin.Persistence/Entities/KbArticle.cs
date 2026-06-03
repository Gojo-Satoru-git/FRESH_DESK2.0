using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class KbArticle
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string ArticleType { get; set; } = null!;

    public string Status { get; set; } = null!;

    public Guid? AuthorId { get; set; }

    public Guid? FolderId { get; set; }

    public bool IsPublished { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// TRUE = this article is eligible for the auto-resolution engine. Engine only fires if confidence &gt; confidence_threshold AND guardrail_excluded = FALSE.
    /// </summary>
    public bool AutoResolve { get; set; }

    /// <summary>
    /// Minimum match confidence (0.85 default) required to trigger auto-resolve. Articles with high reopen rates should have this raised automatically by learning loop.
    /// </summary>
    public decimal ConfidenceThreshold { get; set; }

    /// <summary>
    /// Phase 1 (keyword match) trigger words. Stored as PostgreSQL text array. Example: ARRAY[&apos;forgot password&apos;, &apos;reset password&apos;, &apos;login failed&apos;].
    /// </summary>
    public List<string>? Keywords { get; set; }

    public string? ResolutionText { get; set; }

    /// <summary>
    /// TRUE = this article covers a guardrail topic (payroll, financial, legal/compliance). Auto-resolution engine NEVER fires for guardrail_excluded articles regardless of confidence.
    /// </summary>
    public bool GuardrailExcluded { get; set; }

    /// <summary>
    /// Learning loop counter: incremented each time this article is matched (auto-resolve attempted).
    /// </summary>
    public int TimesMatched { get; set; }

    /// <summary>
    /// Learning loop counter: incremented each time a ticket auto-resolved via this article is reopened. High reopen rate → confidence_threshold auto-raised by learning loop job.
    /// </summary>
    public int TimesReopened { get; set; }

    public virtual User? Author { get; set; }

    public virtual ICollection<AutoResolutionLog> AutoResolutionLogs { get; set; } = new List<AutoResolutionLog>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual KbFolder? Folder { get; set; }

    public virtual ICollection<KbAttachment> KbAttachments { get; set; } = new List<KbAttachment>();

    public virtual User? UpdatedByNavigation { get; set; }
}
