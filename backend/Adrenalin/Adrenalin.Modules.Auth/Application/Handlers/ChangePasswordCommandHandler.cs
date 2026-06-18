using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Enums;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserSessionRepository _sessions;
    private readonly IRefreshTokenRepository _refreshTokens;
    public ChangePasswordCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IUserSessionRepository sessions,
        IRefreshTokenRepository refreshTokens)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _sessions = sessions;
        _refreshTokens = refreshTokens;
    }
    public async Task<bool> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user =
            await _users.GetByIdAsync(
                request.UserId,
                cancellationToken);

        if (user is null)
        {
            throw new ValidationException(
                "User not found");
        }
         var validPassword =
            _passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash);

        if (!validPassword)
        {
            throw new ValidationException(
                "Current password is incorrect");
        }
        var newHash =
            _passwordHasher.Hash(
                request.NewPassword);

        user.ChangePassword(newHash);
        var sessions =
            await _sessions.GetAllActiveByUserAsync(
                user.Id,
                cancellationToken);

        foreach (var session in sessions)
        {
            session.End();
            _sessions.Update(session);
        }
        var tokens =
            await _refreshTokens.GetByUserIdAsync(
                user.Id,
                cancellationToken);

        foreach (var token in tokens)
        {
            if (!token.IsRevoked)
            {
                token.Revoke(RevocationReason.PasswordChanged);

                await _refreshTokens.UpdateAsync(
                    token,
                    cancellationToken);
            }
        }

        return true;
    }
    }
}