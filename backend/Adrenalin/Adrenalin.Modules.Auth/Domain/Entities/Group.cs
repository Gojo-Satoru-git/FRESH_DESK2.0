using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities
{
public sealed class Group : SoftDeleteEntity
{
    public string Name { get; private set; } = string.Empty;

    public string? RegionCode { get; private set; }

    public string? TierCode { get; private set; }

    public int UnattendedAlertMinutes { get; private set; }

    public ICollection<UserGroup> UserGroups { get; private set; } = [];
}
}