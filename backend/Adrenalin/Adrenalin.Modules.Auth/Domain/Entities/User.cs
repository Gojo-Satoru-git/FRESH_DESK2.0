using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Constants;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
    public sealed class User : ActiveSoftDeleteEntity
    {
        public string Email { get; private set; } = string.Empty;

        public string NormalizedEmail { get; private set; } = string.Empty;

        public string? Username { get; private set; }

        public string? NormalizedUsername { get; private set; }

        public string PasswordHash { get; private set; } = string.Empty;

        public string? FirstName { get; private set; }

        public string? LastName { get; private set; }

        public string? Phone { get; private set; }

        public string? AvatarUrl { get; private set; }

        public bool EmailVerified { get; private set; }

        public DateTimeOffset? EmailVerifiedAt { get; private set; }

        public DateTimeOffset? PasswordChangedAt { get; private set; }

        public int FailedLoginAttempts { get; private set; }

        public DateTimeOffset? LockoutEnd { get; private set; }

        public DateTimeOffset? LastLoginAt { get; private set; }
        public DateTimeOffset? LastFailedLoginAt { get; private set; }

        public ICollection<UserRole> UserRoles { get; private set; } = [];

        public ICollection<UserGroup> UserGroups { get; private set; } = [];

        public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];

        public ICollection<UserSession> UserSessions { get; private set; } = [];

        public ICollection<UserOtpCode> UserOtpCodes { get; private set; } = [];

        public ICollection<UserVerificationToken> UserVerificationTokens { get; private set; } = [];
        public ICollection<TokenBlacklist> TokenBlacklists { get; private set; } = [];
        public static User Create(
                string email,
                string passwordHash,
                string firstName,
                string lastName,
                string? username,
                string? phone)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                NormalizedEmail =
                    email.ToUpperInvariant(),

                Username = username,

                NormalizedUsername =
                    username?.ToUpperInvariant(),

                PasswordHash = passwordHash,

                FirstName = firstName,

                LastName = lastName,
                Phone=phone,

                EmailVerified = false
            };
        }
        public void VerifyEmail()
{
    EmailVerified = true;
    EmailVerifiedAt = DateTimeOffset.UtcNow;
}
public void ChangePassword(
    string passwordHash)
{
    PasswordHash = passwordHash;
    PasswordChangedAt = DateTime.UtcNow;
}
public void RecordFailedLogin()
{
     if (LastFailedLoginAt.HasValue &&
        DateTimeOffset.UtcNow - LastFailedLoginAt.Value >
        TimeSpan.FromMinutes(AuthConstants.FailedAttemptWindowMinutes))
    {
        FailedLoginAttempts = 0;
    }
    FailedLoginAttempts++;
     LastFailedLoginAt = DateTimeOffset.UtcNow;

     if (FailedLoginAttempts >= AuthConstants.MaxFailedAttempts)
    {
        LockoutEnd =
    DateTimeOffset.UtcNow.AddHours(
        AuthConstants.LockoutHours);

        FailedLoginAttempts = 0;
    }
}
public void RecordSuccessfulLogin()
{
    FailedLoginAttempts = 0;
    LockoutEnd = null;
    LastLoginAt = DateTimeOffset.UtcNow;
}
public bool IsLockedOut()
{
    if (!LockoutEnd.HasValue)
        return false;

    if (LockoutEnd <= DateTimeOffset.UtcNow)
    {
        LockoutEnd = null;
        FailedLoginAttempts = 0;
        return false;
    }

    return true;
}
public void Unlock()
{
    FailedLoginAttempts = 0;
    LockoutEnd = null;
}
    }

}
