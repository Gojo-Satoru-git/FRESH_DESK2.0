using Microsoft.EntityFrameworkCore;
using Adrenalin.Modules.Ticketing.Domain.Entities.Email;
using Adrenalin.Modules.Ticketing.Domain.Interfaces;
using Adrenalin.Persistence.Context;

namespace Adrenalin.Persistence.Repositories;

public sealed class EmailAliasRoutingRepository : IEmailAliasRoutingRepository
{
    private readonly AdrenalinDbContext _db;

    public EmailAliasRoutingRepository(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<EmailAliasRouting?> GetBestMatchAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        var email = emailAddress.Trim().ToLowerInvariant();
        var domain = email.Contains('@') ? $"*@{email.Split('@').Last()}" : null;

        var query = _db.Set<EmailAliasRouting>().Where(r => r.IsActive);

        var matches = await query
            .Where(r => r.EmailAddress.ToLower() == email || (domain != null && r.EmailAddress.ToLower() == domain) || r.EmailAddress == "*")
            .ToListAsync(cancellationToken);

        return matches
            .OrderByDescending(r => r.EmailAddress.ToLower() == email ? 3 : (r.EmailAddress.ToLower() == domain ? 2 : 1))
            .ThenByDescending(r => r.Priority)
            .FirstOrDefault();
    }

    public async Task<IReadOnlyList<EmailAliasRouting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Set<EmailAliasRouting>()
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);
    }
}
