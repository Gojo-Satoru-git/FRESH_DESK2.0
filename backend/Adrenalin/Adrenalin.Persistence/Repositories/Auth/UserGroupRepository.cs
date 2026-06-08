using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth;

public sealed class UserGroupRepository : IUserGroupRepository
{
    private readonly AdrenalinDbContext _db;
    public UserGroupRepository(AdrenalinDbContext db) => _db = db;

    public async Task<UserGroup?> GetAsync(Guid userId, Guid groupId, CancellationToken ct = default)
        => await _db.UserGroups.IgnoreQueryFilters()
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId && !ug.IsDeleted, ct);

    public async Task<UserGroup?> GetIncludingDeletedAsync(Guid userId, Guid groupId, CancellationToken ct = default)
        => await _db.UserGroups.IgnoreQueryFilters()
            .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId, ct);

    public async Task<IReadOnlyList<UserGroup>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.UserGroups.Include(ug => ug.Group).IgnoreQueryFilters()
            .Where(ug => ug.UserId == userId && !ug.IsDeleted).ToListAsync(ct);

    public async Task<IReadOnlyList<UserGroup>> GetByGroupAsync(Guid groupId, CancellationToken ct = default)
        => await _db.UserGroups.Include(ug => ug.User).IgnoreQueryFilters()
            .Where(ug => ug.GroupId == groupId && !ug.IsDeleted).ToListAsync(ct);

    public void Add(UserGroup ug) => _db.UserGroups.Add(ug);
    public void Update(UserGroup ug) => _db.UserGroups.Update(ug);

    public async Task SoftDeleteByGroupAsync(Guid groupId, Guid actorId, CancellationToken ct = default)
    {
        var rows = await _db.UserGroups.IgnoreQueryFilters()
            .Where(ug => ug.GroupId == groupId && !ug.IsDeleted).ToListAsync(ct);
        foreach (var ug in rows) ug.SoftDelete(actorId);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
