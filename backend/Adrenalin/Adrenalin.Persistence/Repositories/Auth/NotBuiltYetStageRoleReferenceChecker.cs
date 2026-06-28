// FILE: Adrenalin/Adrenalin.Persistence/Repositories/Auth/NotBuiltYetStageRoleReferenceChecker.cs
// NEW FILE — temporary stand-in, replace once FS-02 stage tables exist.

using Adrenalin.Modules.Auth.Domain.Interfaces;

namespace Adrenalin.Persistence.Repositories.Auth;

/// <summary>
/// Stand-in for IStageRoleReferenceChecker until FS-02 (Workflow Engine — Stages &amp;
/// Assignment) actually exists in this codebase. Always returns 0, so deletion is never
/// blocked on the "referenced by a stage" half of FR-RP-006 — only on agent assignment.
/// Replace this registration in DI as soon as the real stage config table is built.
/// </summary>
public sealed class NotBuiltYetStageRoleReferenceChecker : IStageRoleReferenceChecker
{
    public Task<int> CountReferencingStagesAsync(Guid workflowRoleId, CancellationToken ct = default)
        => Task.FromResult(0);
}
