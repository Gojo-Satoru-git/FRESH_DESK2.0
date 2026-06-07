
using System.Net;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Exceptions;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponseDTO>
{
    private readonly IUserRepository _users;

    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    private readonly ITokenHasher _tokenHasher;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
    IRefreshTokenRepository refreshTokens,
    IRefreshTokenGenerator refreshTokenGenerator,
    ITokenHasher tokenHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _refreshTokens = refreshTokens;
        _refreshTokenGenerator = refreshTokenGenerator;
        _tokenHasher = tokenHasher;
    }

    public async Task<LoginResponseDTO> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user =
            await _users.GetByEmailAsync(
                request.Email,
                cancellationToken);

        if (user is null)
        {
            throw new Exception(
                "Invalid email or password");
        }

        var isValid =
            _passwordHasher.Verify(
                request.Password,
                user.PasswordHash);

        if (!isValid)
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
           user.Email, roles,
           permissions);
        var refreshToken =
     _refreshTokenGenerator.Generate();

        var tokenHash =
            _tokenHasher.Hash(refreshToken);
        string? ipAddress = request.IpAddress;

        
        var refreshTokenEntity = new RefreshToken(
         user.Id,
         tokenHash,
         Guid.NewGuid(),                // familyId
         DateTimeOffset.UtcNow.AddDays(7),
          request.DeviceInfo,
         ipAddress);

        await _refreshTokens.AddAsync(
        refreshTokenEntity,
        cancellationToken);

        await _refreshTokens.SaveChangesAsync(
            cancellationToken);
        return new LoginResponseDTO(
        accessToken,
    refreshToken,
    DateTime.UtcNow.AddMinutes(15),
    DateTime.UtcNow.AddDays(7));
    }
}