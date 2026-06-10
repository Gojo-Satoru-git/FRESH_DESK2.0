using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Exceptions;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Auth.Application.Handlers
{
    public class VerifyEmailOtpCommandHandler : IRequestHandler<VerifyEmailOtpCommand, bool>
    {
        private readonly IUserOtpCodeRepository _otpRepository;
    private readonly IUserRepository _users;
    private readonly ITokenHasher _tokenHasher;
    public VerifyEmailOtpCommandHandler(
        IUserOtpCodeRepository otpRepository,
        IUserRepository users,
        ITokenHasher tokenHasher)
    {
        _otpRepository = otpRepository;
        _users = users;
        _tokenHasher = tokenHasher;
    }
    public async Task<bool> Handle(
        VerifyEmailOtpCommand request,
        CancellationToken cancellationToken)
    {
        var otpHash =
            _tokenHasher.Hash(
                request.Otp);

        var otp =
            await _otpRepository.GetByHashAsync(
                otpHash,
                cancellationToken);

        if (otp is null)
            throw new InvalidCredentialsException();

        if (otp.IsUsed)
            throw new InvalidCredentialsException();
        if (otp.ExpiresAt <
            DateTimeOffset.UtcNow)
            throw new InvalidCredentialsException();

        var user =
            await _users.GetByIdAsync(
                otp.UserId,
                cancellationToken);

        if (user is null)
            throw new InvalidCredentialsException();

        user.VerifyEmail();

        otp.MarkUsed();

        return true;
    }
    }
}