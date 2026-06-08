using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Domain.Entities;
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
    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokens,
        IUserRepository users,
        IJwtProvider jwtProvider,
        IRefreshTokenGenerator refreshTokenGenerator,
        ITokenHasher tokenHasher)
    {
        _refreshTokens = refreshTokens;
        _users = users;
        _jwtProvider = jwtProvider;
        _refreshTokenGenerator = refreshTokenGenerator;
        _tokenHasher = tokenHasher;
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
            throw new InvalidCredentialsException();
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
                DateTimeOffset.UtcNow.AddDays(7));

        await _refreshTokens.AddAsync(
            refreshTokenEntity,
            cancellationToken);

        await _refreshTokens.SaveChangesAsync(
            cancellationToken);

        return new LoginResponseDTO(
            accessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));
    }
    }
}