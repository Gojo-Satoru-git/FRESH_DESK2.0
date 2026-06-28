// FILE: Adrenalin/Adrenalin.Modules.Auth/Domain/Entities/WorkflowRole.cs
// NEW FILE

using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// FS-05 §3.1 — Workflow Role Master.
/// Determines which workflow stages an agent is eligible to be assigned tickets at.
/// Carries NO platform permissions. Entirely independent from Role (Access Level).
/// Do not cross-reference with Role/Permission in engine logic (BR-RP-004).
/// </summary>
public sealed class WorkflowRole : ActiveSoftDeleteEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    /// <summary>Informational only, per FS-05 "System Default" definition — not a lock.</summary>
    public bool IsSystemDefault { get; private set; }

    private readonly List<UserWorkflowRole> _userWorkflowRoles = new();
    public IReadOnlyCollection<UserWorkflowRole> UserWorkflowRoles => _userWorkflowRoles.AsReadOnly();

    private WorkflowRole() { } // EF Core

    public static WorkflowRole Create(string name, string? description, bool isSystemDefault, Guid actorId)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 80)
            throw new ArgumentException("Role Name must be 2-80 characters.", nameof(name));

        var role = new WorkflowRole
        {
            Name = name.Trim(),
            Description = description,
            IsSystemDefault = isSystemDefault,
        };
        role.CreatedBy = actorId;
        return role;
    }

    /// <summary>FR-RP-003 — name/description editable at any time, regardless of System Default flag.</summary>
    public void Rename(string name, string? description, Guid actorId)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > 80)
            throw new ArgumentException("Role Name must be 2-80 characters.", nameof(name));

        Name = name.Trim();
        Description = description;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>FR-RP-004 — hides from pickers; existing assignments are preserved (BR-RP-009).</summary>
    public void Deactivate(Guid actorId)
    {
        Deactivate();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>FR-RP-005 — immediately restores visibility in all pickers.</summary>
    public void Reactivate(Guid actorId)
    {
        Activate();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// FR-RP-006 / BR-RP-003 — caller (handler) must verify zero active assignments and
    /// zero stage references BEFORE calling this. This method only enforces the soft-delete
    /// invariant itself; it does not re-check usage, since usage spans other aggregates
    /// (UserWorkflowRole, Workflow stage config) that this entity cannot see.
    /// </summary>
    public void SoftDelete(Guid actorId)
    {
        Delete();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
