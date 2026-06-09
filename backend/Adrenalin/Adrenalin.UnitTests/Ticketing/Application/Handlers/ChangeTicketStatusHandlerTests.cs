using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.UnitTests.Fakes;

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

// Tests for <see cref="ChangeTicketStatusCommandHandler"/>.
// Covers: valid transitions, invalid transitions, resolve guards.
public class ChangeTicketStatusHandlerTests
{
    private readonly FakeTicketRepository _repo;
    private readonly ChangeTicketStatusCommandHandler _handler;

    public ChangeTicketStatusHandlerTests()
    {
        _repo    = new FakeTicketRepository();
        _handler = new ChangeTicketStatusCommandHandler(_repo);
    }

    private async Task<Ticket> CreateTicket(string subject = "Subject", string desc = "Desc")
    {
        var ticket = Ticket.Create(Guid.NewGuid(), Guid.NewGuid(), subject, desc);
        await _repo.AddAsync(ticket);
        return ticket;
    }

    // Happy path
    [Fact]
    public async Task Handle_ShouldTransitionToOpen_WhenTicketIsNew()
    {
        // Arrange
        var ticket  = await CreateTicket();
        var command = new ChangeTicketStatusCommand(ticket.Id, TicketStatus.Open, Guid.NewGuid(), "Opening");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public async Task Handle_ShouldResolve_WhenCustomerCallTakenAndRcaProvided()
    {
        // Arrange
        var ticket = await CreateTicket();
        var userId = Guid.NewGuid();

        ticket.ChangeStatus(TicketStatus.Open, userId);
        ticket.ChangeStatus(TicketStatus.Assigned, userId);
        ticket.ChangeStatus(TicketStatus.InProgress, userId);
        ticket.MarkCustomerCallTaken(userId);
        ticket.ProvideRootCauseAnalysis("Network packet drop", userId);

        var command = new ChangeTicketStatusCommand(ticket.Id, TicketStatus.Resolved, userId, "Issue fixed");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
    }

    // Guard / error cases

    [Fact]
    public async Task Handle_ShouldThrow_WhenTransitionSkipsSteps()
    {
        // Arrange — New → InProgress is an invalid jump
        var ticket  = await CreateTicket();
        var command = new ChangeTicketStatusCommand(ticket.Id, TicketStatus.InProgress, Guid.NewGuid(), "Skip");

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenResolvingWithoutCallAndRca()
    {
        // Arrange
        var ticket = await CreateTicket();
        ticket.ChangeStatus(TicketStatus.Open, Guid.NewGuid());

        var command = new ChangeTicketStatusCommand(ticket.Id, TicketStatus.Resolved, Guid.NewGuid(), "Resolve");

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }
}
