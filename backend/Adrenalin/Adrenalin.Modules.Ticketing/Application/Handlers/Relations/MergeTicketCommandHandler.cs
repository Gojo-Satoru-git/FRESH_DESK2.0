using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Interfaces;
using Adrenalin.SharedKernel.Mediator;

namespace Adrenalin.Modules.Ticketing.Application.Handlers;

public sealed class MergeTicketCommandHandler : IRequestHandler<MergeTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MergeTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(MergeTicketCommand request, CancellationToken cancellationToken)
    {
        if (request.MasterTicketId == request.DuplicateTicketId)
        {
            throw new TicketDomainException("A ticket cannot be merged into itself.");
        }

        var masterTicket = await _ticketRepository.GetByIdAsync(request.MasterTicketId, cancellationToken);

        if(masterTicket is null)
        {
            throw new TicketDomainException("Master ticket not found.");
        }

        var duplicateTicket = await _ticketRepository.GetByIdAsync(request.DuplicateTicketId, cancellationToken);

        if(duplicateTicket is null)
        {
            throw new TicketDomainException("Duplicate ticket not found.");
        }

        if (masterTicket.CompanyId != duplicateTicket.CompanyId)
        {
            throw new TicketDomainException("Cannot merge tickets from different companies.");
        }

        var mergerCompanyId = await _ticketRepository.GetUserCompanyIdAsync(request.MergedBy, cancellationToken);
        
        if (mergerCompanyId != masterTicket.CompanyId)
        {
            throw new TicketDomainException("The user merging the tickets must belong to the same company as the tickets.");
        }

        if(masterTicket.Status == TicketStatus.Closed)
        {
            throw new TicketDomainException("Cannot merge into a closed ticket.");
        }

        if (duplicateTicket.Status == TicketStatus.Closed)
        {
            throw new TicketDomainException("Duplicate ticket is already closed.");
        }

        var relation = TicketRelation.Create(masterTicket.Id, duplicateTicket.Id, TicketRelationType.MergedInto);

        masterTicket.AddRelation(relation);

        duplicateTicket.MergeInto(request.MergedBy, masterTicket.TicketNumber ?? masterTicket.Id.ToString());

        var masterComment = TicketComment.Create(
            masterTicket.Id,
            request.MergedBy,
            null,
            $"Ticket {duplicateTicket.TicketNumber} was merged into this ticket.",
            CommentVisibility.Internal
        );

        masterTicket.AddComment(masterComment, request.MergedBy);

        var duplicateComment = TicketComment.Create(
            duplicateTicket.Id,
            request.MergedBy,
            null,
            $"Merged into ticket {masterTicket.TicketNumber}.",
            CommentVisibility.Internal
        );

        duplicateTicket.AddComment(duplicateComment, request.MergedBy);

        return relation.Id;
    }

}