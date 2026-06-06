using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.KnowledgeBase.Domain.Entities;

public sealed class KbAttachment
{
    public Guid Id { get; private set; }

    public Guid ArticleId { get; private set; }

    public string FileUrl { get; private set; } = null!;

    public string FileName { get; private set; } = null!;

    public long? FileSizeBytes { get; private set; }

    public string? MimeType { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public KbArticle Article { get; private set; } = null!;
}
