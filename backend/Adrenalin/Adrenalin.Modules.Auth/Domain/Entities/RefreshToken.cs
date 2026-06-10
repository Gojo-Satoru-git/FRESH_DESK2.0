using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Adrenalin.Modules.Auth.Domain.Enums;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
public sealed class RefreshToken :BaseEntity
{
    public RefreshToken(
    Guid userId,
    string tokenHash,
    Guid familyId,
    DateTimeOffset expiresAt,
    string? deviceInfo = null,
    IPAddress? ipAddress = null)
{
    Id = Guid.NewGuid();
    CreatedAt = DateTimeOffset.UtcNow;
    UserId = userId;
    TokenHash = tokenHash;
    FamilyId = familyId;
    IssuedAt = DateTimeOffset.UtcNow;
    ExpiresAt = expiresAt;
    DeviceInfo = deviceInfo;
    IpAddress = ipAddress;
    IsRevoked = false;
}

    public DateTimeOffset CreatedAt { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public Guid FamilyId { get; private set; }

    public string? DeviceInfo { get; private set; }

  public IPAddress? IpAddress { get; private set; }
     public string? UserAgent { get; private set; }

    public DateTimeOffset IssuedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? LastUsedAt { get; private set; }

    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RotatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public RevocationReason? RevokedReason { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public User User { get; private set; } = null!;
    public RefreshToken? ReplacedByToken { get; private set; }

    public ICollection<RefreshToken> InverseReplacedByToken
    {
        get;
        private set;
    } = [];
    public ICollection<UserSession> UserSessions
    {
        get;
        private set;
    } = [];
    public void Revoke()
{
    IsRevoked = true;
    RevokedAt = DateTimeOffset.UtcNow;
}
}
}
