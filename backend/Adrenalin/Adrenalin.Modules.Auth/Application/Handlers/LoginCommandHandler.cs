using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, LoginResponseDTO>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<LoginResponseDTO> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null)
            throw new Exception("Invalid email or password");

        var isValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!isValid)
            throw new Exception("Invalid email or password");

        var permissions = await _users.GetEffectivePermissionsAsync(user.Id, cancellationToken);
        var roles = await _users.GetUserRolesAsync(user.Id, cancellationToken);

        var token = _jwtProvider.GenerateToken(user.Id, user.Email, roles, permissions);

        return new LoginResponseDTO(token, DateTime.UtcNow.AddHours(1));
    }
}