using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.UnitTests.Fakes;

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

// Tests for ticket lifecycle commands:
// Close, Reopen, Resolve.
public class TicketLifecycleHandlerTests
{
    private readonly FakeTicketRepository _repo;

    public TicketLifecycleHandlerTests()
    {
        _repo = new FakeTicketRepository();
    }

    // Builds a ticket that has been fully resolved.
    private async Task<(Ticket ticket, Guid userId)> BuildResolvedTicket()
    {
        var userId = Guid.NewGuid();
        var ticket = Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), "Subj", "Desc");
        ticket.ChangeStatus(TicketStatus.Open, userId);
        ticket.MarkCustomerCallTaken(userId);
        ticket.ProvideRootCauseAnalysis("Root cause", userId);
        ticket.Resolve(userId, "Resolved");
        await _repo.AddAsync(ticket);
        return (ticket, userId);
    }

    // Resolve

    [Fact]
    public async Task ResolveTicket_ShouldSucceed_WhenCallAndRcaProvided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var ticket = Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), "Subj", "Desc");
        ticket.ChangeStatus(TicketStatus.Open, userId);
        ticket.MarkCustomerCallTaken(userId);
        ticket.ProvideRootCauseAnalysis("Fix details", userId);
        await _repo.AddAsync(ticket);

        var command = new ResolveTicketCommand(ticket.Id, userId, "Resolution notes");
        var handler = new ResolveTicketCommandHandler(_repo);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
    }

    // Close

    [Fact]
    public async Task CloseTicket_ShouldSucceed_WhenTicketIsResolved()
    {
        // Arrange
        var (ticket, userId) = await BuildResolvedTicket();
        var command = new CloseTicketCommand(ticket.Id, userId, "Done");
        var handler = new CloseTicketCommandHandler(_repo);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    [Fact]
    public async Task CloseTicket_ShouldThrow_WhenTicketIsNotResolved()
    {
        // Arrange — ticket is still in New state
        var ticket = Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), "Subj", "Desc");
        await _repo.AddAsync(ticket);

        var command = new CloseTicketCommand(ticket.Id, Guid.NewGuid(), "Done");
        var handler = new CloseTicketCommandHandler(_repo);

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    // Reopen

    [Fact]
    public async Task ReopenTicket_ShouldSucceed_WhenTicketIsResolved()
    {
        // Arrange
        var (ticket, userId) = await BuildResolvedTicket();
        var command = new ReopenTicketCommand(ticket.Id, userId, "Need more help");
        var handler = new ReopenTicketCommandHandler(_repo);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Equal(TicketStatus.Reopened, ticket.Status);
    }
}
