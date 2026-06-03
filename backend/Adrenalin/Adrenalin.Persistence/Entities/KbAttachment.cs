using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class KbAttachment
{
    public Guid Id { get; set; }

    public Guid ArticleId { get; set; }

    public string FileUrl { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public long? FileSizeBytes { get; set; }

    public string? MimeType { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual KbArticle Article { get; set; } = null!;
}
