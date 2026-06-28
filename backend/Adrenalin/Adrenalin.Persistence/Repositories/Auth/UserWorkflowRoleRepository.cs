// FILE: Adrenalin/Adrenalin.Persistence/Repositories/Auth/UserWorkflowRoleRepository.cs
// NEW FILE

using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth;

public sealed class UserWorkflowRoleRepository : IUserWorkflowRoleRepository
{
    private readonly AdrenalinDbContext _db;
    public UserWorkflowRoleRepository(AdrenalinDbContext db) => _db = db;

    public async Task<UserWorkflowRole?> GetAsync(Guid userId, Guid workflowRoleId, CancellationToken ct = default)
        => await _db.UserWorkflowRoles
            .FirstOrDefaultAsync(uwr => uwr.UserId == userId && uwr.WorkflowRoleId == workflowRoleId, ct);

    public async Task<IReadOnlyList<UserWorkflowRole>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.UserWorkflowRoles
            .Include(uwr => uwr.WorkflowRole)
            .Where(uwr => uwr.UserId == userId)
            .OrderByDescending(uwr => uwr.IsPrimary)
            .ToListAsync(ct);

    public async Task<UserWorkflowRole?> GetPrimaryAsync(Guid userId, CancellationToken ct = default)
        => await _db.UserWorkflowRoles
            .Include(uwr => uwr.WorkflowRole)
            .FirstOrDefaultAsync(uwr => uwr.UserId == userId && uwr.IsPrimary, ct);

    public async Task<IReadOnlyList<UserWorkflowRole>> GetByWorkflowRoleAsync(Guid workflowRoleId, CancellationToken ct = default)
        => await _db.UserWorkflowRoles
            .Where(uwr => uwr.WorkflowRoleId == workflowRoleId)
            .ToListAsync(ct);

    public void Add(UserWorkflowRole assignment) => _db.UserWorkflowRoles.Add(assignment);
    public void Update(UserWorkflowRole assignment) => _db.UserWorkflowRoles.Update(assignment);
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
