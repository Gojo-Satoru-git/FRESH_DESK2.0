using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.KnowledgeBase.Domain.Entities;

public sealed class KbFolder
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public Guid? ParentId { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsDeleted { get; private set; }

    public Guid? CreatedBy { get; private set; }

    public Guid? UpdatedBy { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public ICollection<KbFolder> InverseParent { get; private set; } = new List<KbFolder>();

    public ICollection<KbArticle> KbArticles { get; private set; } = new List<KbArticle>();

    public KbFolder? Parent { get; private set; }
}
