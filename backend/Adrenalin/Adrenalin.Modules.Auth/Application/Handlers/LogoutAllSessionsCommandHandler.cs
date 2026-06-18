using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Domain.Enums;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class LogoutAllSessionsCommandHandler: IRequestHandler<LogoutAllSessionsCommand, bool>
    {
        private readonly IUserSessionRepository _sessions;
    private readonly IRefreshTokenRepository _refreshTokens;
     public LogoutAllSessionsCommandHandler(
        IUserSessionRepository sessions,
        IRefreshTokenRepository refreshTokens)
    {
        _sessions = sessions;
        _refreshTokens = refreshTokens;
    }
     public async Task<bool> Handle(
        LogoutAllSessionsCommand request,
        CancellationToken cancellationToken)
    {
        var sessions =
            await _sessions.GetAllActiveByUserAsync(
                request.UserId,
                cancellationToken);

        foreach (var session in sessions)
        {
            session.End();

            _sessions.Update(session);
        }
         var tokens =
            await _refreshTokens.GetByUserIdAsync(
                request.UserId,
                cancellationToken);

        foreach (var token in tokens)
        {
            if (!token.IsRevoked)
            {
                token.Revoke( RevocationReason.LogoutAll);

                await _refreshTokens.UpdateAsync(
                    token,
                    cancellationToken);
            }
        }

        return true;
    }
    
    }
}