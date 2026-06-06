using Adrenalin.Modules.Ticketing.Application.Commands;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.UnitTests.Fakes;

namespace Adrenalin.UnitTests.Ticketing;

// Tests for ticket relation commands:
// AddRelation, LinkTickets, and MergeTicket.
public class TicketRelationHandlerTests
{
    private readonly FakeTicketRepository _repo;

    public TicketRelationHandlerTests()
    {
        _repo = new FakeTicketRepository();
    }

    private async Task<Ticket> Add(Guid companyId, Guid moduleId, string subject = "S", string desc = "D")
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
        var t1 = await Add(companyId, moduleId, "Parent");
        var t2 = await Add(companyId, moduleId, "Child");

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
        var t1 = await Add(Guid.NewGuid(), Guid.NewGuid(), "T1");
        var t2 = await Add(Guid.NewGuid(), Guid.NewGuid(), "T2");

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
        var tA = await Add(companyId, moduleId, "A");
        var tB = await Add(companyId, moduleId, "B");
        var tC = await Add(companyId, moduleId, "C");

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
        var t1 = await Add(Guid.NewGuid(), Guid.NewGuid(), "T1");
        var t2 = await Add(Guid.NewGuid(), Guid.NewGuid(), "T2");

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
        var master    = await Add(Guid.NewGuid(), Guid.NewGuid(), "Master");
        var duplicate = await Add(Guid.NewGuid(), Guid.NewGuid(), "Duplicate");

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
        var master    = await Add(companyId, moduleId, "Master");
        var duplicate = await Add(companyId, moduleId, "Duplicate");

        var mergerId = Guid.NewGuid();
        _repo.UserCompanyMap[mergerId] = Guid.NewGuid(); // wrong company

        var command = new MergeTicketCommand(master.Id, duplicate.Id, mergerId);
        var handler = new MergeTicketCommandHandler(_repo, new FakeUnitOfWork());

        // Act & Assert
        await Assert.ThrowsAsync<TicketDomainException>(() =>
            handler.Handle(command, CancellationToken.None));
    }
}
