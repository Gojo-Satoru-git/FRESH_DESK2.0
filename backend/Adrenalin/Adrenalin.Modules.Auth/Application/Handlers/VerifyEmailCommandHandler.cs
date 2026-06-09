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
    public class VerifyEmailCommandHandler: IRequestHandler<VerifyEmailCommand, bool>
    {
         private readonly IUserVerificationTokenRepository _tokens;
    private readonly IUserRepository _users;
    private readonly ITokenHasher _tokenHasher;
    public VerifyEmailCommandHandler(
        IUserVerificationTokenRepository tokens,
        IUserRepository users,
        ITokenHasher tokenHasher)
    {
        _tokens = tokens;
        _users = users;
        _tokenHasher = tokenHasher;
    }
    public async Task<bool> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash =
            _tokenHasher.Hash(request.Token);

        var token =
            await _tokens.GetByHashAsync(
                tokenHash,
                cancellationToken);

        if (token is null)
            throw new Exception("Invalid token");

        if (token.IsUsed)
            throw new Exception("Token already used");
         if (token.IsExpired())
            throw new Exception("Token expired");

        if (token.Purpose != VerificationPurposes.EmailVerification)
            throw new Exception("Invalid token purpose");

        var user =
            await _users.GetByIdAsync(
                token.UserId,
                cancellationToken);

        if (user is null)
            throw new Exception("User not found");

        user.VerifyEmail();

        token.MarkAsUsed();
        

        return true;
    }
    }
}