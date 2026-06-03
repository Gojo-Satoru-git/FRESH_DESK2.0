using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

/// <summary>
/// Self-referencing folder hierarchy. parent_id=NULL for root folders. Use WITH RECURSIVE CTE to retrieve full tree. Depth limit enforced in API layer.
/// </summary>
public partial class KbFolder
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid? ParentId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<KbFolder> InverseParent { get; set; } = new List<KbFolder>();

    public virtual ICollection<KbArticle> KbArticles { get; set; } = new List<KbArticle>();

    public virtual KbFolder? Parent { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }
}
