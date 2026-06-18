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
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly IUserRepository _users;

        private readonly IPasswordHasher _hasher;
        private readonly IUserVerificationTokenRepository _verificationTokens;
        private readonly ITokenHasher _tokenHasher;
        private readonly IEmailService _emailService;
        private readonly IUserOtpCodeRepository _otpRepository;
private readonly IOtpGenerator _otpGenerator;
        public RegisterUserCommandHandler(IUserRepository users, IPasswordHasher hasher, IUserVerificationTokenRepository verificationTokens, ITokenHasher tokenHasher,IUserOtpCodeRepository otpRepository,
IOtpGenerator otpGenerator,
IEmailService emailService)
        {
            _users = users;
            _hasher = hasher;
            _verificationTokens = verificationTokens;
            _tokenHasher = tokenHasher; 
            _emailService = emailService;
            _otpRepository = otpRepository;
_otpGenerator = otpGenerator;
        }
        public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            var existing = await _users.GetByEmailAsync(request.Email, cancellationToken);
            if (existing is not null)
            {
                throw new Exception("Email already exists");
            }

            var passwordHash = _hasher.Hash(request.Password);
            var user = User.Create(request.Email,
                passwordHash,
                request.FirstName,
                request.LastName,
                request.Username,
                request.Phone);
            await _users.AddAsync(user, cancellationToken);
            var otp =
    _otpGenerator.Generate();

var otpHash =
    _tokenHasher.Hash(otp);

var otpEntity =
    new UserOtpCode(
        user.Id,
        otpHash,
        OtpPurposes.EmailVerification,
        DateTimeOffset.UtcNow.AddMinutes(10),
        user.Email);

await _otpRepository.AddAsync(
    otpEntity,
    cancellationToken);

await _emailService.SendAsync(
    user.Email,
    "Email Verification OTP",
    $"Your OTP is {otp}");



            return user.Id;

        }
    

    }
}