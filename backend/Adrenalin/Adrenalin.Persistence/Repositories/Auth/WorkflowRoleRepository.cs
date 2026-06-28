// FILE: Adrenalin/Adrenalin.Persistence/Repositories/Auth/WorkflowRoleRepository.cs
// NEW FILE

using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth;

public sealed class WorkflowRoleRepository : IWorkflowRoleRepository
{
    private readonly AdrenalinDbContext _db;
    public WorkflowRoleRepository(AdrenalinDbContext db) => _db = db;

    public async Task<WorkflowRole?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.WorkflowRoles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<WorkflowRole?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _db.WorkflowRoles
            .FirstOrDefaultAsync(r => !r.IsDeleted && r.Name.ToLower() == name.ToLower(), ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.WorkflowRoles.AnyAsync(r => !r.IsDeleted && r.Name.ToLower() == name.ToLower(), ct);

    public async Task<IReadOnlyList<WorkflowRole>> GetAllAsync(CancellationToken ct = default)
        => await _db.WorkflowRoles.OrderBy(r => r.Name).ToListAsync(ct);

    public async Task<IReadOnlyList<WorkflowRole>> GetActiveAsync(CancellationToken ct = default)
        => await _db.WorkflowRoles.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync(ct);

    public async Task<int> CountAssignedAgentsAsync(Guid workflowRoleId, CancellationToken ct = default)
        => await _db.UserWorkflowRoles.CountAsync(uwr => uwr.WorkflowRoleId == workflowRoleId, ct);

    public async Task<bool> HasAnyActiveAssignmentAsync(Guid workflowRoleId, CancellationToken ct = default)
        => await _db.UserWorkflowRoles.AnyAsync(uwr => uwr.WorkflowRoleId == workflowRoleId, ct);

    public void Add(WorkflowRole role) => _db.WorkflowRoles.Add(role);
    public void Update(WorkflowRole role) => _db.WorkflowRoles.Update(role);
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
