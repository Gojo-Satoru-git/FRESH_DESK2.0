// FILE: Adrenalin/Adrenalin.Modules.Auth/Domain/Entities/UserWorkflowRole.cs
// NEW FILE

using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// FS-05 §3.4 / FS-03 §3.2 — join entity carrying an agent's Primary Role and
/// zero-or-more Additional Roles. Exactly one row per (UserId, WorkflowRoleId) may have
/// IsPrimary = true at a time — enforced in the handler/domain service, not here,
/// since it requires seeing the user's full set of rows (cross-row invariant).
/// </summary>
public sealed class UserWorkflowRole : SoftDeleteEntity
{
    public Guid UserId { get; private set; }
    public Guid WorkflowRoleId { get; private set; }
    public bool IsPrimary { get; private set; }

    public WorkflowRole? WorkflowRole { get; private set; }

    private UserWorkflowRole() { } // EF Core

    public static UserWorkflowRole Create(Guid userId, Guid workflowRoleId, bool isPrimary, Guid actorId)
    {
        var assignment = new UserWorkflowRole
        {
            UserId = userId,
            WorkflowRoleId = workflowRoleId,
            IsPrimary = isPrimary,
        };
        assignment.CreatedBy = actorId;
        return assignment;
    }

    public void MakePrimary(Guid actorId)
    {
        IsPrimary = true;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MakeAdditional(Guid actorId)
    {
        IsPrimary = false;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid actorId)
    {
        Delete();
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
