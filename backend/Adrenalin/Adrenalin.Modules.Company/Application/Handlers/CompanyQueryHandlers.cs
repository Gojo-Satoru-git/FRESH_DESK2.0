using Adrenalin.Modules.Company.Application.DTOs;
using Adrenalin.Modules.Company.Application.Queries;
using Adrenalin.SharedKernel.Exceptions.Company;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Pagination;

namespace Adrenalin.Modules.Company.Application.Handlers;

public sealed class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDetailDto>
{
    private readonly ICompanyQueryService _queryService;

    public GetCompanyByIdQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<CompanyDetailDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _queryService.GetCompanyByIdAsync(request.CompanyId, cancellationToken);
        return company ?? throw new CompanyNotFoundException(request.CompanyId);
    }
}

public sealed class ListCompaniesQueryHandler : IRequestHandler<ListCompaniesQuery, PagedResult<CompanyListItemDto>>
{
    private readonly ICompanyQueryService _queryService;

    public ListCompaniesQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PagedResult<CompanyListItemDto>> Handle(ListCompaniesQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.ListCompaniesAsync(
            request.GeoRegion, request.SupportTier, request.IsActive,
            request.SortBy, request.SortDescending,
            request.Page, request.PageSize,
            cancellationToken);
    }
}

public sealed class SearchCompaniesQueryHandler : IRequestHandler<SearchCompaniesQuery, PagedResult<CompanyListItemDto>>
{
    private readonly ICompanyQueryService _queryService;

    public SearchCompaniesQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PagedResult<CompanyListItemDto>> Handle(SearchCompaniesQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.SearchCompaniesAsync(
            request.Term, request.GeoRegion, request.SupportTier, request.IsActive,
            request.Page, request.PageSize,
            cancellationToken);
    }
}

public sealed class GetCompanyContactsQueryHandler : IRequestHandler<GetCompanyContactsQuery, PagedResult<ContactDto>>
{
    private readonly ICompanyQueryService _queryService;

    public GetCompanyContactsQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PagedResult<ContactDto>> Handle(GetCompanyContactsQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetCompanyContactsAsync(
            request.CompanyId, request.SearchTerm, request.IsAuthorized,
            request.Page, request.PageSize,
            cancellationToken);
    }
}

public sealed class GetCompanyDomainsQueryHandler : IRequestHandler<GetCompanyDomainsQuery, IReadOnlyList<CompanyDomainDto>>
{
    private readonly ICompanyQueryService _queryService;

    public GetCompanyDomainsQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<IReadOnlyList<CompanyDomainDto>> Handle(GetCompanyDomainsQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetCompanyDomainsAsync(request.CompanyId, cancellationToken);
    }
}

public sealed class GetCompanyOwnershipQueryHandler : IRequestHandler<GetCompanyOwnershipQuery, CompanyOwnershipDto>
{
    private readonly ICompanyQueryService _queryService;

    public GetCompanyOwnershipQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<CompanyOwnershipDto> Handle(GetCompanyOwnershipQuery request, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetCompanyOwnershipAsync(request.CompanyId, cancellationToken);
        return result ?? throw new CompanyNotFoundException(request.CompanyId);
    }
}

public sealed class GetCompanyHealthQueryHandler : IRequestHandler<GetCompanyHealthQuery, CompanyHealthDto>
{
    private readonly ICompanyQueryService _queryService;

    public GetCompanyHealthQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<CompanyHealthDto> Handle(GetCompanyHealthQuery request, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetCompanyHealthAsync(request.CompanyId, cancellationToken);
        return result ?? throw new CompanyNotFoundException(request.CompanyId);
    }
}

public sealed class GetCompanySummaryQueryHandler : IRequestHandler<GetCompanySummaryQuery, CompanySummaryDto>
{
    private readonly ICompanyQueryService _queryService;

    public GetCompanySummaryQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<CompanySummaryDto> Handle(GetCompanySummaryQuery request, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetCompanySummaryAsync(request.CompanyId, cancellationToken);
        return result ?? throw new CompanyNotFoundException(request.CompanyId);
    }
}

public sealed class GetCompanyContactLimitQueryHandler : IRequestHandler<GetCompanyContactLimitQuery, CompanyContactLimitDto>
{
    private readonly ICompanyQueryService _queryService;

    public GetCompanyContactLimitQueryHandler(ICompanyQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<CompanyContactLimitDto> Handle(GetCompanyContactLimitQuery request, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetCompanyContactLimitAsync(request.CompanyId, cancellationToken);
        return result ?? throw new CompanyNotFoundException(request.CompanyId);
    }
}
