using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Auth.Application.DTOs
{
    public sealed record LockedUserDto(
    Guid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    DateTimeOffset? LockoutEnd,
    int FailedLoginAttempts
);
}