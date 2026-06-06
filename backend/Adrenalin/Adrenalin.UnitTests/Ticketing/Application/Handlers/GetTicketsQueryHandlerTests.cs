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

namespace Adrenalin.UnitTests.Ticketing.Application.Handlers;

public class GetTicketsQueryHandlerTests
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

        public Task<IReadOnlyList<Ticket>> GetTicketsAsync(
            string? ticketNumber,
            TicketStatus? status,
            Guid? assignedAgentId,
            Guid? companyId,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ticketNumber))
            {
                query = query.Where(x => x.TicketNumber == ticketNumber);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (assignedAgentId.HasValue)
            {
                query = query.Where(x => x.AssignedAgentId == assignedAgentId);
            }

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            var result = query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<IReadOnlyList<Ticket>>(result);
        }

        public Task<int> CountTicketsAsync(
            string? ticketNumber,
            TicketStatus? status,
            Guid? assignedAgentId,
            Guid? companyId,
            CancellationToken cancellationToken = default)
        {
            var query = Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(ticketNumber))
            {
                query = query.Where(x => x.TicketNumber == ticketNumber);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            if (assignedAgentId.HasValue)
            {
                query = query.Where(x => x.AssignedAgentId == assignedAgentId.Value);
            }

            if (companyId.HasValue)
            {
                query = query.Where(x => x.CompanyId == companyId);
            }

            return Task.FromResult(query.Count());
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
