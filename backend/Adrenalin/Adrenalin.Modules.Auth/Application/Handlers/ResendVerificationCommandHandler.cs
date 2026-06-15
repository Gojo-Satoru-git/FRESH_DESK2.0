using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Constants;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand, bool>
    {
          private readonly IUserRepository _users;
    private readonly IUserVerificationTokenRepository _tokens;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailService _emailService;

    public ResendVerificationCommandHandler(
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
        ResendVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var user =
            await _users.GetByEmailAsync(
                request.Email,
                cancellationToken);

        if (user is null)
            return true;

        if (user.EmailVerified)
            return true;

        var rawToken =
            Guid.NewGuid().ToString();
        var tokenHash =
            _tokenHasher.Hash(rawToken);

        var verificationToken =
            new Domain.Entities.UserVerificationToken(
                user.Id,
                tokenHash,
                VerificationPurposes.EmailVerification,
                DateTimeOffset.UtcNow.AddHours(24));

        await _tokens.AddAsync(
            verificationToken,
            cancellationToken);

        var verifyUrl =
            $"http://localhost:5088/api/auth/verify-email?token={rawToken}";
       await _emailService.SendAsync(
    user.Email,
    "Verify Your Email",
    $@"
    <h2>Email Verification</h2>

    <p>
        <a href='{verifyUrl}'>
            Verify Email
        </a>
    </p>
    ");

        return true;
    }
    }
}