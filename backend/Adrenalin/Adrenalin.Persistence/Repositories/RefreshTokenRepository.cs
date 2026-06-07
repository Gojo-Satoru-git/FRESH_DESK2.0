using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;
namespace Adrenalin.Persistence.Repositories
{
    public sealed class RefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly AdrenalinDbContext _db;

    public RefreshTokenRepository(
        AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        await _db.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);
    }

    public async Task<RefreshToken?> GetByHashAsync(
        string tokenHash,
        CancellationToken cancellationToken)
    {
        return await _db.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(
            cancellationToken);
    }
    public Task UpdateAsync(
    RefreshToken refreshToken,
    CancellationToken cancellationToken)
{
    _db.RefreshTokens.Update(refreshToken);

    return Task.CompletedTask;
}
}
}