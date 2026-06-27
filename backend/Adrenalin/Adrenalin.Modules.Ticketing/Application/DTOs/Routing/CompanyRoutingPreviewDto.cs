using System;
using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.DTOs.Routing;

public sealed record CompanyRoutingPreviewDto(
    Guid CompanyId,
    string? CompanyName,
    Guid? DefaultGroupId,
    string? DefaultGroupName,
    IReadOnlyList<RoutingRuleDto> ExplicitRules,
    IReadOnlyList<string> Warnings
);
