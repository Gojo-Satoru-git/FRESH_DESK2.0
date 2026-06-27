// Ported from your branch and adapted to main's conventions: main uses one
// flat ISlaRepository (not per-entity repos) and IUnitOfWork.SaveChangesAsync
// at the end of a handler (see CheckEscalationsCommandHandler), rather than
// repo.SaveChangesAsync() per call. Behaviour is identical to the branch.

using Adrenalin.SharedKernel.Enums.SLA;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.SLA.Application.Commands;

/// <summary>
/// Fired when a ticket enters a workflow stage. No hard dependency on the
/// Workflow module — StageCode is a plain string, matching the loose-coupling
/// already used by sla.sla_stage_configs (see schema_sla_v11_postgres.sql).
/// Safe to add now even though the Workflow module doesn't exist yet; nothing
/// calls this until that module's stage-transition code does.
/// </summary>
public sealed record HandleTicketEnteredStageCommand(
    Guid TicketId,
    string TenantId,
    string StageCode,
    string? FromStageCode,
    DateTime EnteredAtUtc) : IRequest<Result>;

/// <summary>Fired when a ticket leaves a workflow stage (mirrors entered).</summary>
public sealed record HandleTicketLeftStageCommand(
    Guid TicketId,
    string TenantId,
    string StageCode,
    string? ToStageCode,
    DateTime LeftAtUtc) : IRequest<Result>;

/// <summary>Admin CRUD for per-stage SLA timer behaviour (Run/Pause/Reset).</summary>
public sealed record UpsertSlaStageConfigCommand(
    string StageCode,
    string StageName,
    SlaTimerBehaviour TimerBehaviour,
    Guid? OverridePolicyId,
    Guid ActorId) : IRequest<Result<Guid>>;
