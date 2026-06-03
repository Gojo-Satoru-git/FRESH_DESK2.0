using System;
using System.Collections.Generic;
using System.Net;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// Stores hashed refresh tokens with family-based rotation tracking. On token reuse detection (possible theft), entire family_id is revoked immediately. token_hash is SHA-256 of raw token.
/// </summary>
public partial class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public Guid FamilyId { get; set; }

    public string? DeviceInfo { get; set; }

    public IPAddress? IpAddress { get; set; }

    public DateTime IssuedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? LastUsedAt { get; set; }

    public DateTime? RotatedAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedAt { get; set; }

    public Guid? ReplacedByTokenId { get; set; }

    public string? CreatedByIp { get; set; }

    public string? RevokedByIp { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<RefreshToken> InverseReplacedByToken { get; set; } = new List<RefreshToken>();

    public virtual RefreshToken? ReplacedByToken { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
}
