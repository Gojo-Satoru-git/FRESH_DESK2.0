// FILE: Adrenalin/Adrenalin.Modules.Auth/Domain/Interfaces/IWorkflowRoleRepository.cs
// NEW FILE

using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces;

public interface IWorkflowRoleRepository
{
    Task<WorkflowRole?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<WorkflowRole?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<WorkflowRole>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<WorkflowRole>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>FR-RP-007 — count of agents currently assigned this role (Primary or Additional).</summary>
    Task<int> CountAssignedAgentsAsync(Guid workflowRoleId, CancellationToken ct = default);

    /// <summary>
    /// FR-RP-006 / BR-RP-003 — must also check workflow stage "Allowed Roles" config (FS-02 owns
    /// that table; call out to IStageRoleReferenceChecker rather than reaching across modules here).
    /// </summary>
    Task<bool> HasAnyActiveAssignmentAsync(Guid workflowRoleId, CancellationToken ct = default);

    void Add(WorkflowRole role);
    void Update(WorkflowRole role);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
