using Adrenalin.Modules.Company.Application.DTOs;
using Adrenalin.Modules.Company.Application.Queries;
using Adrenalin.Persistence.Context;
using Adrenalin.SharedKernel.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Adrenalin.Persistence.Repositories;

public sealed class CompanyQueryService : ICompanyQueryService
{
    private readonly AdrenalinDbContext _db;

    public CompanyQueryService(AdrenalinDbContext db)
    {
        _db = db;
    }

    public async Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId && !c.IsDeleted)
            .Select(c => new CompanyDetailDto(
                c.Id,
                c.Name,
                c.CspId,
                c.GeoRegion,
                c.SupportTier,
                c.Industry,
                c.HealthScore,
                c.IsActive,
                c.IsPayrollCustomer,
                c.PaymentOnHold,
                c.DeliverySupportActive,
                c.LeaveCreditCycle,
                c.MigrationDate,
                c.Notes,
                c.CamUserId,
                c.DeliveryManagerId,
                c.Contacts.Count(ct => !ct.IsDeleted),
                c.CompanyContactsLimit != null ? c.CompanyContactsLimit.MaxContacts : 20,
                c.CreatedAt,
                c.UpdatedAt,
                c.CreatedBy,
                c.UpdatedBy,
                c.CompanyDomains
                    .Where(d => !d.IsDeleted)
                    .Select(d => new CompanyDomainDto(
                        d.Id,
                        d.CompanyId,
                        d.Domain,
                        d.IsPrimary,
                        false,
                        null))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<CompanyListItemDto>> ListCompaniesAsync(
        string? geoRegion, string? supportTier, bool? isActive,
        string? sortBy, bool sortDescending,
        int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(geoRegion))
            query = query.Where(c => c.GeoRegion == geoRegion);

        if (!string.IsNullOrWhiteSpace(supportTier))
            query = query.Where(c => c.SupportTier == supportTier);

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        // Sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => sortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "healthscore" => sortDescending ? query.OrderByDescending(c => c.HealthScore) : query.OrderBy(c => c.HealthScore),
            "createdat" => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "supporttier" => sortDescending ? query.OrderByDescending(c => c.SupportTier) : query.OrderBy(c => c.SupportTier),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyListItemDto(
                c.Id,
                c.Name,
                c.GeoRegion,
                c.SupportTier,
                c.Industry,
                c.HealthScore,
                c.IsActive,
                c.PaymentOnHold,
                c.CamUserId,
                c.Contacts.Count(ct => !ct.IsDeleted),
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<CompanyListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<CompanyListItemDto>> SearchCompaniesAsync(
        string? term, string? geoRegion, string? supportTier, bool? isActive,
        int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Companies
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var lowerTerm = term.ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(lowerTerm) ||
                (c.Industry != null && c.Industry.ToLower().Contains(lowerTerm)) ||
                (c.CspId != null && c.CspId.ToLower().Contains(lowerTerm)));
        }

        if (!string.IsNullOrWhiteSpace(geoRegion))
            query = query.Where(c => c.GeoRegion == geoRegion);

        if (!string.IsNullOrWhiteSpace(supportTier))
            query = query.Where(c => c.SupportTier == supportTier);

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyListItemDto(
                c.Id,
                c.Name,
                c.GeoRegion,
                c.SupportTier,
                c.Industry,
                c.HealthScore,
                c.IsActive,
                c.PaymentOnHold,
                c.CamUserId,
                c.Contacts.Count(ct => !ct.IsDeleted),
                c.CreatedAt,
                c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<CompanyListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<ContactDto>> GetCompanyContactsAsync(
        Guid companyId, string? searchTerm, bool? isAuthorized,
        int page, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Contacts
            .AsNoTracking()
            .Where(c => c.CompanyId == companyId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lower = searchTerm.ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(lower) ||
                c.Email.ToLower().Contains(lower));
        }

        if (isAuthorized.HasValue)
            query = query.Where(c => c.IsAuthorized == isAuthorized.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ContactDto(
                c.Id,
                c.CompanyId,
                c.UserId,
                c.Name,
                c.Email,
                c.Phone,
                c.IsAuthorized,
                c.AutoCreated,
                c.CanViewAllTickets,
                c.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ContactDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<CompanyDomainDto>> GetCompanyDomainsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _db.CompanyDomains
            .AsNoTracking()
            .Where(d => d.CompanyId == companyId && !d.IsDeleted)
            .OrderByDescending(d => d.IsPrimary)
            .ThenBy(d => d.Domain)
            .Select(d => new CompanyDomainDto(
                d.Id,
                d.CompanyId,
                d.Domain,
                d.IsPrimary,
                false,
                null))
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyOwnershipDto?> GetCompanyOwnershipAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId && !c.IsDeleted)
            .Select(c => new CompanyOwnershipDto(
                c.Id,
                c.Name,
                c.CamUserId,
                c.DeliveryManagerId,
                c.DeliverySupportActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CompanyHealthDto?> GetCompanyHealthAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId && !c.IsDeleted)
            .Select(c => new CompanyHealthDto(
                c.Id,
                c.Name,
                c.HealthScore,
                c.DeliverySupportActive,
                c.PaymentOnHold,
                c.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CompanySummaryDto?> GetCompanySummaryAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId && !c.IsDeleted)
            .Select(c => new CompanySummaryDto(
                c.Id,
                c.Name,
                c.SupportTier,
                c.GeoRegion,
                c.HealthScore,
                c.IsActive,
                c.Contacts.Count(ct => !ct.IsDeleted),
                c.CompanyDomains.Count(d => !d.IsDeleted),
                c.CamUserId,
                c.DeliveryManagerId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CompanyContactLimitDto?> GetCompanyContactLimitAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _db.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId && !c.IsDeleted)
            .Select(c => new CompanyContactLimitDto(
                c.Id,
                c.Name,
                c.CompanyContactsLimit != null ? c.CompanyContactsLimit.MaxContacts : 20,
                c.Contacts.Count(ct => !ct.IsDeleted)))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
