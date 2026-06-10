using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Exceptions;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class ResetPasswordCommandHandler: IRequestHandler<ResetPasswordCommand,bool>
    {
         private readonly IUserVerificationTokenRepository _tokens;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHasher _tokenHasher;
    private readonly IRefreshTokenRepository _refreshTokens;

     public ResetPasswordCommandHandler(
        IUserVerificationTokenRepository tokens,
        IUserRepository users,
        IPasswordHasher passwordHasher,
        ITokenHasher tokenHasher,
        IRefreshTokenRepository refreshTokens)
    {
        _tokens = tokens;
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenHasher = tokenHasher;
        _refreshTokens = refreshTokens;
    }
    public async Task<bool> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash =
            _tokenHasher.Hash(request.Token);

        var token =
            await _tokens.GetByHashAsync(
                tokenHash,
                cancellationToken);

        if (token is null)
            throw new InvalidCredentialsException();
         if (token.IsUsed)
            throw new InvalidCredentialsException();

        if (token.ExpiresAt <
            DateTimeOffset.UtcNow)
            throw new InvalidCredentialsException();

        var user =
            await _users.GetByIdAsync(
                token.UserId,
                cancellationToken);

        if (user is null)
            throw new InvalidCredentialsException();
         var passwordHash =
            _passwordHasher.Hash(
                request.NewPassword);

        user.ChangePassword(passwordHash);

        token.MarkAsUsed();

        var refreshTokens =
            await _refreshTokens
                .GetByUserIdAsync(
                    user.Id,
                    cancellationToken);
        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.Revoke();
        }

        return true;
    }
    }
}