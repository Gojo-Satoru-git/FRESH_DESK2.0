using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AdrenalinDbContext _db;
    public RoleRepository(AdrenalinDbContext db) => _db = db;

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Role?> GetWithPermissionsAsync(Guid id, CancellationToken ct = default)
        => await _db.Roles
            .Include(r => r.RolePermissions.Where(rp => !rp.IsDeleted))
                .ThenInclude(rp => rp.Permission)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
        => await _db.Roles.Where(r => !r.IsDeleted).OrderBy(r => r.Name).ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.Roles.AnyAsync(r => !r.IsDeleted && r.Name.ToLower() == name.ToLower(), ct);

    public void Add(Role role) => _db.Roles.Add(role);
    public void Update(Role role) => _db.Roles.Update(role);
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
