using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, int page, int pageSize, CancellationToken cancellationToken);

    Task<int> CountTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, CancellationToken cancellationToken = default);
    
    Task<Guid?> GetUserCompanyIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);

    Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default);

    void Update(Ticket ticket);

    void Remove(Ticket ticket);

    Task<(Guid ContactId, Guid CompanyId)?> GetContactAndCompanyByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<(Guid ContactId, Guid CompanyId)?> GetContactAndCompanyByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<(Guid ContactId, Guid CompanyId)> AutoCreateContactAndCompanyAsync(string email, string name, CancellationToken cancellationToken = default);

    Task<(Guid ContactId, Guid CompanyId)> AutoCreateContactForUserAsync(Guid userId, string email, string name, CancellationToken cancellationToken = default);

    Task<(Guid ModuleId, string ModuleName, string? Department)> ResolveOrCreateModuleAsync(string categoryName, CancellationToken cancellationToken = default);

    Task<(string Email, string Name)> GetUserEmailAndNameAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, string>> GetUserDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, string>> GetContactDisplayNamesAsync(IEnumerable<Guid> contactIds, CancellationToken cancellationToken = default);

    Task<string> GetCompanyRegionAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<bool> IsUserAdminAsync(Guid userId, CancellationToken cancellationToken = default);
}