namespace Adrenalin.SharedKernel.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedBy { get; protected set; }

    public Guid? UpdatedBy { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset? UpdatedAt { get; protected set; }

    public byte[]? RowVersion { get; protected set; }
}