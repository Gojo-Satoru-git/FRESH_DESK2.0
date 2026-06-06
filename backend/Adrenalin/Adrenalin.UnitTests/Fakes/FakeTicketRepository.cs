using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;

namespace Adrenalin.UnitTests.Fakes;

// In-memory fake of ITicketRepository shared across all unit tests.
// Supports both list-based (multi-ticket) and single-ticket-return modes.
public class FakeTicketRepository : ITicketRepository
{
    public List<Ticket> Tickets { get; } = new();

    // When set, <see cref="GetByIdAsync"/> returns this ticket instead of
    // searching <see cref="Tickets"/>. Useful for simple single-ticket tests.
    public Ticket? TicketToReturn { get; set; }

    public Guid? DefaultCompanyId { get; set; }

    // Maps UserId → CompanyId for access-control tests.
    public Dictionary<Guid, Guid> UserCompanyMap { get; } = new();

    public Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        // Prefer the pinned ticket when set, otherwise search the list
        if (TicketToReturn is not null && TicketToReturn.Id == ticketId)
            return Task.FromResult<Ticket?>(TicketToReturn);

        return Task.FromResult(Tickets.FirstOrDefault(x => x.Id == ticketId));
    }

    public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        Tickets.Add(ticket);
        // Keep TicketToReturn in sync when only one ticket is used
        TicketToReturn ??= ticket;
        return Task.CompletedTask;
    }

    public void Update(Ticket ticket) {}

    public void Remove(Ticket ticket) => Tickets.Remove(ticket);

    public Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var found = TicketToReturn?.Id == ticketId
            || Tickets.Any(x => x.Id == ticketId);
        return Task.FromResult(found);
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
            query = query.Where(x => x.TicketNumber == ticketNumber);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (assignedAgentId.HasValue)
            query = query.Where(x => x.AssignedAgentId == assignedAgentId);

        if (companyId.HasValue)
            query = query.Where(x => x.CompanyId == companyId);

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
            query = query.Where(x => x.TicketNumber == ticketNumber);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (assignedAgentId.HasValue)
            query = query.Where(x => x.AssignedAgentId == assignedAgentId.Value);

        if (companyId.HasValue)
            query = query.Where(x => x.CompanyId == companyId);

        return Task.FromResult(query.Count());
    }

    public Task<Guid?> GetUserCompanyIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (UserCompanyMap.TryGetValue(userId, out var cid))
            return Task.FromResult<Guid?>(cid);

        if (DefaultCompanyId.HasValue)
            return Task.FromResult<Guid?>(DefaultCompanyId.Value);

        // Fallback: infer from any stored ticket
        var any = TicketToReturn ?? Tickets.FirstOrDefault();
        return Task.FromResult<Guid?>(any?.CompanyId);
    }
}
