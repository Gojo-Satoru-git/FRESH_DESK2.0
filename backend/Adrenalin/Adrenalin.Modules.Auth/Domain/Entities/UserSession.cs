using System.Net;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
    public sealed class UserSession : BaseEntity
    {
        public Guid UserId { get; private set; }

        public Guid? RefreshTokenId { get; private set; }

        public string? DeviceName { get; private set; }

        public IPAddress? IpAddress { get; private set; }

        public string? GeoLocation { get; private set; }

        public DateTimeOffset StartedAt { get; private set; }

        public DateTimeOffset LastActiveAt { get; private set; }

        public DateTimeOffset? EndedAt { get; private set; }

        public bool IsActive { get; private set; }
        public User User { get; private set; } = null!;

        public RefreshToken? RefreshToken { get; private set; }
        public static UserSession Start(
    Guid userId,
     Guid refreshTokenId,
    string? deviceName,
    IPAddress? ipAddress)
{
    return new UserSession
    {
        Id = Guid.NewGuid(),
        UserId = userId,
         RefreshTokenId = refreshTokenId,
        DeviceName = deviceName,
        IpAddress = ipAddress,
        StartedAt = DateTimeOffset.UtcNow,
        LastActiveAt = DateTimeOffset.UtcNow,
        IsActive = true
    };
}
public void SetRefreshToken(
    Guid refreshTokenId)
{
    RefreshTokenId = refreshTokenId;
}
public void UpdateActivity()
{
      if (!IsActive)
        return;
    LastActiveAt = DateTimeOffset.UtcNow;
}
public void End()
{
      if (!IsActive)
        return;
    IsActive = false;
    EndedAt = DateTimeOffset.UtcNow;
} 
    }
}
