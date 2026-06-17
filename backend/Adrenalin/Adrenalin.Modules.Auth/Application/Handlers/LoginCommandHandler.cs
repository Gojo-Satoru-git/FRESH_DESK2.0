using System.Net;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Domain.Constants;
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
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider,
        IRefreshTokenRepository refreshTokens,
        IRefreshTokenGenerator refreshTokenGenerator,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork
    )
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
        _refreshTokens = refreshTokens;
        _refreshTokenGenerator = refreshTokenGenerator;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponseDTO> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);
 
        Console.WriteLine($"EMAIL: {request.Email}");
        Console.WriteLine($"USER FOUND: {user != null}");

        if (user is null)
            throw new Exception("Invalid email or password");

        Console.WriteLine($"DB HASH: {user.PasswordHash}");
       if (user.IsLockedOut())
{
  var remaining =
        user.LockoutEnd!.Value - DateTimeOffset.UtcNow;

    throw new ValidationException(
        $"Account locked. Try again in {(int)Math.Ceiling(remaining.TotalMinutes)} minute(s).");
}
        var isValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

       if (!isValid)
{
    user.RecordFailedLogin();
      await _unitOfWork.SaveChangesAsync(
        cancellationToken);
    Console.WriteLine(
    $"Attempts: {user.FailedLoginAttempts}");
    throw new UnauthorizedAccessException(
        "Invalid credentials");
}
user.RecordSuccessfulLogin();

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
           permissions,
           user.FirstName,
           user.LastName);
        var refreshToken =
     _refreshTokenGenerator.Generate();
     Console.WriteLine($"Refresh Token: {refreshToken}");

        var tokenHash =
            _tokenHasher.Hash(refreshToken);
       

        

        var refreshTokenEntity = new RefreshToken(
         user.Id,
         tokenHash,
         Guid.NewGuid(),                // familyId
         DateTimeOffset.UtcNow.AddDays(7),
          request.DeviceInfo,
         request.IpAddress);

        await _refreshTokens.AddAsync(
        refreshTokenEntity,
        cancellationToken);

       
        return new LoginResponseDTO(
        accessToken,
    refreshToken,
    DateTime.UtcNow.AddMinutes(15),
    DateTime.UtcNow.AddDays(7));
    }
}