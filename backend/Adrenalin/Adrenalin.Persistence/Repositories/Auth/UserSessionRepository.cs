using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories.Auth
{
    public class UserSessionRepository: IUserSessionRepository
    {
         private readonly AdrenalinDbContext _context;
        public UserSessionRepository(
        AdrenalinDbContext context)
    {
        _context = context;
    }
     public async Task AddAsync(
        UserSession session,
        CancellationToken cancellationToken)
    {
        await _context.UserSessions.AddAsync(
            session,
            cancellationToken);
    }
     public async Task<UserSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
    public async Task<UserSession?> GetByRefreshTokenIdAsync(
        Guid refreshTokenId,
        CancellationToken cancellationToken)
    {
        return await _context.UserSessions
            .FirstOrDefaultAsync(
                x => x.RefreshTokenId == refreshTokenId,
                cancellationToken);
    }
    public async Task<List<UserSession>>
        GetUserSessionsAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        return await _context.UserSessions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.LastActiveAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<UserSession>> GetActiveSessionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.UserSessions
            .Where(x => x.UserId == userId && x.IsActive)
            .OrderByDescending(x => x.LastActiveAt)
            .ToListAsync(cancellationToken);


    }
    public async Task<List<UserSession>> GetAllActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.UserSessions
            .Where(x => x.UserId == userId && x.IsActive)
            .OrderByDescending(x => x.LastActiveAt)
            .ToListAsync(cancellationToken);
    }
        public void Update(UserSession session)
        {
            _context.UserSessions.Update(session);
    }
    }
}