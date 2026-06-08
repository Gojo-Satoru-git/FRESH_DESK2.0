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
		var entry = _context.Entry(ticket);
		if (entry.State == EntityState.Detached)
		{
			_context.Tickets.Attach(ticket);
			entry.State = EntityState.Modified;
		}
	}

	public async Task<bool> ExistsAsync(Guid ticketId, CancellationToken cancellationToken = default)
	{
		return await _context.Tickets.AnyAsync(x => x.Id == ticketId, cancellationToken);
	}

	public void Remove(Ticket ticket)
	{
		_context.Tickets.Remove(ticket);
	}

	public async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken = default)
	{
		var year = DateTime.UtcNow.Year;
		var prefix = $"TKT-{year}-";
		
		var maxTicketNumber = await _context.Tickets
			.Where(t => t.TicketNumber != null && t.TicketNumber.StartsWith(prefix))
			.OrderByDescending(t => t.TicketNumber)
			.Select(t => t.TicketNumber)
			.FirstOrDefaultAsync(cancellationToken);

		int nextSequence = 1;
		if (maxTicketNumber != null && maxTicketNumber.Length > prefix.Length)
		{
			var suffix = maxTicketNumber.Substring(prefix.Length);
			if (int.TryParse(suffix, out var parsedSequence))
			{
				nextSequence = parsedSequence + 1;
			}
		}

		return $"{prefix}{nextSequence:D6}";
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

	public async Task<(Guid ContactId, Guid CompanyId)?> GetContactAndCompanyByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var contact = await _context.Contacts
			.Where(c => c.UserId == userId && !c.IsDeleted)
			.Select(c => new { c.Id, c.CompanyId })
			.FirstOrDefaultAsync(cancellationToken);

		if (contact != null)
		{
			return (contact.Id, contact.CompanyId);
		}

		return null;
	}

	public async Task<(Guid ContactId, Guid CompanyId)?> GetContactAndCompanyByEmailAsync(string email, CancellationToken cancellationToken = default)
	{
		var contact = await _context.Contacts
			.Where(c => c.Email == email && !c.IsDeleted)
			.Select(c => new { c.Id, c.CompanyId })
			.FirstOrDefaultAsync(cancellationToken);

		if (contact != null)
		{
			return (contact.Id, contact.CompanyId);
		}

		return null;
	}

	public async Task<(Guid ContactId, Guid CompanyId)> AutoCreateContactAndCompanyAsync(string email, string name, CancellationToken cancellationToken = default)
	{
		await EnsureGeoRegionAndCustomerTierExistAsync(cancellationToken);

		var emailDomain = email.Split('@').Last().ToLowerInvariant();

		var companyDomain = await _context.CompanyDomains
			.Where(d => d.Domain == emailDomain && !d.IsDeleted)
			.FirstOrDefaultAsync(cancellationToken);

		Guid companyId;
		if (companyDomain != null)
		{
			companyId = companyDomain.CompanyId;
		}
		else
		{
			var company = Adrenalin.Modules.Company.Domain.Entities.Company.Create(
				name: emailDomain,
				geoRegion: "US",
				supportTier: "Standard"
			);
			await _context.Companies.AddAsync(company, cancellationToken);

			var domain = Adrenalin.Modules.Company.Domain.Entities.CompanyDomain.Create(
				companyId: company.Id,
				domain: emailDomain,
				isPrimary: true
			);
			await _context.CompanyDomains.AddAsync(domain, cancellationToken);

			companyId = company.Id;
		}

		var contact = Adrenalin.Modules.Company.Domain.Entities.Contact.Create(
			companyId: companyId,
			email: email,
			name: name,
			autoCreated: true,
			isAuthorized: true
		);
		await _context.Contacts.AddAsync(contact, cancellationToken);

		return (contact.Id, companyId);
	}

	public async Task<(Guid ContactId, Guid CompanyId)> AutoCreateContactForUserAsync(Guid userId, string email, string name, CancellationToken cancellationToken = default)
	{
		var contact = await _context.Contacts
			.FirstOrDefaultAsync(c => c.Email == email && !c.IsDeleted, cancellationToken);

		if (contact != null)
		{
			_context.Entry(contact).Property(c => c.UserId).CurrentValue = userId;
			return (contact.Id, contact.CompanyId);
		}

		await EnsureGeoRegionAndCustomerTierExistAsync(cancellationToken);

		var emailDomain = email.Split('@').Last().ToLowerInvariant();
		var companyDomain = await _context.CompanyDomains
			.Where(d => d.Domain == emailDomain && !d.IsDeleted)
			.FirstOrDefaultAsync(cancellationToken);

		Guid companyId;
		if (companyDomain != null)
		{
			companyId = companyDomain.CompanyId;
		}
		else
		{
			var company = Adrenalin.Modules.Company.Domain.Entities.Company.Create(
				name: emailDomain,
				geoRegion: "US",
				supportTier: "Standard"
			);
			await _context.Companies.AddAsync(company, cancellationToken);

			var domain = Adrenalin.Modules.Company.Domain.Entities.CompanyDomain.Create(
				companyId: company.Id,
				domain: emailDomain,
				isPrimary: true
			);
			await _context.CompanyDomains.AddAsync(domain, cancellationToken);

			companyId = company.Id;
		}

		contact = Adrenalin.Modules.Company.Domain.Entities.Contact.Create(
			companyId: companyId,
			email: email,
			name: name,
			autoCreated: false,
			isAuthorized: true,
			userId: userId
		);
		await _context.Contacts.AddAsync(contact, cancellationToken);

		return (contact.Id, companyId);
	}

	public async Task<(Guid ModuleId, string ModuleName, string? Department)> ResolveOrCreateModuleAsync(string categoryName, CancellationToken cancellationToken = default)
	{
		var module = await _context.Modules
			.FirstOrDefaultAsync(m => m.Label == categoryName || m.Code == categoryName, cancellationToken);

		if (module != null)
		{
			return (module.Id, module.Label, module.Department);
		}

		var code = categoryName.ToUpperInvariant().Replace(" ", "_");
		module = Adrenalin.Modules.Lookup.Domain.Entities.Module.Create(
			code: code,
			label: categoryName,
			department: "Support"
		);
		await _context.Modules.AddAsync(module, cancellationToken);

		return (module.Id, module.Label, module.Department);
	}

	public async Task<(string Email, string Name)> GetUserEmailAndNameAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var user = await _context.Users
			.Where(u => u.Id == userId && !u.IsDeleted)
			.Select(u => new { u.Email, u.FirstName, u.LastName })
			.FirstOrDefaultAsync(cancellationToken);

		if (user != null)
		{
			var fullName = $"{user.FirstName} {user.LastName}".Trim();
			if (string.IsNullOrWhiteSpace(fullName))
			{
				fullName = user.Email.Split('@').First();
			}
			return (user.Email, fullName);
		}

		throw new KeyNotFoundException($"User with ID {userId} not found.");
	}

	public async Task<Dictionary<Guid, string>> GetUserDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
	{
		var uniqueIds = userIds.Where(id => id != Guid.Empty).Distinct().ToList();
		if (!uniqueIds.Any()) return new Dictionary<Guid, string>();

		var users = await _context.Users
			.Where(u => uniqueIds.Contains(u.Id) && !u.IsDeleted)
			.Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
			.ToListAsync(cancellationToken);

		return users.ToDictionary(
			u => u.Id,
			u => {
				var fullName = $"{u.FirstName} {u.LastName}".Trim();
				return !string.IsNullOrWhiteSpace(fullName) ? fullName : u.Email.Split('@').First();
			}
		);
	}

	public async Task<Dictionary<Guid, string>> GetContactDisplayNamesAsync(IEnumerable<Guid> contactIds, CancellationToken cancellationToken = default)
	{
		var uniqueIds = contactIds.Where(id => id != Guid.Empty).Distinct().ToList();
		if (!uniqueIds.Any()) return new Dictionary<Guid, string>();

		var contacts = await _context.Contacts
			.Where(c => uniqueIds.Contains(c.Id) && !c.IsDeleted)
			.Select(c => new { c.Id, c.Name, c.Email })
			.ToListAsync(cancellationToken);

		return contacts.ToDictionary(
			c => c.Id,
			c => !string.IsNullOrWhiteSpace(c.Name) ? c.Name : c.Email.Split('@').First()
		);
	}


	public async Task<string> GetCompanyRegionAsync(Guid companyId, CancellationToken cancellationToken = default)
	{
		var company = await _context.Companies
			.Where(c => c.Id == companyId && !c.IsDeleted)
			.Select(c => c.GeoRegion)
			.FirstOrDefaultAsync(cancellationToken);

		return company ?? "US";
	}

	public async Task<bool> IsUserAdminAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		return await (from ur in _context.UserRoles
					   join r in _context.Roles on ur.RoleId equals r.Id
					   where ur.UserId == userId && (r.Name.ToLower() == "admin") && !ur.IsDeleted && !r.IsDeleted
					   select ur.Id)
					  .AnyAsync(cancellationToken);
	}

	private async Task EnsureGeoRegionAndCustomerTierExistAsync(CancellationToken cancellationToken)
	{
		var regionExists = await _context.GeoRegions.AnyAsync(r => r.Code == "US", cancellationToken);
		if (!regionExists)
		{
			var usRegion = Adrenalin.Modules.Lookup.Domain.Entities.GeoRegion.Create(
				code: "US",
				label: "United States",
				timezone: "UTC",
				businessStart: new TimeOnly(9, 0),
				businessEnd: new TimeOnly(17, 0)
			);
			await _context.GeoRegions.AddAsync(usRegion, cancellationToken);
		}

		var tierExists = await _context.CustomerTiers.AnyAsync(t => t.Code == "Standard", cancellationToken);
		if (!tierExists)
		{
			var standardTier = Adrenalin.Modules.Lookup.Domain.Entities.CustomerTier.Create(
				code: "Standard",
				label: "Standard Support",
				description: "Standard Support Tier",
				priorityBump: 0
			);
			await _context.CustomerTiers.AddAsync(standardTier, cancellationToken);
		}
	}
}