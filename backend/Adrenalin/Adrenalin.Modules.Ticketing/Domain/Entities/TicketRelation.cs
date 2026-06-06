using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

#pragma warning disable RCS1194 // Implement exception constructors
public class TicketDomainException : Exception
#pragma warning restore RCS1194 // Implement exception constructors
{
    public TicketDomainException(string message) : base(message) { }
}
public sealed class TicketRelation : BaseEntity
{
    public Guid ParentTicketId { get; private set; }
    public Guid ChildTicketId { get; private set; }
    public TicketRelationType RelationType { get; private set; }
    private TicketRelation() {}
    private TicketRelation(Guid parentTicketId, Guid childTicketId, TicketRelationType relationType)
    {
        ParentTicketId = parentTicketId;
        ChildTicketId = childTicketId;
        RelationType = relationType;
    }

    public static TicketRelation Create(Guid parentTicketId, Guid childTicketId, TicketRelationType relationType)
    {
        if (parentTicketId == Guid.Empty)
            throw new TicketDomainException("Parent ticket ID cannot be empty.");

        if (childTicketId == Guid.Empty)
            throw new TicketDomainException("Child ticket ID cannot be empty.");

        if (parentTicketId == childTicketId)
            throw new TicketDomainException("A ticket cannot establish a relation to itself.");

        if (!Enum.IsDefined(typeof(TicketRelationType), relationType))
            throw new TicketDomainException("Invalid ticket relation type provided.");

        return new TicketRelation(parentTicketId, childTicketId, relationType);
    }
}