

namespace Adrenalin.SharedKernel.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; protected set; }
        public Guid? CreatedBy { get; protected set; }
        public Guid? UpdatedBy { get; protected set; }
        public bool IsDeleted { get; protected set; } = false;
        public byte[]? RowVersion { get; protected set; } // EF Core concurrency token
    }
}