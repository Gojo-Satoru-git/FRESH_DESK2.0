using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class TokenBlacklist : BaseEntity
{
    private TokenBlacklist() { }

    public static TokenBlacklist Revoke(
        string jti,
        Guid userId,
        DateTimeOffset expiresAt,
        string? reason = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jti);

        return new TokenBlacklist
        {
            Jti = jti,
            UserId = userId,
            ExpiresAt = expiresAt,
            BlacklistedAt = DateTimeOffset.UtcNow,
            Reason = reason
        };
    }

    public string Jti { get; private set; } = null!;

    public Guid UserId { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset BlacklistedAt { get; private set; }

    public User User { get; private set; } = null!;
}
