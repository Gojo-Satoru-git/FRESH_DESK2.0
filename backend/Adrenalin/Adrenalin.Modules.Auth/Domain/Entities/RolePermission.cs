using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
public sealed class RolePermission :SoftDeleteEntity
{
    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

   

    public Role Role { get; private set; } = null!;

    public Permission Permission { get; private set; } = null!;
}
}