using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adrenalin.UnitTests;

public class ChangeTicketStatusCommandHandlerTests
{
    private class FakeTicketRepository : ITicketRepository
    {
        public List<Ticket> Tickets { get; } = new();

        public Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tickets.FirstOrDefault(x => x.Id == ticketId));
        }

        public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            Tickets.Add(ticket);
            return Task.CompletedTask;
        }

        public void Update(Ticket ticket) { }

        public Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tickets.Any(x => x.Id == ticketId));
        }

        public void Remove(Ticket ticket)
        {
            Tickets.Remove(ticket);
        }

        public Task<IReadOnlyList<Ticket>> GetTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, int page, int pageSize, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Ticket>>(Array.Empty<Ticket>());
        }

        public Task<int> CountTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Guid? DefaultCompanyId { get; set; }
        public Dictionary<Guid, Guid> UserCompanyMap { get; } = new();

        public Task<Guid?> GetUserCompanyIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (UserCompanyMap.TryGetValue(userId, out var cid))
            {
                return Task.FromResult<Guid?>(cid);
            }
            if (DefaultCompanyId.HasValue)
            {
                return Task.FromResult<Guid?>(DefaultCompanyId.Value);
            }
            if (Tickets.Any())
            {
                return Task.FromResult<Guid?>(Tickets.First().CompanyId);
            }
            return Task.FromResult<Guid?>(null);
        }
    }

    private readonly FakeTicketRepository _ticketRepository;
    private readonly ChangeTicketStatusCommandHandler _handler;

    public ChangeTicketStatusCommandHandlerTests()
    {
        _ticketRepository = new FakeTicketRepository();
        _handler = new ChangeTicketStatusCommandHandler(_ticketRepository);
    }

    [Fact]
    public async Task Handle_ShouldTransitionStatus_WhenTransitionIsValid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subject", "Description");
        await _ticketRepository.AddAsync(ticket);

        var command = new ChangeTicketStatusCommand(ticket.Id, TicketStatus.Open, Guid.NewGuid(), "Transition to Open");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Equal(TicketStatus.Open, ticket.Status);
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenTransitionIsInvalid()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subject", "Description");
        await _ticketRepository.AddAsync(ticket);

        // Transitioning directly from New to InProgress is invalid
        var command = new ChangeTicketStatusCommand(ticket.Id, TicketStatus.InProgress, Guid.NewGuid(), "Direct transition");

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowException_WhenResolvingWithoutCustomerCallAndRca()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subject", "Description");
        ticket.ChangeStatus(TicketStatus.Open, Guid.NewGuid());
        await _ticketRepository.AddAsync(ticket);

        var command = new ChangeTicketStatusCommand(ticket.Id, TicketStatus.Resolved, Guid.NewGuid(), "Resolving ticket");

        // Act & Assert (Customer call and RCA are missing, so it must throw)
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldResolve_WhenCustomerCallTakenAndRcaProvided()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Subject", "Description");
        var userId = Guid.NewGuid();

        ticket.ChangeStatus(TicketStatus.Open, userId);
        ticket.MarkCustomerCallTaken(userId);
        ticket.ProvideRootCauseAnalysis("Identified network packet drop issue.", userId);
        await _ticketRepository.AddAsync(ticket);

        var command = new ChangeTicketStatusCommand(ticket.Id, TicketStatus.Resolved, userId, "Issue fixed");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
    }
}
