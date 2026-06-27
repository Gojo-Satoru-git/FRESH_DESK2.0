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
    public class ForgotPasswordCommandHandler: IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly IUserRepository _users;
    private readonly IUserVerificationTokenRepository _tokens;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailService _emailService;
     public ForgotPasswordCommandHandler(
        IUserRepository users,
        IUserVerificationTokenRepository tokens,
        ITokenHasher tokenHasher,
        IEmailService emailService)
    {
        _users = users;
        _tokens = tokens;
        _tokenHasher = tokenHasher;
        _emailService = emailService;
    }
    public async Task<bool> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user =
            await _users.GetByEmailAsync(
                request.Email,
                cancellationToken);

        // Don't reveal whether email exists
        if (user is null)
        {
            return true;
        }
         var rawToken =
            Guid.NewGuid().ToString();

        var tokenHash =
            _tokenHasher.Hash(rawToken);

        var verificationToken =
            new UserVerificationToken(
                user.Id,
                tokenHash,
                VerificationPurposes.PasswordReset,
                DateTimeOffset.UtcNow.AddHours(1));

        await _tokens.AddAsync(
            verificationToken,
            cancellationToken);
         var resetUrl =
            $"http://localhost:4200/reset-password?token={rawToken}";

        await _emailService.SendAsync(
    user.Email,
    "Set Your Password",
    $@"
    <h2>Welcome to Adrenalin</h2>

    <p>Your account has been created.</p>

    <p>
        <a href='{resetUrl}'>
            Click here to set your password
        </a>
    </p>

    <p>This link expires in 24 hours.</p>
    ");

        return true;
    }
    }
}