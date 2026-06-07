using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth;

public sealed class GroupRepository : IGroupRepository
{
    private readonly AdrenalinDbContext _db;
    public GroupRepository(AdrenalinDbContext db) => _db = db;

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Groups.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<Group?> GetWithMembersAsync(Guid id, CancellationToken ct = default)
        => await _db.Groups
            .Include(g => g.UserGroups.Where(ug => !ug.IsDeleted)).ThenInclude(ug => ug.User)
            .IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<IReadOnlyList<Group>> GetAllAsync(CancellationToken ct = default)
        => await _db.Groups.Where(g => !g.IsDeleted && g.IsActive).OrderBy(g => g.Name).ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _db.Groups.AnyAsync(g => !g.IsDeleted && g.Name.ToLower() == name.ToLower(), ct);

    public void Add(Group g) => _db.Groups.Add(g);
    public void Update(Group g) => _db.Groups.Update(g);
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _db.SaveChangesAsync(ct);
}
