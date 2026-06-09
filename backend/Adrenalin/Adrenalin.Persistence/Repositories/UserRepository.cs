using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AdrenalinDbContext _db;

    public UserRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _db.AddAsync(user, cancellationToken);
       
    }
    public async Task<List<string>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _db.UserRoles
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .Select(x => x.Role.Name)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _db.UserRoles
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .SelectMany(x => x.Role.RolePermissions)
            .Where(x => !x.IsDeleted)
            .Select(x =>
                x.Permission.Resource +
                ":" +
                x.Permission.Action)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
    public async Task<User?> GetByIdAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _db.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken);
    }
    public async Task<User?> GetWithRolesAsync(Guid id, CancellationToken ct)
       => await _db.Users
           .Include(u => u.UserRoles.Where(ur => !ur.IsDeleted))
               .ThenInclude(ur => ur.Role)
           .IgnoreQueryFilters()
           .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        string? emailQuery, bool? isActive, int pageNumber, int pageSize, CancellationToken ct)
    {
        var query = _db.Users.Where(u => !u.IsDeleted).AsQueryable();
        if (!string.IsNullOrWhiteSpace(emailQuery))
            query = query.Where(u => EF.Functions.ILike(u.Email, $"%{emailQuery}%"));
        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct)
        => await _db.UserRoles
            .Where(ur => ur.UserId == userId && !ur.IsDeleted && !ur.Role.IsDeleted)
            .SelectMany(ur => ur.Role.RolePermissions
                .Where(rp => !rp.IsDeleted && !rp.Permission.IsDeleted)
                .Select(rp => rp.Permission.Resource + ":" + rp.Permission.Action))
            .Distinct()
            .ToListAsync(ct);
}