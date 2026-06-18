using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using NSubstitute;
using Adrenalin.EventBus;
using Adrenalin.EventBus.Events;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Events;
using Adrenalin.Modules.Ticketing.Application.Handlers;

namespace Adrenalin.UnitTests.Ticketing.EmailToTicket;

public class EmailEventPublishingTests
{
    private readonly IEventBus _eventBus;

    public EmailEventPublishingTests()
    {
        _eventBus = Substitute.For<IEventBus>();
    }

    [Fact]
    public void TicketCreate_FromEmail_ShouldGenerate_TicketCreatedDomainEvent()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        // Act
        var ticket = Ticket.Create(
            companyId: companyId,
            moduleId: moduleId,
            subject: "Email Subject",
            description: "Email Body",
            type: TicketType.Incident,
            source: TicketSource.Email,
            createdByUserId: null,
            priority: TicketPriority.High,
            assignedAgentId: null,
            contactId: contactId
        );
        ticket.SetTicketNumber("TKT-123");

        // Assert
        var domainEvents = ticket.DomainEvents.ToList();
        domainEvents.Should().ContainSingle(e => e is TicketCreatedDomainEvent);

        var createdEvent = (TicketCreatedDomainEvent)domainEvents.First(e => e is TicketCreatedDomainEvent);
        createdEvent.Title.Should().Be("Email Subject");
        createdEvent.Priority.Should().Be(TicketPriority.High);
    }

    [Fact]
    public async Task TicketCreatedDomainEventHandler_ShouldPublish_TicketCreatedIntegrationEvent()
    {
        // Arrange
        var handler = new TicketCreatedDomainEventHandler(_eventBus);
        var ticketId = Guid.NewGuid();
        var domainEvent = new TicketCreatedDomainEvent(
            TicketId: ticketId,
            TicketNumber: "TKT-100",
            Title: "Email Subject",
            Type: TicketType.Incident,
            Priority: TicketPriority.Medium,
            ReporterId: Guid.NewGuid(),
            AssigneeId: null,
            CompanyId: Guid.NewGuid()
        );

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await _eventBus.Received(1).PublishAsync(
            Arg.Is<TicketCreatedIntegrationEvent>(e => 
                e.TicketId == ticketId &&
                e.TicketNumber == "TKT-100" &&
                e.Title == "Email Subject" &&
                e.Category == "Incident" &&
                e.Priority == "Medium"
            ),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public void TicketAddComment_FromEmail_ShouldGenerate_TicketCommentAddedDomainEvent()
    {
        // Arrange
        var ticket = Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), "Subject", "Desc", TicketType.Incident, TicketSource.Email);
        ticket.ClearDomainEvents();

        // Act
        var comment = TicketComment.Create(ticket.Id, null, Guid.NewGuid(), "This is a reply from email", null, false);
        ticket.AddComment(comment, Guid.NewGuid());

        // Assert
        var domainEvents = ticket.DomainEvents.ToList();
        domainEvents.Should().ContainSingle(e => e is TicketCommentAddedDomainEvent);

        var commentEvent = (TicketCommentAddedDomainEvent)domainEvents.First(e => e is TicketCommentAddedDomainEvent);
        commentEvent.Body.Should().Be("This is a reply from email");
    }

    [Fact]
    public async Task TicketCommentAddedDomainEventHandler_ShouldPublish_TicketCommentAddedIntegrationEvent()
    {
        // Arrange
        var handler = new TicketCommentAddedDomainEventHandler(_eventBus);
        var ticketId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var domainEvent = new TicketCommentAddedDomainEvent(
            TicketId: ticketId,
            CommentId: commentId,
            Body: "Reply from Email",
            MentionedUsers: new System.Collections.Generic.List<string>()
        );

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        await _eventBus.Received(1).PublishAsync(
            Arg.Is<TicketCommentAddedIntegrationEvent>(e => 
                e.TicketId == ticketId &&
                e.CommentId == commentId &&
                e.Body == "Reply from Email"
            ),
            Arg.Any<CancellationToken>()
        );
    }
}
