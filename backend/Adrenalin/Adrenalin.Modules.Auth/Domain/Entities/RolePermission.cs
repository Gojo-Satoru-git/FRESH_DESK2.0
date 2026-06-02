using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public partial class RolePermission
{
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual User? UpdatedByNavigation { get; set; }
}