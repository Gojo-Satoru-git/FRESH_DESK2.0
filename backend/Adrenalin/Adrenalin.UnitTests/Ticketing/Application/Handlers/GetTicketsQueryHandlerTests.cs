using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Handlers;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.SharedKernel.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Adrenalin.UnitTests.Fakes;
namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

public class GetTicketsQueryHandlerTests
{

    private readonly FakeTicketRepository _ticketRepository;
    private readonly GetTicketsQueryHandler _handler;

    public GetTicketsQueryHandlerTests()
    {
        _ticketRepository = new FakeTicketRepository();
        _handler = new GetTicketsQueryHandler(_ticketRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedTickets_WhenValidQuery()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        
        var ticket1 = Ticket.Create(companyId, moduleId, "Ticket 1", "Description 1");
        var ticket2 = Ticket.Create(companyId, moduleId, "Ticket 2", "Description 2");
        var ticket3 = Ticket.Create(companyId, moduleId, "Ticket 3", "Description 3");

        // Force one to be InProgress
        ticket2.ChangeStatus(TicketStatus.Open, Guid.NewGuid(), "Transition to Open");
        ticket2.ChangeStatus(TicketStatus.Assigned, Guid.NewGuid(), "Transition to Assigned");
        ticket2.ChangeStatus(TicketStatus.InProgress, Guid.NewGuid(), "Transition to InProgress");

        await _ticketRepository.AddAsync(ticket1);
        await _ticketRepository.AddAsync(ticket2);
        await _ticketRepository.AddAsync(ticket3);

        var query = new GetTicketsQuery(
            TicketNumber: null,
            Status: "InProgress",
            AssignedAgentId: null,
            CompanyId: companyId,
            Page: 1,
            PageSize: 10
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(ticket2.Id, result.Items.First().Id);
        Assert.Equal("InProgress", result.Items.First().Status);
    }
}
