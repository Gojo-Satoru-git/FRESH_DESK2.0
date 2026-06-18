using Adrenalin.Modules.Company.Application.Commands;
using Adrenalin.Modules.Company.Application.DTOs;
using Adrenalin.Modules.Company.Application.Queries;
using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.Modules.Ticketing.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Adrenalin.unify.API.Controllers;

/// <summary>
/// Manages companies, their domains, and contacts.
/// </summary>
[ApiController]
[Route("api/companies")]
[Authorize]
[Tags("Companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly IDispatcher _dispatcher;

    public CompaniesController(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    // ───────────────────────────────────────────── Company CRUD ─────────────────

    /// <summary>Lists companies with filtering, sorting, and pagination.</summary>
    [HttpGet]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(PagedResult<CompanyListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] ListCompaniesQuery query, CancellationToken ct)
    {
        var result = await _dispatcher.Send(query, ct);
        return Ok(result);
    }

    /// <summary>Gets a company by ID with full details.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(CompanyDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetCompanyByIdQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Searches companies by term across name, industry, and CSP ID.</summary>
    [HttpGet("search")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(PagedResult<CompanyListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] SearchCompaniesQuery query, CancellationToken ct)
    {
        var result = await _dispatcher.Send(query, ct);
        return Ok(result);
    }

    /// <summary>Gets a quick summary for a company.</summary>
    [HttpGet("{id:guid}/summary")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(CompanySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetCompanySummaryQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Gets health information for a company.</summary>
    [HttpGet("{id:guid}/health")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(CompanyHealthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHealth(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetCompanyHealthQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Gets ownership (CAM, Delivery Manager) for a company.</summary>
    [HttpGet("{id:guid}/ownership")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(CompanyOwnershipDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOwnership(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetCompanyOwnershipQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Creates a new company.</summary>
    [HttpPost]
    [Authorize(Policy = "company:create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCompanyCommand command, CancellationToken ct)
    {
        var actorId = GetActorId();
        var cmd = command with { CreatedBy = actorId ?? command.CreatedBy };
        var result = await _dispatcher.Send(cmd, ct);

        return result.IsSuccess
            ? Ok(new { Message = "Company created successfully.", CompanyId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Updates an existing company.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCompanyRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new UpdateCompanyCommand(id, request.Name, request.GeoRegion, request.SupportTier, request.Industry, request.Notes, actorId.Value);
        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { Message = "Company updated successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Soft-deletes a company.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "company:delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new DeleteCompanyCommand(id, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Company deleted successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Restores a soft-deleted company.</summary>
    [HttpPost("{id:guid}/restore")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new RestoreCompanyCommand(id, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Company restored successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Activates a company.</summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new ActivateCompanyCommand(id, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Company activated successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Deactivates a company.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new DeactivateCompanyCommand(id, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Company deactivated successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Assigns a Customer Account Manager (CAM) to a company.</summary>
    [HttpPost("{id:guid}/cam")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignCam(Guid id, [FromBody] AssignCamRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new AssignCamCommand(id, request.CamUserId, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "CAM assigned successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Assigns a Delivery Manager to a company.</summary>
    [HttpPost("{id:guid}/delivery-manager")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignDeliveryManager(Guid id, [FromBody] AssignDeliveryManagerRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new AssignDeliveryManagerCommand(id, request.DeliveryManagerId, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Delivery Manager assigned successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Updates the health score for a company.</summary>
    [HttpPut("{id:guid}/health-score")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHealthScore(Guid id, [FromBody] UpdateHealthScoreRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new UpdateHealthScoreCommand(id, request.Score, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Health score updated successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Updates the support tier for a company.</summary>
    [HttpPut("{id:guid}/tier")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTier(Guid id, [FromBody] UpdateTierRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new UpdateTierCommand(id, request.SupportTier, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Support tier updated successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Updates the contact limit for a company.</summary>
    [HttpPut("{id:guid}/contact-limit")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContactLimit(Guid id, [FromBody] UpdateContactLimitRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new UpdateContactLimitCommand(id, request.MaxContacts, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Contact limit updated successfully." })
            : BadRequest(new { error = result.Error });
    }

    // ───────────────────────────────────────────── Domains ──────────────────────

    /// <summary>Gets all domains for a company.</summary>
    [HttpGet("{id:guid}/domains")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyDomainDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDomains(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetCompanyDomainsQuery(id), ct);
        return Ok(result);
    }

    /// <summary>Adds a domain to a company.</summary>
    [HttpPost("{id:guid}/domains")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddDomain(Guid id, [FromBody] AddDomainRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(
            new AddCompanyDomainCommand(id, request.Domain, request.IsPrimary, actorId.Value), ct);

        return result.IsSuccess
            ? Ok(new { Message = "Domain added successfully.", DomainId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Removes a domain from a company.</summary>
    [HttpDelete("{id:guid}/domains/{domainId:guid}")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDomain(Guid id, Guid domainId, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new DeleteCompanyDomainCommand(id, domainId, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Domain removed successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Sets a domain as primary for a company.</summary>
    [HttpPost("{id:guid}/domains/{domainId:guid}/primary")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimaryDomain(Guid id, Guid domainId, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new SetPrimaryCompanyDomainCommand(id, domainId, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Domain set as primary successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Verifies a company domain.</summary>
    [HttpPost("{id:guid}/domains/{domainId:guid}/verify")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyDomain(Guid id, Guid domainId, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new VerifyCompanyDomainCommand(id, domainId, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Domain verified successfully." })
            : BadRequest(new { error = result.Error });
    }

    // ───────────────────────────────────────────── Groups (Company ↔ Group Mapping) ──────────────────────

    /// <summary>Lists all groups assigned to a company.</summary>
    [HttpGet("{id:guid}/groups")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(IReadOnlyList<CompanyGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroups(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetCompanyGroupsQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>Assigns a company to a support group.</summary>
    [HttpPost("{id:guid}/groups")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignGroup(Guid id, [FromBody] AssignCompanyGroupRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(
            new AssignCompanyToGroupCommand(id, request.GroupId, request.IsDefault, request.Priority, actorId.Value), ct);

        return result.IsSuccess
            ? Ok(new { Message = "Company assigned to group successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Removes a company from a support group.</summary>
    [HttpDelete("{id:guid}/groups/{groupId:guid}")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveGroup(Guid id, Guid groupId, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new RemoveCompanyFromGroupCommand(id, groupId, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Company removed from group successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Sets a group as the default support group for a company.</summary>
    [HttpPost("{id:guid}/groups/{groupId:guid}/default")]
    [Authorize(Policy = "company:update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetDefaultGroup(Guid id, Guid groupId, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var result = await _dispatcher.Send(new SetDefaultCompanyGroupCommand(id, groupId, actorId.Value), ct);
        return result.IsSuccess
            ? Ok(new { Message = "Group set as default successfully." })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>Gets ticket metrics for a company.</summary>
    [HttpGet("{id:guid}/ticket-metrics")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(CompanyTicketMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketMetrics(Guid id, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();
        var result = await _dispatcher.Send(new Adrenalin.Modules.Ticketing.Application.Queries.GetCompanyTicketMetricsQuery(id, actorId.Value), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    /// <summary>Gets routing preview for a company.</summary>
    [HttpGet("{id:guid}/routing-preview")]
    [Authorize(Policy = "company:read")]
    [ProducesResponseType(typeof(CompanyRoutingPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoutingPreview(Guid id, CancellationToken ct)
    {
        var result = await _dispatcher.Send(new GetCompanyRoutingPreviewQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error });
    }

    // ───────────────────────────────────────────── Contacts (under company) ─────

    /// <summary>Gets contacts for a company with filtering and pagination.</summary>
    [HttpGet("{id:guid}/contacts")]
    [Authorize(Policy = "contact:read")]
    [ProducesResponseType(typeof(PagedResult<ContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContacts(
        Guid id,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isAuthorized = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetCompanyContactsQuery(id, searchTerm, isAuthorized, page, pageSize);
        var result = await _dispatcher.Send(query, ct);
        return Ok(result);
    }

    /// <summary>Creates a contact under a company.</summary>
    [HttpPost("{id:guid}/contacts")]
    [Authorize(Policy = "contact:create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContact(Guid id, [FromBody] CreateContactRequest request, CancellationToken ct)
    {
        var actorId = GetActorId();
        if (!actorId.HasValue) return Unauthorized();

        var command = new CreateContactCommand(id, request.Name, request.Email, request.Phone, request.IsAuthorized, actorId.Value);
        var result = await _dispatcher.Send(command, ct);

        return result.IsSuccess
            ? Ok(new { Message = "Contact created successfully.", ContactId = result.Value })
            : BadRequest(new { error = result.Error });
    }

    // ───────────────────────────────────────────── Helper ───────────────────────

    private Guid? GetActorId()
    {
        var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }
}

// ── Request DTOs (kept in controller file following TicketsController pattern) ──

public sealed record UpdateCompanyRequest(
    string Name,
    string GeoRegion,
    string SupportTier,
    string? Industry = null,
    string? Notes = null
);

public sealed record AssignCamRequest(Guid CamUserId);
public sealed record AssignDeliveryManagerRequest(Guid DeliveryManagerId);
public sealed record UpdateHealthScoreRequest(int Score);
public sealed record UpdateTierRequest(string SupportTier);
public sealed record UpdateContactLimitRequest(int MaxContacts);
public sealed record AddDomainRequest(string Domain, bool IsPrimary = false);
public sealed record CreateContactRequest(string Name, string Email, string? Phone = null, bool IsAuthorized = true);
public sealed record AssignCompanyGroupRequest(Guid GroupId, bool IsDefault = false, int Priority = 0);
