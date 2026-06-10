using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Entities;

namespace Adrenalin.Modules.Auth.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
         Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken);

    Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
    Task UpdateAsync(
    RefreshToken refreshToken,
    CancellationToken cancellationToken);
    Task<List<RefreshToken>> GetByUserIdAsync(
    Guid userId,
    CancellationToken cancellationToken);
    }
    
}