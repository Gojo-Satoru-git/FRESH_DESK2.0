using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
public sealed class Role : SoftDeleteEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public bool IsSystemRole { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = [];

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];
}
}
