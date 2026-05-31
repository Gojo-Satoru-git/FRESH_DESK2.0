using System;
using System.Collections.Generic;

namespace Adrenalin.unify.API.Models.AuthModels;

/// <summary>
/// Named roles: junior_agent, team_lead, manager, admin, collaborator, pmo.
/// System roles (IsSystemRole = true) cannot be deleted via admin UI.
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

    // Audit relationships
    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    // Role-Permission relationships
    public virtual ICollection<RolePermission> RolePermissions { get; set; }
        = new List<RolePermission>();

    // User-Role relationships
    public virtual ICollection<UserRole> UserRoles { get; set; }
        = new List<UserRole>();
}