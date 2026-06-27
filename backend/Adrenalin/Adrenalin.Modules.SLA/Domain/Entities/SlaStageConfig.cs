// Ported from your branch (aspnet_source_old) into main.
// Maps a workflow stage code to SLA timer behaviour (Run/Pause/Reset).
// No hard FK to the Workflow module — loose coupling by design, since the
// Workflow module doesn't exist in main yet (separate dev, separate branch).
// Matches sla.sla_stage_configs from schema_sla_v11_postgres.sql exactly.

using Adrenalin.SharedKernel.Entities;
using Adrenalin.SharedKernel.Enums.SLA;

namespace Adrenalin.Modules.SLA.Domain.Entities;

public sealed class SlaStageConfig : SoftDeleteEntity
{
    /// <summary>
    /// Stable stage code (e.g. "pending_customer", "in_review"). No hard FK to
    /// Workflow module — loose coupling by design. Stored lowercase.
    /// </summary>
    public string StageCode { get; private set; } = null!;
    public string StageName { get; private set; } = null!;
    public SlaTimerBehaviour TimerBehaviour { get; private set; } = SlaTimerBehaviour.Run;

    /// <summary>
    /// Only relevant when TimerBehaviour = Reset. Which SLA policy to use for
    /// the new clock window. Null = reuse the ticket's original policy.
    /// </summary>
    public Guid? OverridePolicyId { get; private set; }

    public bool IsActive { get; private set; } = true;

    public static SlaStageConfig Create(
        string stageCode, string stageName,
        SlaTimerBehaviour behaviour, Guid? overridePolicyId, Guid? createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stageCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(stageName);

        return new SlaStageConfig
        {
            StageCode = stageCode.ToLowerInvariant().Trim(),
            StageName = stageName.Trim(),
            TimerBehaviour = behaviour,
            OverridePolicyId = overridePolicyId,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public void Update(
        string stageName, SlaTimerBehaviour behaviour,
        Guid? overridePolicyId, Guid? updatedBy)
    {
        StageName = stageName.Trim();
        TimerBehaviour = behaviour;
        OverridePolicyId = overridePolicyId;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate() { IsActive = true; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTimeOffset.UtcNow; }
}
