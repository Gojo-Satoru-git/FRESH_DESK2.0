using System;
using System.Collections.Generic;
using Adrenalin.unify.API.Models.Lookup;

namespace Adrenalin.unify.API.Models.AuthModels;

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

    // Self-referencing audit relationships
    public virtual ICollection<User> InverseCreatedByNavigation { get; set; }
        = new List<User>();

    public virtual ICollection<User> InverseUpdatedByNavigation { get; set; }
        = new List<User>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual User? UpdatedByNavigation { get; set; }

    // Group relationships
    public virtual ICollection<Group> GroupCreatedByNavigations { get; set; }
        = new List<Group>();

    public virtual ICollection<Group> GroupUpdatedByNavigations { get; set; }
        = new List<Group>();

    // Module relationships
    public virtual ICollection<ProductModule> ModuleCreatedByNavigations { get; set; }
        = new List<ProductModule>();

    public virtual ICollection<ProductModule> ModuleUpdatedByNavigations { get; set; }
        = new List<ProductModule>();

    // Permission relationships
    public virtual ICollection<Permission> PermissionCreatedByNavigations { get; set; }
        = new List<Permission>();

    public virtual ICollection<Permission> PermissionUpdatedByNavigations { get; set; }
        = new List<Permission>();

    // ProductVersion relationships
    public virtual ICollection<ProductVersion> ProductVersionCreatedByNavigations { get; set; }
        = new List<ProductVersion>();

    public virtual ICollection<ProductVersion> ProductVersionUpdatedByNavigations { get; set; }
        = new List<ProductVersion>();

    // Refresh Tokens
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();

    // RolePermission relationships
    public virtual ICollection<RolePermission> RolePermissionCreatedByNavigations { get; set; }
        = new List<RolePermission>();

    public virtual ICollection<RolePermission> RolePermissionUpdatedByNavigations { get; set; }
        = new List<RolePermission>();

    // Role relationships
    public virtual ICollection<Role> RoleCreatedByNavigations { get; set; }
        = new List<Role>();

    public virtual ICollection<Role> RoleUpdatedByNavigations { get; set; }
        = new List<Role>();

    // SubModule relationships
    public virtual ICollection<SubModule> SubModuleCreatedByNavigations { get; set; }
        = new List<SubModule>();

    public virtual ICollection<SubModule> SubModuleUpdatedByNavigations { get; set; }
        = new List<SubModule>();

    // Token Blacklist
    public virtual ICollection<TokenBlacklist> TokenBlacklists { get; set; }
        = new List<TokenBlacklist>();

    // User Groups
    public virtual ICollection<UserGroup> UserGroupCreatedByNavigations { get; set; }
        = new List<UserGroup>();

    public virtual ICollection<UserGroup> UserGroups { get; set; }
        = new List<UserGroup>();

    // OTP Codes
    public virtual ICollection<UserOtpCode> UserOtpCodes { get; set; }
        = new List<UserOtpCode>();

    // User Roles
    public virtual ICollection<UserRole> UserRoleAssignedByNavigations { get; set; }
        = new List<UserRole>();

    public virtual ICollection<UserRole> UserRoleCreatedByNavigations { get; set; }
        = new List<UserRole>();

    public virtual ICollection<UserRole> UserRoleUpdatedByNavigations { get; set; }
        = new List<UserRole>();

    public virtual ICollection<UserRole> UserRoles { get; set; }
        = new List<UserRole>();

    // Sessions
    public virtual ICollection<UserSession> UserSessions { get; set; }
        = new List<UserSession>();

    // Verification Tokens
    public virtual ICollection<UserVerificationToken> UserVerificationTokens { get; set; }
        = new List<UserVerificationToken>();
}