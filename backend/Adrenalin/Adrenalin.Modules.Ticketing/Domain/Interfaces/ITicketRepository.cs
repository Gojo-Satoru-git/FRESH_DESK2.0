using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Domain.Enums;

namespace Adrenalin.Modules.Ticketing.Domain.Interfaces;

public interface ITicketRepository
{
    // Existing 
    Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<Ticket?> GetByTicketNumberAsync(
        string ticketNumber,
        CancellationToken ct = default);

    Task<IReadOnlyList<Ticket>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken ct = default);

    Task AddAsync(
        Ticket ticket,
        CancellationToken ct = default);

    Task UpdateAssignmentAsync(
        Guid ticketId,
        Guid? agentId,
        Guid? groupId,
        Guid triggeredBy,
        string? notes = null,
        bool clearAgentIfNull = false,
        CancellationToken ct = default);

    Task AddAssignmentLogAsync(TicketAssignmentLog log, CancellationToken ct = default);

    Task<bool> TryClaimTicketAsync(
        Guid ticketId,
        Guid agentId,
        byte[] rowVersion,
        CancellationToken ct = default);

    Task<Guid?> GetLeastLoadedAgentInGroupAsync(
        Guid groupId,
        CancellationToken ct = default);

    Task<Guid?> GetUserCompanyIdAsync(
        Guid userId,
        CancellationToken ct = default);

    Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default);

    void Update(Ticket ticket);
    Task<IReadOnlyList<Ticket>> GetTicketsAsync(
         string? ticketNumber,
         TicketStatus? status,
         Guid? assignedAgentId,
         Guid? companyId,
         int page,
         int pageSize,
         CancellationToken ct = default);

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
    Task<bool> IsUserInternalAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountTicketsAsync(
        string? ticketNumber,
        TicketStatus? status,
        Guid? assignedAgentId,
        Guid? companyId,
        CancellationToken ct = default);
}