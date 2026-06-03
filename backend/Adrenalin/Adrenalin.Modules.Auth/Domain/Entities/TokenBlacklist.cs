using System;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// Revoked JWT IDs. Auth middleware performs O(1) lookup on jti before accepting any token. Rows pruned nightly: DELETE FROM auth.token_blacklist WHERE expires_at &lt; NOW().
/// </summary>
public partial class TokenBlacklist
{
    public Guid Id { get; set; }

    public string Jti { get; set; } = null!;

    public Guid UserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string? Reason { get; set; }

    public DateTime BlacklistedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
