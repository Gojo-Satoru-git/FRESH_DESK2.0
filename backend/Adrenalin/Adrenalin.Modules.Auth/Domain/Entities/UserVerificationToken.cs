using System;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// Hashed URL tokens for email verification and password reset flows. is_used=true after first use (tokens are single-use). Expired rows purged by nightly cleanup job alongside refresh_tokens.
/// </summary>
public partial class UserVerificationToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string Purpose { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public bool IsUsed { get; set; }

    public string? CreatedByIp { get; set; }

    public virtual User User { get; set; } = null!;
}
