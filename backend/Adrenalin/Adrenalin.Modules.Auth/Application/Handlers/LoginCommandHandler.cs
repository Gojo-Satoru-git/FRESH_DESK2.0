using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, Guid>
{
    private readonly IUserRepository _users;

    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(
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
            throw new Exception(
                "Invalid email or password");
        }

        return user.Id;
    }
}