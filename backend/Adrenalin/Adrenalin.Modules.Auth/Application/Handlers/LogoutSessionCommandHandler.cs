using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Enums;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class LogoutSessionCommandHandler: IRequestHandler<
        LogoutSessionCommand,
        bool>
    {
         private readonly IUserSessionRepository _sessions;
         private readonly ICurrentUserService _currentUser;
          private readonly IRefreshTokenRepository _refreshTokens;
    public LogoutSessionCommandHandler(
        IUserSessionRepository sessions,
        ICurrentUserService currentUser,
        IRefreshTokenRepository refreshTokens)
    {
        _sessions = sessions;
        _currentUser = currentUser;
        _refreshTokens = refreshTokens;
    }
    public async Task<bool> Handle(
        LogoutSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session =
            await _sessions.GetByIdAsync(
                request.SessionId,
                cancellationToken);

        if (session is null)
            return false;
        if (session.UserId != _currentUser.UserId)
{
    throw new UnauthorizedAccessException();
}
         if (session.RefreshTokenId.HasValue)
        {
            var token =
                await _refreshTokens.GetByIdAsync(
                    session.RefreshTokenId.Value,
                    cancellationToken);

            if (token is not null &&
                !token.IsRevoked)
            {
                token.Revoke(RevocationReason.SessionExpired );
                await _refreshTokens.UpdateAsync(
                    token,
                    cancellationToken);
            }
        }
        session.End();
        _sessions.Update(session);

        return true;
    }
    }
}