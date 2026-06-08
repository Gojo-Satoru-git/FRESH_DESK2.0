using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Exceptions;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.UnitTests.Fakes;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

// Tests for ticket relation commands:
// AddRelation, LinkTickets, and MergeTicket.
public class TicketRelationHandlerTests
{
    private readonly FakeTicketRepository _repo;

    public TicketRelationHandlerTests()
    {
        _repo = new FakeTicketRepository();
    }

    private async Task<Ticket> Add(Guid companyId, Guid moduleId, string subject = "Subject", string desc = "Description")
    {
        var ticket = Ticket.Create(companyId, moduleId, subject, desc);
        await _repo.AddAsync(ticket);
        return ticket;
    }

    // AddRelation

    [Fact]
    public async Task AddRelation_ShouldSucceed_WhenTicketsBelongToSameCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId  = Guid.NewGuid();
        var t1 = await Add(companyId, moduleId, "ParentTicket");
        var t2 = await Add(companyId, moduleId, "ChildTicket");

        var command = new AddRelationCommand(t1.Id, t2.Id, TicketRelationType.DependsOn);
        var handler = new AddRelationCommandHandler(_repo);

        // Act
        var relationId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, relationId);
        Assert.Contains(t1.TicketRelations,
            r => r.ChildTicketId == t2.Id && r.RelationType == TicketRelationType.DependsOn);
    }

    [Fact]
    public async Task AddRelation_ShouldThrow_WhenTicketsBelongToDifferentCompanies()
    {
        // Arrange
        var t1 = await Add(Guid.NewGuid(), Guid.NewGuid(), "Ticket1");
        var t2 = await Add(Guid.NewGuid(), Guid.NewGuid(), "Ticket2");

        var command = new AddRelationCommand(t1.Id, t2.Id, TicketRelationType.DependsOn);
        var handler = new AddRelationCommandHandler(_repo);

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task AddRelation_ShouldThrow_WhenCircularRelationDetected()
    {
        // Arrange — A→B→C, then C→A is circular
        var companyId = Guid.NewGuid();
        var moduleId  = Guid.NewGuid();
        var tA = await Add(companyId, moduleId, "TicketA");
        var tB = await Add(companyId, moduleId, "TicketB");
        var tC = await Add(companyId, moduleId, "TicketC");

        var handler = new AddRelationCommandHandler(_repo);
        await handler.Handle(new AddRelationCommand(tA.Id, tB.Id, TicketRelationType.ParentChild), CancellationToken.None);
        await handler.Handle(new AddRelationCommand(tB.Id, tC.Id, TicketRelationType.ParentChild), CancellationToken.None);

        // Act & Assert — C→A creates a cycle
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(new AddRelationCommand(tC.Id, tA.Id, TicketRelationType.ParentChild), CancellationToken.None));
    }

    // LinkTickets

    [Fact]
    public async Task LinkTickets_ShouldThrow_WhenTicketsBelongToDifferentCompanies()
    {
        // Arrange
        var t1 = await Add(Guid.NewGuid(), Guid.NewGuid(), "Ticket1");
        var t2 = await Add(Guid.NewGuid(), Guid.NewGuid(), "Ticket2");

        var command = new LinkTicketsCommand(t1.Id, t2.Id, TicketRelationType.DependsOn);
        var handler = new LinkTicketsCommandHandler(_repo);

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    // MergeTicket

    [Fact]
    public async Task MergeTicket_ShouldThrow_WhenTicketsBelongToDifferentCompanies()
    {
        // Arrange
        var master    = await Add(Guid.NewGuid(), Guid.NewGuid(), "MasterTicket");
        var duplicate = await Add(Guid.NewGuid(), Guid.NewGuid(), "DuplicateTicket");

        var command = new MergeTicketCommand(master.Id, duplicate.Id, Guid.NewGuid());
        var handler = new MergeTicketCommandHandler(_repo, new FakeUnitOfWork());

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task MergeTicket_ShouldThrow_WhenMergerBelongsToDifferentCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId  = Guid.NewGuid();
        var master    = await Add(companyId, moduleId, "MasterTicket");
        var duplicate = await Add(companyId, moduleId, "DuplicateTicket");

        var mergerId = Guid.NewGuid();
        _repo.UserCompanyMap[mergerId] = Guid.NewGuid(); // wrong company

        var command = new MergeTicketCommand(master.Id, duplicate.Id, mergerId);
        var handler = new MergeTicketCommandHandler(_repo, new FakeUnitOfWork());

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
