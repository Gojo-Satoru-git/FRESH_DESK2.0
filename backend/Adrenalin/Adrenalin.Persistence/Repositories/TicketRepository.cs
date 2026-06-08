using Adrenalin.Modules.Ticketing.Domain.Entities;
using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class TicketRepository : ITicketRepository
{
	private readonly AdrenalinDbContext _context;

	public TicketRepository(AdrenalinDbContext context)
	{
		_context = context;
	}

	public async Task<Ticket?> GetByIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
	{
		return await _context.Tickets
			.Include(x => x.TicketComments)
			.ThenInclude(c => c.Attachments)
			.Include(x => x.TicketAttachments)
			.Include(x => x.TicketWatchers)
			.Include(x => x.TicketRelations)
			.Include(x => x.TicketStatusHistories)
			.Include(x => x.TicketAssignmentLogs)
			.FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);
	}

    public async Task AddAssignmentLogAsync(
       TicketAssignmentLog log,
       CancellationToken ct = default)
    {
        // INSERT into ticket.ticket_assignment_log
        await _context.TicketAssignmentLogs.AddAsync(log, ct);
    }

    public async Task<Guid?> GetLeastLoadedAgentInGroupAsync(
        Guid groupId, CancellationToken ct = default)
    {
        // Get all agent IDs in this group
        // via auth.user_groups junction table
        var agentIds = await _context.UserGroups
            .Where(ug => ug.GroupId == groupId)
            .Select(ug => ug.UserId)
            .ToListAsync(ct);

        if (!agentIds.Any()) return null;

        // ticket.tickets exact columns used:
        // assigned_agent_id, status (ENUM), is_deleted
        // Count open tickets per agent — excluding resolved/closed
        var openStatuses = new[] { "resolved", "closed" };

        return await _context.Tickets
            .Where(t =>
                t.AssignedAgentId != null &&
                agentIds.Contains(t.AssignedAgentId!.Value) &&
                !openStatuses.Contains(
                    t.Status.ToString().ToLower()) &&
                !t.IsDeleted)
            .GroupBy(t => t.AssignedAgentId)
            .OrderBy(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> CountTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, CancellationToken cancellationToken = default)
	{
		var query = _context.Tickets
			.AsNoTracking()
			.AsQueryable();

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

		return await query.CountAsync(cancellationToken);
	}

	public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
	{
		await _context.Tickets.AddAsync(
			ticket,
			cancellationToken);
	}

	public async Task<IReadOnlyList<Ticket>> GetTicketsAsync(string? ticketNumber, TicketStatus? status, Guid? assignedAgentId, Guid? companyId, int page, int pageSize, CancellationToken cancellationToken)
	{
		var query = _context.Tickets
			.AsNoTracking()
			.AsQueryable();

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

		return await query
			.OrderByDescending(x => x.CreatedAt)
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToListAsync(cancellationToken);
	}

	public void Update(Ticket ticket)
	{
		_context.Tickets.Update(ticket);
	}

	public async Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default)
	{
		return await _context.Tickets.AnyAsync(x => x.Id == ticketId, cancellationToken);
	}

	public void Remove(Ticket ticket)
	{
		_context.Tickets.Remove(ticket);
	}

	public async Task<Guid?> GetUserCompanyIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		// 1. Check if the user is a Contact
		var contactCompanyId = await _context.Contacts
			.Where(c => c.UserId == userId && !c.IsDeleted)
			.Select(c => (Guid?)c.CompanyId)
			.FirstOrDefaultAsync(cancellationToken);

		if (contactCompanyId.HasValue)
		{
			return contactCompanyId.Value;
		}

		// 2. Check if the user is a CAM or Delivery Manager for any Company
		var companyId = await _context.Companies
			.Where(c => (c.CamUserId == userId || c.DeliveryManagerId == userId) && !c.IsDeleted)
			.Select(c => (Guid?)c.Id)
			.FirstOrDefaultAsync(cancellationToken);

		return companyId;
	}
    public async Task UpdateAssignmentAsync(
    Guid ticketId,
    Guid? agentId,
    Guid? groupId,
    Guid triggeredBy,
    CancellationToken ct = default)
    {
        // Must load with tracking so EF Core sees the changes
        // Include assignment logs so the collection is loaded
        var ticket = await _context.Tickets
            .Include(t => t.TicketAssignmentLogs)
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct);

        if (ticket is null) return;

        // AssignAgent calls TicketAssignmentLog.Create() internally
        // and adds it to _ticketAssignmentLogs collection
        // Only assign if we have an agent
        if (agentId.HasValue)
        {
            ticket.AssignAgent(
                agentId: agentId.Value,
                assignedBy: triggeredBy,
                notes: "Auto-assigned via automation rules");
        }

        // AssignGroup sets GroupId on the ticket
        if (groupId.HasValue)
        {
            ticket.AssignGroup(
                groupId: groupId.Value,
                modifiedBy: triggeredBy);
        }

        // No manual property setting needed
        // AssignAgent and AssignGroup call Touch() internally
        // which sets UpdatedAt and UpdatedBy
    }
}