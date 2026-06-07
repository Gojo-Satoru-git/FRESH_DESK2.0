using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetWithRolesAsync(Guid id, CancellationToken ct);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        string? emailQuery, bool? isActive, int pageNumber, int pageSize, CancellationToken ct);
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct);
    Task AddAsync(User user, CancellationToken cancellationToken);
    void Update(User user);
    Task<int> SaveChangesAsync(CancellationToken ct);
}