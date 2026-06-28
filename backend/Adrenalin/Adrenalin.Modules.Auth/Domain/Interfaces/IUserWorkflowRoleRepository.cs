// FILE: Adrenalin/Adrenalin.Modules.Auth/Domain/Interfaces/IUserWorkflowRoleRepository.cs
// NEW FILE

using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces;

public interface IUserWorkflowRoleRepository
{
    Task<UserWorkflowRole?> GetAsync(Guid userId, Guid workflowRoleId, CancellationToken ct = default);
    Task<IReadOnlyList<UserWorkflowRole>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<UserWorkflowRole?> GetPrimaryAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserWorkflowRole>> GetByWorkflowRoleAsync(Guid workflowRoleId, CancellationToken ct = default);

    void Add(UserWorkflowRole assignment);
    void Update(UserWorkflowRole assignment);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
