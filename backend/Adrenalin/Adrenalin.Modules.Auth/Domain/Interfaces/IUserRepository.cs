using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string Email, CancellationToken cancellationToken);
        Task AddAsync(User user, CancellationToken cancellationToken);
        Task<List<string>> GetUserRolesAsync(
            Guid userId,
            CancellationToken cancellationToken);
        Task<List<string>> GetUserPermissionsAsync(
            Guid userId,
            CancellationToken cancellationToken);
        Task<User?> GetByIdAsync(
            Guid userId,
            CancellationToken cancellationToken);

        // RBAC query methods
        Task<User?> GetWithRolesAsync(
            Guid userId,
            CancellationToken cancellationToken);
        Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
            string? emailQuery,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken);
        Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
            Guid userId,
            CancellationToken cancellationToken);
    }
}