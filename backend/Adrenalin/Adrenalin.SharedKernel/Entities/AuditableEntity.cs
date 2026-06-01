

namespace Adrenalin.SharedKernel.Entities
{
    public abstract class AuditableEntity : BaseEntity
    {
        public bool IsActive { get; protected set; } = true;
    }
}