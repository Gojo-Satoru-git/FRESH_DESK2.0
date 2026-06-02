using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
public sealed class UserOtpCode : BaseEntity
{
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
}
}