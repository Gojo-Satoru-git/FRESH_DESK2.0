using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities.Email;

public sealed class EmailAliasRouting : ActiveSoftDeleteEntity
{
    public string EmailAddress { get; set; } = null!;
    public Guid GroupId { get; set; }
    public Guid? CompanyId { get; set; }
    public int Priority { get; set; }
}
