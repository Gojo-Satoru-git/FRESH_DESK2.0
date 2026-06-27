using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Routing;

public sealed record GroupRoutingPreviewDto(
    Guid GroupId,
    string? GroupName,
    IReadOnlyList<CompanyDefaultRouteDto> DefaultForCompanies,
    IReadOnlyList<RoutingRuleDto> ExplicitRulesTargetingGroup
);

public sealed record CompanyDefaultRouteDto(
    Guid CompanyId,
    string? CompanyName
);
