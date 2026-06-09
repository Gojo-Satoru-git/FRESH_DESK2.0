using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly ITokenHasher _tokenHasher;
        public LogoutCommandHandler(
          IRefreshTokenRepository refreshTokens,
          ITokenHasher tokenHasher)
        {
            _refreshTokens = refreshTokens;
            _tokenHasher = tokenHasher;
        }
        public async Task<bool> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken)
        {
            var tokenHash =
                _tokenHasher.Hash(
                    request.RefreshToken);

            var refreshToken =
                await _refreshTokens.GetByHashAsync(
                    tokenHash,
                    cancellationToken);

            if (refreshToken is null)
            {
                return true;
            }

            if (!refreshToken.IsRevoked)
            {
                refreshToken.Revoke();

                await _refreshTokens.UpdateAsync(
                    refreshToken,
                    cancellationToken);

                
            }

            return true;
        }
    }
}