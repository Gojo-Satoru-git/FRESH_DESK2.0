using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class TicketComment : SoftDeleteEntity
{
    public Guid TicketId { get; private set; }
    public Guid? AuthorId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string Body { get; private set; } = null!;    
    public CommentVisibility Visibility { get; private set; }
    public Ticket Ticket { get; private set; } = null!;


    private readonly List<TicketAttachment> _attachments = new();
    public IReadOnlyCollection<TicketAttachment> Attachments => _attachments.AsReadOnly();

    public bool IsCustomerReply => ContactId.HasValue;

    private TicketComment() {}

    public void MoveToTicket(Guid destinationTicketId)
    {
        if (destinationTicketId == Guid.Empty)
            throw new TicketDomainException("Destination ticket ID cannot be empty.");
        TicketId = destinationTicketId;
    }

    private TicketComment(Guid ticketId, Guid? authorId, Guid? contactId, string body, CommentVisibility visibility)
    {
        TicketId = ticketId;
        AuthorId = authorId;
        ContactId = contactId;
        Body = body;
        Visibility = visibility;
    }

    public static TicketComment Create(Guid ticketId, Guid? authorId, Guid? contactId, string body, CommentVisibility visibility)
    {
        if (ticketId == Guid.Empty)
            throw new TicketDomainException("TicketId cannot be empty.");

        if (authorId == null && contactId == null)
            throw new TicketDomainException("Either AuthorId or ContactId must be provided.");

        if (authorId.HasValue && authorId.Value == Guid.Empty)
            throw new TicketDomainException("AuthorId cannot be empty Guid.");

        if (contactId.HasValue && contactId.Value == Guid.Empty)
            throw new TicketDomainException("ContactId cannot be empty Guid.");

        if (authorId.HasValue && contactId.HasValue)
            throw new TicketDomainException("Cannot set both AuthorId and ContactId.");

        if (contactId.HasValue && visibility == CommentVisibility.Internal)
            throw new TicketDomainException("Customer comments cannot be internal.");

        if (string.IsNullOrWhiteSpace(body))
            throw new TicketDomainException("Comment body cannot be empty.");

        if (body.Length > 10000)
            throw new TicketDomainException("Comment body exceeds 10000 characters.");

        var ticketComment = new TicketComment(
            ticketId,
            authorId,
            contactId,
            body.Trim(),
            visibility
        );

        return ticketComment;
    }
}