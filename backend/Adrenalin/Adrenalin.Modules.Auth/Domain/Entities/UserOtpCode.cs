using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
    public sealed class UserOtpCode :BaseEntity
    {
        public UserOtpCode(
    Guid userId,
    string codeHash,
    string purpose,
    DateTimeOffset expiresAt,
    string? deliveryTarget = null)
{
    UserId = userId;
    CodeHash = codeHash;
    Purpose = purpose;
    ExpiresAt = expiresAt;
    DeliveryTarget = deliveryTarget;
}
        public Guid UserId { get; private set; }

        public string CodeHash { get; private set; } = string.Empty;

        public string Purpose { get; private set; } = string.Empty;

        public string? DeliveryTarget { get; private set; }

        public DateTimeOffset ExpiresAt { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? VerifiedAt { get; private set; }

        public int FailedAttempts { get; private set; }

        public bool IsUsed { get; private set; }
        public User User { get; private set; } = null!;
        public void MarkUsed()
{
    IsUsed = true;
    VerifiedAt = DateTimeOffset.UtcNow;
}
public void IncrementFailedAttempts()
{
    FailedAttempts++;
}
    }
}
