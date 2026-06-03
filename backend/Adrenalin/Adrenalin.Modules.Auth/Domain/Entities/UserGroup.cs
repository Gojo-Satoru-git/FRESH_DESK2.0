using System;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public partial class UserGroup
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid GroupId { get; set; }

    public bool IsLead { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
