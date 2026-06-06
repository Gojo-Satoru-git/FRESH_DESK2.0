using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

public class WatcherCommandHandlerTests
{
    private class FakeTicketRepository : ITicketRepository
    {
        public Ticket? TicketToReturn { get; set; }

        public Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TicketToReturn?.Id == ticketId ? TicketToReturn : null);
        }

        public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Ticket ticket) { }
        public Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default) => Task.FromResult(TicketToReturn?.Id == ticketId);
        public void Remove(Ticket ticket) { }

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
            if (TicketToReturn is not null)
            {
                return Task.FromResult<Guid?>(TicketToReturn.CompanyId);
            }
            return Task.FromResult<Guid?>(null);
        }
    }

    private readonly FakeTicketRepository _ticketRepository;
    private readonly AddWatcherCommandHandler _addHandler;
    private readonly RemoveWatcherCommandHandler _removeHandler;

    public WatcherCommandHandlerTests()
    {
        _ticketRepository = new FakeTicketRepository();
        _addHandler = new AddWatcherCommandHandler(_ticketRepository);
        _removeHandler = new RemoveWatcherCommandHandler(_ticketRepository);
    }

    [Fact]
    public async Task Handle_AddWatcher_ShouldAddWatcher_WhenTicketExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Test Subject", "Test Description");
        _ticketRepository.TicketToReturn = ticket;

        var userId = Guid.NewGuid();
        var addedBy = Guid.NewGuid();
        var command = new AddWatcherCommand(ticket.Id, userId, addedBy);

        // Act
        var result = await _addHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Single(ticket.TicketWatchers);
        var watcher = ticket.TicketWatchers.GetEnumerator();
        watcher.MoveNext();
        Assert.Equal(userId, watcher.Current.UserId);
        Assert.Equal(addedBy, watcher.Current.AddedBy);
    }

    [Fact]
    public async Task Handle_RemoveWatcher_ShouldRemoveWatcher_WhenTicketExists()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Test Subject", "Test Description");
        
        var userId = Guid.NewGuid();
        var addedBy = Guid.NewGuid();
        ticket.AddWatcher(userId, addedBy);
        
        _ticketRepository.TicketToReturn = ticket;

        var command = new RemoveWatcherCommand(ticket.Id, userId);

        // Act
        var result = await _removeHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(ticket.Id, result);
        Assert.Empty(ticket.TicketWatchers);
    }

    [Fact]
    public async Task Handle_RemoveWatcher_ShouldThrowException_WhenWatcherDoesNotExist()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var ticket = Ticket.Create(companyId, moduleId, "Test Subject", "Test Description");
        
        _ticketRepository.TicketToReturn = ticket;

        var command = new RemoveWatcherCommand(ticket.Id, Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() => 
            _removeHandler.Handle(command, CancellationToken.None));
    }
}
