using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth;

public sealed class RolePermissionRepository : IRolePermissionRepository
{
    private readonly AdrenalinDbContext _db;
    public RolePermissionRepository(AdrenalinDbContext db) => _db = db;

    public async Task<RolePermission?> GetAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
        => await _db.RolePermissions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted, ct);

    public async Task<IReadOnlyList<RolePermission>> GetByRoleWithPermissionsAsync(Guid roleId, CancellationToken ct = default)
        => await _db.RolePermissions.Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted && !rp.Permission.IsDeleted)
            .ToListAsync(ct);

    public void Add(RolePermission rp) => _db.RolePermissions.Add(rp);
    public void Update(RolePermission rp) => _db.RolePermissions.Update(rp);

    public async Task SoftDeleteByRoleAsync(Guid roleId, Guid actorId, CancellationToken ct = default)
    {
        var rows = await _db.RolePermissions.Where(rp => rp.RoleId == roleId && !rp.IsDeleted).ToListAsync(ct);
        foreach (var rp in rows) rp.SoftDelete(actorId);
    }

    public async Task SoftDeleteByPermissionAsync(Guid permissionId, Guid actorId, CancellationToken ct = default)
    {
        var rows = await _db.RolePermissions.Where(rp => rp.PermissionId == permissionId && !rp.IsDeleted).ToListAsync(ct);
        foreach (var rp in rows) rp.SoftDelete(actorId);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
