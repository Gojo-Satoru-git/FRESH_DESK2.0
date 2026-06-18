using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Domain.Constants;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Enums;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Exceptions;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand,LoginResponseDTO>
    {
         private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRepository _users;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    IUserSessionRepository _sessions;
    private readonly IUnitOfWork _unitOfWork;
    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        IJwtProvider jwtProvider,
        IRefreshTokenGenerator refreshTokenGenerator,
        ITokenHasher tokenHasher,
        IUserSessionRepository sessions,
         IUnitOfWork unitOfWork
        )
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _jwtProvider = jwtProvider;
        _refreshTokenGenerator = refreshTokenGenerator;
        _tokenHasher = tokenHasher;
        _sessions = sessions;
        _unitOfWork = unitOfWork;
    }
    private async Task HandleTokenReuseAsync(
    RefreshToken token,
    CancellationToken cancellationToken)
{
    var familyTokens =
        await _refreshTokens.GetByFamilyIdAsync(
            token.FamilyId,
            cancellationToken);

    foreach (var familyToken in familyTokens)
    {
        if (!familyToken.IsRevoked)
        {
            familyToken.Revoke( RevocationReason.TokenReuseDetected);

            await _refreshTokens.UpdateAsync(
                familyToken,
                cancellationToken);
        }
    }

    var sessions =
        await _sessions.GetAllActiveByUserAsync(
            token.UserId,
            cancellationToken);

    foreach (var session in sessions)
    {
        session.End();

        _sessions.Update(session);
    }
     await _unitOfWork.SaveChangesAsync(
        cancellationToken);
}
    public async Task<LoginResponseDTO> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash =
            _tokenHasher.Hash(
                request.RefreshToken);

        var storedToken =
            await _refreshTokens
                .GetByHashAsync(
                    tokenHash,
                    cancellationToken);

        if (storedToken is null)
        {
            throw new InvalidCredentialsException();
        }

        if (storedToken.IsRevoked)
        {
             await HandleTokenReuseAsync(
        storedToken,
        cancellationToken);

    throw new UnauthorizedAccessException(
        "Refresh token reuse detected. Login again.");
        }

        if (storedToken.ExpiresAt <
            DateTimeOffset.UtcNow)
        {
            throw new InvalidCredentialsException();
        }

        var user =
            await _users.GetByIdAsync(
                storedToken.UserId,
                cancellationToken);

        if (user is null)
        {
            throw new InvalidCredentialsException();
        }
        var session =
    await _sessions.GetByRefreshTokenIdAsync(
        storedToken.Id,
        cancellationToken);
        if (session is null)
{
   throw new InvalidCredentialsException();
}

if (!session.IsActive)
{
    throw new UnauthorizedAccessException(
        "Session expired");
}
if (DateTimeOffset.UtcNow - session.LastActiveAt >
    TimeSpan.FromMinutes(
        AuthConstants.SessionIdleTimeoutMinutes))
{
    session.End();

    storedToken.Revoke();

    await _refreshTokens.UpdateAsync(
        storedToken,
        cancellationToken);

    _sessions.Update(session);

    throw new UnauthorizedAccessException(
        "Session expired due to inactivity");
}
    session.UpdateActivity();
        var roles =
            await _users.GetUserRolesAsync(
                user.Id,
                cancellationToken);

        var permissions =
            await _users.GetUserPermissionsAsync(
                user.Id,
                cancellationToken);

        var accessToken =
            _jwtProvider.GenerateToken(
                user.Id,
                user.Email,
                roles,
                permissions,
                session.Id,
                user.FirstName,
                user.LastName);

        var newRefreshToken =
            _refreshTokenGenerator.Generate();

        var newTokenHash =
            _tokenHasher.Hash(
                newRefreshToken);

        storedToken.Revoke();

        await _refreshTokens.UpdateAsync(
            storedToken,
            cancellationToken);

        var refreshTokenEntity =
            new RefreshToken(
                user.Id,
                newTokenHash,
                storedToken.FamilyId,
                DateTimeOffset.UtcNow.AddDays(7),
                storedToken.DeviceInfo,
        storedToken.IpAddress,
        session.Id);

        await _refreshTokens.AddAsync(
            refreshTokenEntity,
            cancellationToken);
        session.SetRefreshToken(
    refreshTokenEntity.Id);

session.UpdateActivity();
_sessions.Update(session);


       

        return new LoginResponseDTO(
            accessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));
    }
    }
}