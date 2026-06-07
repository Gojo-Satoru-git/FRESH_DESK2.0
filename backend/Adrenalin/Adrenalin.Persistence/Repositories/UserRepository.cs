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
        await _db.SaveChangesAsync(cancellationToken);
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
}