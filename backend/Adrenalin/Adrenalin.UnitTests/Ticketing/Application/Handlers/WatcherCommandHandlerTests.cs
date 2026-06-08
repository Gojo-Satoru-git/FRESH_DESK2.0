using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.UnitTests.Fakes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

public class WatcherCommandHandlerTests
{
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
