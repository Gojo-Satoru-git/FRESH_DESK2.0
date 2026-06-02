using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
public sealed class TokenBlacklist : BaseEntity
{
    public string Jti { get; private set; } = string.Empty;

    public Guid UserId { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public string? Reason { get; private set; }

    public DateTimeOffset BlacklistedAt { get; private set; }
    public User User { get; private set; } = null!;
}
}