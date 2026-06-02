using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
public sealed class UserGroup :SoftDeleteEntity
{
    public Guid UserId { get; private set; }

    public Guid GroupId { get; private set; }

    public bool IsLead { get; private set; }

    

    public User User { get; private set; } = null!;

    public Group Group { get; private set; } = null!;
}
}