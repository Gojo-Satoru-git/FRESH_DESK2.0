using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// Named roles: junior_agent, team_lead, manager, admin, collaborator, pmo. System roles (is_system_role=true) cannot be deleted via admin UI.
/// </summary>
public partial class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
