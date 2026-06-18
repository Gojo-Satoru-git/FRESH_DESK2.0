using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces
{
    public interface IUserSessionRepository
{
    Task AddAsync(
        UserSession session,
        CancellationToken cancellationToken);

    Task<UserSession?> GetByIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken);

    Task<List<UserSession>> GetActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<List<UserSession>> GetAllActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken);
    Task<UserSession?> GetByRefreshTokenIdAsync(
        Guid refreshTokenId,
        CancellationToken cancellationToken);
    Task<List<UserSession>> GetUserSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    void Update(UserSession session);
}
}