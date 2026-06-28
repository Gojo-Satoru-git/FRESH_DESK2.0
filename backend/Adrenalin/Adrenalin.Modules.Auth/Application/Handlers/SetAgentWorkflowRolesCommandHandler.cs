// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Handlers/SetAgentWorkflowRolesCommandHandler.cs
// NEW FILE

using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Handlers;

/// <summary>
/// FR-RP-040 — only Active Workflow Roles may be assigned.
/// FS-05 §3.4 cardinality — exactly one Primary, zero-or-more Additional.
/// FR-RP-045-equivalent — audited via IAuditLogWriter.
/// </summary>
public sealed class SetAgentWorkflowRolesCommandHandler : IRequestHandler<SetAgentWorkflowRolesCommand, Result>
{
    private readonly IWorkflowRoleRepository _roles;
    private readonly IUserWorkflowRoleRepository _assignments;
    private readonly IAuditLogWriter _audit;

    public SetAgentWorkflowRolesCommandHandler(
        IWorkflowRoleRepository roles,
        IUserWorkflowRoleRepository assignments,
        IAuditLogWriter audit)
    {
        _roles = roles;
        _assignments = assignments;
        _audit = audit;
    }

    public async Task<Result> Handle(SetAgentWorkflowRolesCommand cmd, CancellationToken ct)
    {
        var primaryRole = await _roles.GetByIdAsync(cmd.PrimaryWorkflowRoleId, ct);
        if (primaryRole is null || primaryRole.IsDeleted)
            return Result.Failure("Primary Workflow Role not found.");

        // FR-RP-040 — Agent Editor picker only offers Active roles; the handler re-checks
        // server-side rather than trusting the client sent an active ID.
        if (!primaryRole.IsActive)
            return Result.Failure($"Workflow Role '{primaryRole.Name}' is inactive and cannot be newly assigned.");

        var distinctAdditionalIds = cmd.AdditionalWorkflowRoleIds.Distinct().Where(id => id != cmd.PrimaryWorkflowRoleId).ToList();
        var additionalRoles = new List<WorkflowRole>();
        foreach (var id in distinctAdditionalIds)
        {
            var role = await _roles.GetByIdAsync(id, ct);
            if (role is null || role.IsDeleted)
                return Result.Failure($"Additional Workflow Role {id} not found.");
            if (!role.IsActive)
                return Result.Failure($"Workflow Role '{role.Name}' is inactive and cannot be newly assigned.");
            additionalRoles.Add(role);
        }

        var existing = await _assignments.GetByUserAsync(cmd.UserId, ct);
        var oldSummary = existing.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(new
        {
            roles = existing.Select(e => new { name = e.WorkflowRole?.Name, isPrimary = e.IsPrimary })
        });
        // Soft-delete everything not in the new target set; this keeps the operation
        // idempotent and avoids duplicate-key issues on the (UserId, WorkflowRoleId) unique index.
        var targetIds = new HashSet<Guid> { cmd.PrimaryWorkflowRoleId };
        foreach (var id in distinctAdditionalIds) targetIds.Add(id);

        foreach (var assignment in existing.Where(e => !targetIds.Contains(e.WorkflowRoleId)))
        {
            assignment.SoftDelete(cmd.ActorId);
            _assignments.Update(assignment);
        }

        // Upsert Primary.
        var primaryAssignment = existing.FirstOrDefault(e => e.WorkflowRoleId == cmd.PrimaryWorkflowRoleId);
        if (primaryAssignment is null)
        {
            primaryAssignment = UserWorkflowRole.Create(cmd.UserId, cmd.PrimaryWorkflowRoleId, isPrimary: true, cmd.ActorId);
            _assignments.Add(primaryAssignment);
        }
        else
        {
            primaryAssignment.MakePrimary(cmd.ActorId);
            _assignments.Update(primaryAssignment);
        }

        // Demote any OTHER row that was previously primary (DB partial-unique-index on
        // is_primary=true would otherwise reject this — must flip the old one first).
        foreach (var other in existing.Where(e => e.IsPrimary && e.WorkflowRoleId != cmd.PrimaryWorkflowRoleId))
        {
            other.MakeAdditional(cmd.ActorId);
            _assignments.Update(other);
        }

        // Upsert Additional roles.
        foreach (var id in distinctAdditionalIds)
        {
            var assignment = existing.FirstOrDefault(e => e.WorkflowRoleId == id);
            if (assignment is null)
            {
                _assignments.Add(UserWorkflowRole.Create(cmd.UserId, id, isPrimary: false, cmd.ActorId));
            }
            else if (assignment.IsPrimary)
            {
                assignment.MakeAdditional(cmd.ActorId);
                _assignments.Update(assignment);
            }
        }

        await _assignments.SaveChangesAsync(ct);

        var newSummary = System.Text.Json.JsonSerializer.Serialize(new
        {
            roles = new[] { new { name = primaryRole.Name, isPrimary = true } }
        .Concat(additionalRoles.Select(r => new { name = r.Name, isPrimary = false }))
        });

        await _audit.WriteAsync(
            tableName: "user_workflow_roles",
            recordId: cmd.UserId,
            changeType: "AgentWorkflowRolesChanged",
            actorId: cmd.ActorId,
            oldValues: oldSummary,
            newValues: newSummary,
            ct: ct);

        return Result.Success();
    }
}
