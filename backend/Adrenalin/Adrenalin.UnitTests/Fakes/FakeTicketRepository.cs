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

    public HashSet<Guid> AdminUserIds { get; } = new();

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

    public Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"TKT-{DateTime.UtcNow.Year}-{Tickets.Count + 1:D6}");
    }

    public Task<(Guid ContactId, Guid CompanyId)?> GetContactAndCompanyByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var companyId = DefaultCompanyId ?? Guid.NewGuid();
        return Task.FromResult<(Guid ContactId, Guid CompanyId)?>((userId, companyId));
    }

    public Task<(Guid ContactId, Guid CompanyId)?> GetContactAndCompanyByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var contactId = Guid.NewGuid();
        var companyId = DefaultCompanyId ?? Guid.NewGuid();
        return Task.FromResult<(Guid ContactId, Guid CompanyId)?>((contactId, companyId));
    }

    public Task<(Guid ContactId, Guid CompanyId)> AutoCreateContactAndCompanyAsync(string email, string name, CancellationToken cancellationToken = default)
    {
        var contactId = Guid.NewGuid();
        var companyId = DefaultCompanyId ?? Guid.NewGuid();
        return Task.FromResult((contactId, companyId));
    }

    public Task<(Guid ContactId, Guid CompanyId)> AutoCreateContactForUserAsync(Guid userId, string email, string name, CancellationToken cancellationToken = default)
    {
        var companyId = DefaultCompanyId ?? Guid.NewGuid();
        return Task.FromResult((userId, companyId));
    }

    public Task<(Guid ModuleId, string ModuleName, string? Department)> ResolveOrCreateModuleAsync(string categoryName, CancellationToken cancellationToken = default)
    {
        var moduleId = Guid.NewGuid();
        return Task.FromResult((moduleId, categoryName, (string?)"Support"));
    }

    public Task<(string Email, string Name)> GetUserEmailAndNameAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(( "user@example.com", "Test User" ));
    }

    public Task<Dictionary<Guid, string>> GetUserDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var dict = userIds.Distinct().ToDictionary(id => id, id => $"Test User ({id.ToString().Substring(0, 8)})");
        return Task.FromResult(dict);
    }

    public Task<Dictionary<Guid, string>> GetContactDisplayNamesAsync(IEnumerable<Guid> contactIds, CancellationToken cancellationToken = default)
    {
        var dict = contactIds.Distinct().ToDictionary(id => id, id => $"Test Contact ({id.ToString().Substring(0, 8)})");
        return Task.FromResult(dict);
    }

    public Task<string> GetCompanyRegionAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult("US");
    }

    public Task<bool> IsUserAdminAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(AdminUserIds.Contains(userId));
    }
}
