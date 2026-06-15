using Adrenalin.Modules.Company.Application.DTOs;
using Adrenalin.SharedKernel.Pagination;

namespace Adrenalin.Modules.Company.Application.Queries;

/// <summary>
/// Read-side query service for the Company module.
/// Follows the ITicketQueryService pattern — all queries use AsNoTracking projections.
/// </summary>
public interface ICompanyQueryService
{
    Task<CompanyDetailDto?> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<PagedResult<CompanyListItemDto>> ListCompaniesAsync(
        string? geoRegion, string? supportTier, bool? isActive,
        string? sortBy, bool sortDescending,
        int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<CompanyListItemDto>> SearchCompaniesAsync(
        string? term, string? geoRegion, string? supportTier, bool? isActive,
        int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ContactDto>> GetCompanyContactsAsync(
        Guid companyId, string? searchTerm, bool? isAuthorized,
        int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CompanyDomainDto>> GetCompanyDomainsAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<CompanyOwnershipDto?> GetCompanyOwnershipAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<CompanyHealthDto?> GetCompanyHealthAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<CompanySummaryDto?> GetCompanySummaryAsync(Guid companyId, CancellationToken cancellationToken = default);

    Task<CompanyContactLimitDto?> GetCompanyContactLimitAsync(Guid companyId, CancellationToken cancellationToken = default);
}
