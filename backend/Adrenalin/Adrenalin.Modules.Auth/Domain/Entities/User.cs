using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// Core identity for all internal staff. Email uniqueness enforced case-insensitively. password_hash is bcrypt — never store plain. Soft-delete via is_deleted.
/// </summary>
public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string NormalizedEmail { get; set; } = null!;

    public string? Username { get; set; }

    public string? NormalizedUsername { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public bool EmailVerified { get; set; }

    public DateTime? EmailVerifiedAt { get; set; }

    public DateTime? PasswordChangedAt { get; set; }

    public bool IsActive { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public byte[]? RowVersion { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    public virtual ICollection<User> InverseCreatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();

    public virtual ICollection<TokenBlacklist> TokenBlacklists { get; set; } = new List<TokenBlacklist>();

    public virtual ICollection<UserVerificationToken> UserVerificationTokens { get; set; } = new List<UserVerificationToken>();

    public virtual ICollection<UserOtpCode> UserOtpCodes { get; set; } = new List<UserOtpCode>();

    public virtual ICollection<UserGroup> UserGroupCreatedByNavigations { get; set; } = new List<UserGroup>();

    public virtual ICollection<UserGroup> UserGroupUsers { get; set; } = new List<UserGroup>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
