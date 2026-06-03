using System;
using System.Net;

namespace Adrenalin.Modules.Auth.Domain.Entities;

/// <summary>
/// One row per device/login. Enables security dashboard with all active devices. last_active_at updated by API middleware on each authenticated call.
/// </summary>
public partial class UserSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? RefreshTokenId { get; set; }

    public string? DeviceName { get; set; }

    public IPAddress? IpAddress { get; set; }

    public string? GeoLocation { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime LastActiveAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public bool IsActive { get; set; }

    public virtual RefreshToken? RefreshToken { get; set; }

    public virtual User User { get; set; } = null!;
}
