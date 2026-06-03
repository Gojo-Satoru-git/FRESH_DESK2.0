using System;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// Hashed OTP codes for email/phone verification and 2FA. failed_attempts incremented on wrong guess; is_used=true on successful verification. Expired rows purged by nightly cleanup job.
/// </summary>
public partial class UserOtpCode
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CodeHash { get; set; } = null!;

    public string Purpose { get; set; } = null!;

    public string? DeliveryTarget { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int FailedAttempts { get; set; }

    public bool IsUsed { get; set; }

    public virtual User User { get; set; } = null!;
}
