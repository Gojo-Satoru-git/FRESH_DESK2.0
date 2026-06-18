using Adrenalin.Modules.Company.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Company.Application.Queries;

public sealed record GetCompanyGroupsQuery(Guid CompanyId)
    : IRequest<Result<IReadOnlyList<CompanyGroupDto>>>;

public sealed record GetGroupCompaniesQuery(Guid GroupId)
    : IRequest<Result<IReadOnlyList<CompanyGroupDto>>>;
