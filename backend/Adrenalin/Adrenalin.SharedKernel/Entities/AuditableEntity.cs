namespace Adrenalin.SharedKernel.Entities;

public abstract class AuditableEntity : BaseEntity
{
<<<<<<< HEAD
    public Guid? CreatedBy { get; protected set; }

    public Guid? UpdatedBy { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset? UpdatedAt { get; protected set; }

    public byte[]? RowVersion { get; protected set; }
=======
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; protected set; }
        public Guid? CreatedBy { get; protected set; }
        public Guid? UpdatedBy { get; protected set; }
    }
>>>>>>> ragavendra
}