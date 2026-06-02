using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// Revoked JWT IDs.
/// Auth middleware performs O(1) lookup on Jti before accepting any token.
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