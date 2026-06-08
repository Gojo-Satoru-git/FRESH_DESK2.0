using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly AdrenalinDbContext _db;
    public UserRoleRepository(AdrenalinDbContext db) => _db = db;

    public async Task<UserRole?> GetAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        => await _db.UserRoles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && !ur.IsDeleted, ct);

    public async Task<UserRole?> GetIncludingDeletedAsync(Guid userId, Guid roleId, CancellationToken ct = default)
        => await _db.UserRoles.IgnoreQueryFilters()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

    public async Task<IReadOnlyList<UserRole>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.UserRoles.Include(ur => ur.Role).IgnoreQueryFilters()
            .Where(ur => ur.UserId == userId && !ur.IsDeleted).ToListAsync(ct);

    public void Add(UserRole ur) => _db.UserRoles.Add(ur);
    public void Update(UserRole ur) => _db.UserRoles.Update(ur);

    public async Task SoftDeleteByUserAsync(Guid userId, Guid actorId, CancellationToken ct = default)
    {
        var rows = await _db.UserRoles.IgnoreQueryFilters()
            .Where(ur => ur.UserId == userId && !ur.IsDeleted).ToListAsync(ct);
        foreach (var ur in rows) ur.SoftDelete(actorId);
    }

    public async Task SoftDeleteByRoleAsync(Guid roleId, Guid actorId, CancellationToken ct = default)
    {
        var rows = await _db.UserRoles.IgnoreQueryFilters()
            .Where(ur => ur.RoleId == roleId && !ur.IsDeleted).ToListAsync(ct);
        foreach (var ur in rows) ur.SoftDelete(actorId);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
