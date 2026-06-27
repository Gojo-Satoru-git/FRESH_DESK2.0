using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Notifications;
using Adrenalin.Modules.Auth.Domain.Constants;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;


using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class CreateExternalUserCommandHandler : IRequestHandler<CreateExternalUserCommand, Guid>
    {
        private readonly IUserRepository _users;
    private readonly IUserRoleRepository _userRoles;
    private readonly IRoleRepository _roles;
    

    private readonly IPasswordHasher _passwordHasher;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IPublisher _publisher;
    private readonly IUserVerificationTokenRepository _tokens;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUser;
     public CreateExternalUserCommandHandler(
        IUserRepository users,
        IUserRoleRepository userRoles,
        IRoleRepository roles,
         IPublisher publisher,
        IPasswordHasher passwordHasher,
        IPasswordGenerator passwordGenerator,
        IUserVerificationTokenRepository tokens,
        ITokenHasher tokenHasher,
        IEmailService emailService,
         ICurrentUserService currentUser)

    {
        _users = users;
        _userRoles = userRoles;
        _roles = roles;
        _publisher = publisher;
        _passwordHasher = passwordHasher;
        _passwordGenerator = passwordGenerator;
        _tokens = tokens;
        _tokenHasher = tokenHasher;
        _emailService = emailService;
         _currentUser = currentUser;
    }
    
     public async Task<Guid> Handle(
        CreateExternalUserCommand request,
        CancellationToken cancellationToken)
    {
        var allowedRoles = new[]
{
    "Customer",
    "CustomerAdmin"
};
if(!allowedRoles.Contains(
    request.RoleName,
    StringComparer.OrdinalIgnoreCase))
{
    throw new Exception(
        "Invalid external role");
}
        var existing =
            await _users.GetByEmailAsync(
                request.Email,
                cancellationToken);

        if (existing is not null)
            throw new Exception(
                "Email already exists");
         var role =
             await _roles.GetByNameAsync(
        request.RoleName,
        cancellationToken);

        if (role is null)
            throw new Exception(
                "Customer role not found");
         var tempPassword =
            _passwordGenerator.Generate();

        var passwordHash =
            _passwordHasher.Hash(
                tempPassword);
        
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
        var assignedBy =
    _currentUser.UserId
    ?? throw new UnauthorizedAccessException();
         var userRole =
            UserRole.Assign(
                user.Id,
                role.Id,
                assignedBy);

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
         await _tokens.AddAsync(
            resetToken,
            cancellationToken);
        var resetLink =
    $"http://localhost:4200/reset-password?token={rawToken}";
        await _emailService.SendAsync(
    user.Email,
    "Set Your Password",
    $@"
    <h2>Welcome to Adrenalin</h2>

    <p>Your account has been created.</p>

    <p>
        <a href='{resetLink}'>
            Click here to set your password
        </a>
    </p>

    <p>This link expires in 24 hours.</p>
    ");
        await _publisher.Publish(
    new ExternalUserCreatedNotification(
        user.Id,
        request.CompanyId,
        user.Email,
        $"{request.FirstName} {request.LastName}",
        request.RoleName,
         assignedBy),
    cancellationToken);
    

        return user.Id;
    }
    }
}