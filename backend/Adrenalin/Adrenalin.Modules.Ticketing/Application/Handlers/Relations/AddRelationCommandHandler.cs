using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using System.Threading;
using System.Threading.Tasks;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class AddRelationCommandHandler : IRequestHandler<AddRelationCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;

    public AddRelationCommandHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(AddRelationCommand request, CancellationToken cancellationToken)
    {
        var parentTicket = await _ticketRepository.GetByIdAsync(request.ParentTicketId, cancellationToken);
        if (parentTicket is null)
        {
            throw new TicketDomainException($"Parent ticket '{request.ParentTicketId}' was not found.");
        }

        var childTicket = await _ticketRepository.GetByIdAsync(request.ChildTicketId, cancellationToken);
        if (childTicket is null)
        {
            throw new TicketDomainException($"Child ticket '{request.ChildTicketId}' was not found.");
        }

        // Security Hardening: Ensure both tickets belong to the same company
        if (parentTicket.CompanyId != childTicket.CompanyId)
        {
            throw new TicketDomainException("Cannot relate tickets from different companies.");
        }

        // Circular ParentChild Check (commonly forgotten)
        if (request.RelationType == TicketRelationType.ParentChild || 
            request.RelationType == TicketRelationType.DependsOn || 
            request.RelationType == TicketRelationType.BlockedBy)
        {
            if (await WouldCreateCycleAsync(request.ParentTicketId, request.ChildTicketId, cancellationToken))
            {
                throw new TicketDomainException("Circular relation is not allowed.");
            }
        }

        var relation = TicketRelation.Create(
            request.ParentTicketId,
            request.ChildTicketId,
            request.RelationType);

        parentTicket.AddRelation(relation);

        return relation.Id;
    }

    private async Task<bool> WouldCreateCycleAsync(Guid parentId, Guid childId, CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid> { childId };
        var queue = new Queue<Guid>();
        queue.Enqueue(childId);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();

            var ticket = await _ticketRepository.GetByIdAsync(currentId, cancellationToken);
            if (ticket is not null)
            {
                foreach (var relation in ticket.TicketRelations)
                {
                    if (relation.ParentTicketId == currentId)
                    {
                        var nextId = relation.ChildTicketId;
                        if (nextId == parentId)
                        {
                            return true;
                        }
                        if (visited.Add(nextId))
                        {
                            queue.Enqueue(nextId);
                        }
                    }
                }
            }
        }

        return false;
    }
}
