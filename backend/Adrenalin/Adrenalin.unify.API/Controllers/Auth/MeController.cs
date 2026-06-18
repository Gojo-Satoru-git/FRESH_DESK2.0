using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Adrenalin.Persistence.Context;

namespace Adrenalin.unify.API.Controllers.Auth;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly AdrenalinDbContext _context;

    public MeController(AdrenalinDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(sub, out var userId))
        {
            return Unauthorized();
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return NotFound(new { error = "User not found." });
        }

        var permissions = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions, 
                  ur => ur.RoleId, 
                  rp => rp.RoleId, 
                  (ur, rp) => rp.PermissionId)
            .Join(_context.Set<Adrenalin.Modules.Auth.Domain.Entities.Permission>(),
                  pId => pId,
                  p => p.Id,
                  (pId, p) => p.Resource + ":" + p.Action)
            .Distinct()
            .ToListAsync(cancellationToken);

        var groups = await _context.UserGroups
            .AsNoTracking()
            .Where(ug => ug.UserId == userId)
            .Select(ug => ug.GroupId)
            .ToListAsync(cancellationToken);

        var contact = await _context.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);

        var name = !string.IsNullOrWhiteSpace(user.FirstName) 
            ? $"{user.FirstName} {user.LastName}".Trim() 
            : user.Email.Split('@').First();

        return Ok(new {
            id = user.Id,
            email = user.Email,
            name = name,
            permissions = permissions,
            groups = groups,
            companyId = contact?.CompanyId,
            contactId = contact?.Id
        });
    }
}
