using System;
using System.Collections.Generic;

namespace Adrenalin.unify.API.Models.AuthModels;

/// <summary>
/// Hashed OTP codes for email/phone verification and 2FA.
/// FailedAttempts incremented on wrong guess.
/// IsUsed=true on successful verification.
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