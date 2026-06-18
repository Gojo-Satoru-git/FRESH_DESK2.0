using Adrenalin.Modules.Ticketing.Application.DTOs;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Ticketing.Application.Queries;

public sealed record GetRoutingRulesQuery(Guid? CompanyId = null)
    : IRequest<Result<IReadOnlyList<RoutingRuleDto>>>;

public sealed record GetRoutingRuleByIdQuery(Guid RuleId)
    : IRequest<Result<RoutingRuleDto>>;

public sealed record GetRoutingRuleHistoryQuery(Guid RuleId)
    : IRequest<Result<IReadOnlyList<RoutingRuleHistoryDto>>>;

public sealed record GetCompanyRoutingPreviewQuery(Guid CompanyId)
    : IRequest<Result<CompanyRoutingPreviewDto>>;

public sealed record GetGroupRoutingPreviewQuery(Guid GroupId)
    : IRequest<Result<GroupRoutingPreviewDto>>;
