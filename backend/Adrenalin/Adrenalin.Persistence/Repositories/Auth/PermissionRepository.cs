using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth;

public sealed class PermissionRepository : IPermissionRepository
{
    private readonly AdrenalinDbContext _db;
    public PermissionRepository(AdrenalinDbContext db) => _db = db;

    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Permissions.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct = default)
        => await _db.Permissions.Where(p => !p.IsDeleted).OrderBy(p => p.Resource).ThenBy(p => p.Action).ToListAsync(ct);

    public async Task<bool> ExistsAsync(string resource, string action, CancellationToken ct = default)
        => await _db.Permissions.AnyAsync(p =>
            !p.IsDeleted && p.Resource == resource.ToLowerInvariant() && p.Action == action.ToLowerInvariant(), ct);

    public void Add(Permission p) => _db.Permissions.Add(p);
    public void Update(Permission p) => _db.Permissions.Update(p);
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
