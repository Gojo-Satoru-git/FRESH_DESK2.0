using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{

    public sealed class UserVerificationToken :BaseEntity
    {
        public UserVerificationToken(
            Guid userId,
            string tokenHash,
            string purpose,
            DateTimeOffset expiresAt,
            string? createdByIp = null)
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
            UserId = userId;
            TokenHash = tokenHash;
            Purpose = purpose;
            ExpiresAt = expiresAt;
            CreatedByIp = createdByIp;
        }

        public bool IsExpired()
        {
            return ExpiresAt < DateTimeOffset.UtcNow;
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            VerifiedAt = DateTimeOffset.UtcNow;
        }
        
        public Guid UserId { get; private set; }

        public string TokenHash { get; private set; } = string.Empty;

        public string Purpose { get; private set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? VerifiedAt { get; private set; }

        public bool IsUsed { get; private set; }

        public string? CreatedByIp { get; private set; }
        public User User { get; private set; } = null!;

    }
}
