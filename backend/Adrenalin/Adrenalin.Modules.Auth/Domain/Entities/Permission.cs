using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// Atomic resource:action pairs. Examples: ticket:assign, company:create, ticket:delete. Permissions are system-defined and should not change without a migration.
/// </summary>
public partial class Permission
{
    public Guid Id { get; set; }

    public string Resource { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual User? UpdatedByNavigation { get; set; }
}
