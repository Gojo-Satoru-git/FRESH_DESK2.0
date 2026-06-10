using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Constants;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class CreateInternalUserCommandHandler: IRequestHandler<CreateInternalUserCommand, Guid>
    {
         private readonly IUserRepository _users;
    private readonly IRoleRepository _roles;
    private readonly IUserRoleRepository _userRoles;
    private readonly IUserVerificationTokenRepository _verificationTokens;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailService _emailService;
    private readonly IPasswordGenerator _PasswordGenerator;
    private readonly ICurrentUserService _currentUser;

    public CreateInternalUserCommandHandler(
        IUserRepository users,
        IRoleRepository roles,
        IUserRoleRepository userRoles,
        IUserVerificationTokenRepository verificationTokens,
        IPasswordHasher passwordHasher,
        ITokenHasher tokenHasher,
        IPasswordGenerator PasswordGenerator,
        IEmailService emailService,
         ICurrentUserService currentUser
       )

    {
        _users = users;
        _roles = roles;
        _userRoles = userRoles;
        _verificationTokens = verificationTokens;
        _passwordHasher = passwordHasher;
        _tokenHasher = tokenHasher;
        _emailService = emailService;
        _PasswordGenerator=PasswordGenerator;
        _currentUser = currentUser;
    }
    public async Task<Guid> Handle(
        CreateInternalUserCommand request,
        CancellationToken cancellationToken)
    {
        var existing =
            await _users.GetByEmailAsync(
                request.Email,
                cancellationToken);

        if (existing is not null)
        {
            throw new Exception(
                "Email already exists");
        }
        var role =
            await _roles.GetByIdAsync(
                request.RoleId,
                cancellationToken);

        if (role is null)
        {
            throw new Exception(
                "Role not found");
        }

        var temporaryPassword =
            _PasswordGenerator.Generate();

        var passwordHash =
            _passwordHasher.Hash(
                temporaryPassword);
        if (!_currentUser.IsAuthenticated ||
    _currentUser.UserId is null)
{
    throw new UnauthorizedAccessException(
        "Admin authentication required.");
}

var adminId = _currentUser.UserId.Value;
         var user =
            User.Create(
                request.Email,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone);

        await _users.AddAsync(
            user,
            cancellationToken);

        var userRole =
            UserRole.Assign(
                user.Id,
                role.Id,
                adminId);
         _userRoles.Add(userRole);

        var rawToken =
            Guid.NewGuid().ToString();

        var tokenHash =
            _tokenHasher.Hash(rawToken);

        var resetToken =
            new UserVerificationToken(
                user.Id,
                tokenHash,
                VerificationPurposes.PasswordReset,
                DateTimeOffset.UtcNow.AddHours(24));

        await _verificationTokens.AddAsync(
            resetToken,
            cancellationToken);
         var resetLink =
            $"http://localhost:5088/api/auth/reset-password?token={rawToken}";

        await _emailService.SendAsync(
            user.Email,
            "Set Your Password",
            resetLink);

        return user.Id;
    }
    }
}