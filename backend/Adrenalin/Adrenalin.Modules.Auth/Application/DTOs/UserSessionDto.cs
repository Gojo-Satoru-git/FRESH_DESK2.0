using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Auth.Application.DTOs
{
    public sealed record UserSessionDto(
    Guid SessionId,
    string? DeviceName,
    string? IpAddress,
    DateTimeOffset StartedAt,
    DateTimeOffset LastActiveAt,
    bool IsActive);
}