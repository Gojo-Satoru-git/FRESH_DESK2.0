using System.Collections.Generic;

namespace Adrenalin.Modules.Ticketing.Application.DTOs;

public sealed record RoutingTraceDto(
    string EvaluationStep,
    bool Matched,
    string Reason,
    Guid? RuleId = null);

public sealed record RoutingSimulationResultDto(
    Guid? ResolvedGroupId,
    Guid? ResolvedAgentId,
    string FinalStrategy,
    string? FinalReason,
    IReadOnlyList<RoutingTraceDto> Traces);
