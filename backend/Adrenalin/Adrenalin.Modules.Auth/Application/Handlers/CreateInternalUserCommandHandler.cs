using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Constants;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Exceptions;

namespace Adrenalin.Modules.Auth.Application.Handlers;

public class CreateInternalUserCommandHandler : IRequestHandler<CreateInternalUserCommand, Guid>
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

    private static readonly string[] AllowedInternalRoles = {
    "Admin", "Manager", "Agent", "Supervisor", "junior_agent", "senior_agent", "team_lead"
};

    public CreateInternalUserCommandHandler(
        IUserRepository users,
        IRoleRepository roles,
        IUserRoleRepository userRoles,
        IUserVerificationTokenRepository verificationTokens,
        IPasswordHasher passwordHasher,
        ITokenHasher tokenHasher,
        IPasswordGenerator PasswordGenerator,
        IEmailService emailService,
        ICurrentUserService currentUser)
    {
        _users = users;
        _roles = roles;
        _userRoles = userRoles;
        _verificationTokens = verificationTokens;
        _passwordHasher = passwordHasher;
        _tokenHasher = tokenHasher;
        _emailService = emailService;
        _PasswordGenerator = PasswordGenerator;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateInternalUserCommand request, CancellationToken cancellationToken)
    {
        var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
        if (existing is not null)
        {
            throw new Exception("Email already exists");
        }

        if (!AllowedInternalRoles.Any(r => r.Equals(request.RoleName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException("Invalid internal role");
        }

        var role = await _roles.GetByNameAsync(request.RoleName, cancellationToken);
        if (role is null)
        { 
            throw new Exception("Role not found");
        }

        var temporaryPassword = _PasswordGenerator.Generate();
        var passwordHash = _passwordHasher.Hash(temporaryPassword);

        // ✅ Fallback verification rule: If called via background EventBus worker thread context, 
        // fallback to a default system admin tracking key instead of blowing up.
        

        var user = User.Create(
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);

        Guid adminId = (_currentUser.IsAuthenticated && _currentUser.UserId is not null)
            ? _currentUser.UserId.Value
            : user.Id; // System GUID fallback

        await _users.AddAsync(user, cancellationToken);

        var userRole = UserRole.Assign(user.Id, role.Id, adminId);
        _userRoles.Add(userRole);

        var rawToken = Guid.NewGuid().ToString();
        var tokenHash = _tokenHasher.Hash(rawToken);

        var resetToken = new UserVerificationToken(
            user.Id,
            tokenHash,
            VerificationPurposes.PasswordReset,
            DateTimeOffset.UtcNow.AddHours(24));

        await _verificationTokens.AddAsync(resetToken, cancellationToken);

        var resetLink = $"http://localhost:4200/reset-password?token={rawToken}";

        await _emailService.SendAsync(
            user.Email,
            "Set Your Password",
            $"<h2>Welcome to Adrenalin</h2><p>Your account has been created.</p><p><a href='{resetLink}'>Click here to set your password</a></p><p>This link expires in 24 hours.</p>");

        return user.Id;
    }
}