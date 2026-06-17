using Adrenalin.Modules.Company.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Pagination;

namespace Adrenalin.Modules.Company.Application.Queries;

/// <summary>Get a single company by ID with full details.</summary>
public sealed record GetCompanyByIdQuery(Guid CompanyId) : IRequest<CompanyDetailDto>;

/// <summary>Paginated list of companies with optional filters.</summary>
public sealed record ListCompaniesQuery(
    string? GeoRegion = null,
    string? SupportTier = null,
    bool? IsActive = null,
    string? SortBy = null,
    bool SortDescending = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CompanyListItemDto>>;

/// <summary>Full-text search across company name, industry, CSP ID.</summary>
public sealed record SearchCompaniesQuery(
    string? Term = null,
    string? GeoRegion = null,
    string? SupportTier = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CompanyListItemDto>>;

/// <summary>Get contacts for a company with pagination.</summary>
public sealed record GetCompanyContactsQuery(
    Guid CompanyId,
    string? SearchTerm = null,
    bool? IsAuthorized = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ContactDto>>;

/// <summary>Get all domains for a company.</summary>
public sealed record GetCompanyDomainsQuery(Guid CompanyId) : IRequest<IReadOnlyList<CompanyDomainDto>>;

/// <summary>Get company ownership (CAM, Delivery Manager).</summary>
public sealed record GetCompanyOwnershipQuery(Guid CompanyId) : IRequest<CompanyOwnershipDto>;

/// <summary>Get company health info.</summary>
public sealed record GetCompanyHealthQuery(Guid CompanyId) : IRequest<CompanyHealthDto>;

/// <summary>Get company summary.</summary>
public sealed record GetCompanySummaryQuery(Guid CompanyId) : IRequest<CompanySummaryDto>;

/// <summary>Get company contact limit info.</summary>
public sealed record GetCompanyContactLimitQuery(Guid CompanyId) : IRequest<CompanyContactLimitDto>;
