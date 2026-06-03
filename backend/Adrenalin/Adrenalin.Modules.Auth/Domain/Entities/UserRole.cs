using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
    public sealed class UserRole : SoftDeleteEntity
    {
        public Guid UserId { get; private set; }

        public Guid RoleId { get; private set; }

        public DateTimeOffset AssignedAt { get; private set; }

        public Guid? AssignedBy { get; private set; }

        public User User { get; private set; } = null!;

        public Role Role { get; private set; } = null!;
    }
}
