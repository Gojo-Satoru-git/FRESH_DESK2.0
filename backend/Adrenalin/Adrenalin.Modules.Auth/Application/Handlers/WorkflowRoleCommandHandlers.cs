// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Handlers/WorkflowRoleCommandHandlers.cs
// NEW FILE

using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;

namespace Adrenalin.Modules.Auth.Application.Handlers;

/// <summary>FR-RP-001, FR-RP-002 (uniqueness), FR-RP-009 (audit).</summary>
public sealed class CreateWorkflowRoleCommandHandler
    : IRequestHandler<CreateWorkflowRoleCommand, Result<Guid>>
{
    private readonly IWorkflowRoleRepository _roles;
    private readonly IAuditLogWriter _audit;

    public CreateWorkflowRoleCommandHandler(IWorkflowRoleRepository roles, IAuditLogWriter audit)
    {
        _roles = roles;
        _audit = audit;
    }

    public async Task<Result<Guid>> Handle(CreateWorkflowRoleCommand cmd, CancellationToken ct)
    {
        // FR-RP-002 — case-insensitive uniqueness, app-layer check (DB index backs this up, see migration SQL).
        if (await _roles.ExistsByNameAsync(cmd.Name, ct))
            return Result<Guid>.Failure($"A Workflow Role named '{cmd.Name}' already exists.");

        WorkflowRole role;
        try
        {
            role = WorkflowRole.Create(cmd.Name, cmd.Description, isSystemDefault: false, cmd.ActorId);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }

        _roles.Add(role);
        await _roles.SaveChangesAsync(ct);

        // FR-RP-009 — audit every create.
        await _audit.WriteAsync(
            tableName: "workflow_roles",
            recordId: role.Id,
            changeType: "WorkflowRoleCreated",
            actorId: cmd.ActorId,
            oldValues: null,
            newValues: $"{{\"name\":\"{role.Name}\",\"description\":\"{role.Description}\",\"isActive\":true}}",
            ct: ct);

        return Result<Guid>.Success(role.Id);
    }
}

/// <summary>FR-RP-003 (edit), FR-RP-002 (uniqueness re-checked on rename), FR-RP-009 (audit).</summary>
public sealed class RenameWorkflowRoleCommandHandler
    : IRequestHandler<RenameWorkflowRoleCommand, Result>
{
    private readonly IWorkflowRoleRepository _roles;
    private readonly IAuditLogWriter _audit;

    public RenameWorkflowRoleCommandHandler(IWorkflowRoleRepository roles, IAuditLogWriter audit)
    {
        _roles = roles;
        _audit = audit;
    }

    public async Task<Result> Handle(RenameWorkflowRoleCommand cmd, CancellationToken ct)
    {
        var role = await _roles.GetByIdAsync(cmd.WorkflowRoleId, ct);
        if (role is null)
            return Result.Failure($"Workflow Role {cmd.WorkflowRoleId} not found.");

        var existing = await _roles.GetByNameAsync(cmd.Name, ct);
        if (existing is not null && existing.Id != role.Id)
            return Result.Failure($"A Workflow Role named '{cmd.Name}' already exists.");

        var oldName = role.Name;
        var oldDescription = role.Description;

        try
        {
            role.Rename(cmd.Name, cmd.Description, cmd.ActorId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        _roles.Update(role);
        await _roles.SaveChangesAsync(ct);

        await _audit.WriteAsync(
            tableName: "workflow_roles",
            recordId: role.Id,
            changeType: "WorkflowRoleEdited",
            actorId: cmd.ActorId,
            oldValues: $"{{\"name\":\"{oldName}\",\"description\":\"{oldDescription}\"}}",
            newValues: $"{{\"name\":\"{role.Name}\",\"description\":\"{role.Description}\"}}",
            ct: ct);

        return Result.Success();
    }
}

/// <summary>FR-RP-004 — hides from pickers, preserves existing assignments (BR-RP-009).</summary>
public sealed class DeactivateWorkflowRoleCommandHandler
    : IRequestHandler<DeactivateWorkflowRoleCommand, Result>
{
    private readonly IWorkflowRoleRepository _roles;
    private readonly IAuditLogWriter _audit;

    public DeactivateWorkflowRoleCommandHandler(IWorkflowRoleRepository roles, IAuditLogWriter audit)
    {
        _roles = roles;
        _audit = audit;
    }

    public async Task<Result> Handle(DeactivateWorkflowRoleCommand cmd, CancellationToken ct)
    {
        var role = await _roles.GetByIdAsync(cmd.WorkflowRoleId, ct);
        if (role is null)
            return Result.Failure($"Workflow Role {cmd.WorkflowRoleId} not found.");

        if (!role.IsActive)
            return Result.Failure("Workflow Role is already inactive.");

        role.Deactivate(cmd.ActorId);
        _roles.Update(role);
        await _roles.SaveChangesAsync(ct);

        await _audit.WriteAsync(
            tableName: "workflow_roles",
            recordId: role.Id,
            changeType: "WorkflowRoleDeactivated",
            actorId: cmd.ActorId,
            oldValues: "{\"isActive\":true}",
            newValues: "{\"isActive\":false}",
            ct: ct);

        return Result.Success();
    }
}

/// <summary>FR-RP-005 — restores visibility immediately.</summary>
public sealed class ReactivateWorkflowRoleCommandHandler
    : IRequestHandler<ReactivateWorkflowRoleCommand, Result>
{
    private readonly IWorkflowRoleRepository _roles;
    private readonly IAuditLogWriter _audit;

    public ReactivateWorkflowRoleCommandHandler(IWorkflowRoleRepository roles, IAuditLogWriter audit)
    {
        _roles = roles;
        _audit = audit;
    }

    public async Task<Result> Handle(ReactivateWorkflowRoleCommand cmd, CancellationToken ct)
    {
        var role = await _roles.GetByIdAsync(cmd.WorkflowRoleId, ct);
        if (role is null)
            return Result.Failure($"Workflow Role {cmd.WorkflowRoleId} not found.");

        if (role.IsActive)
            return Result.Failure("Workflow Role is already active.");

        role.Reactivate(cmd.ActorId);
        _roles.Update(role);
        await _roles.SaveChangesAsync(ct);

        await _audit.WriteAsync(
            tableName: "workflow_roles",
            recordId: role.Id,
            changeType: "WorkflowRoleReactivated",
            actorId: cmd.ActorId,
            oldValues: "{\"isActive\":false}",
            newValues: "{\"isActive\":true}",
            ct: ct);

        return Result.Success();
    }
}

/// <summary>
/// FR-RP-006 / BR-RP-003 — THIS IS THE GUARD THAT WAS MISSING FOR Role (Access Level) IN
/// YOUR EXISTING CODE. Blocks deletion if assigned to any agent OR referenced by any stage.
/// See IStageRoleReferenceChecker's doc comment — the stage half is a known no-op until
/// FS-02's stage config table exists.
/// </summary>
public sealed class DeleteWorkflowRoleCommandHandler
    : IRequestHandler<DeleteWorkflowRoleCommand, Result<WorkflowRoleDeletionBlockedInfo?>>
{
    private readonly IWorkflowRoleRepository _roles;
    private readonly IStageRoleReferenceChecker _stageChecker;
    private readonly IAuditLogWriter _audit;

    public DeleteWorkflowRoleCommandHandler(
        IWorkflowRoleRepository roles,
        IStageRoleReferenceChecker stageChecker,
        IAuditLogWriter audit)
    {
        _roles = roles;
        _stageChecker = stageChecker;
        _audit = audit;
    }

    public async Task<Result<WorkflowRoleDeletionBlockedInfo?>> Handle(
        DeleteWorkflowRoleCommand cmd, CancellationToken ct)
    {
        var role = await _roles.GetByIdAsync(cmd.WorkflowRoleId, ct);
        if (role is null)
            return Result<WorkflowRoleDeletionBlockedInfo?>.Failure($"Workflow Role {cmd.WorkflowRoleId} not found.");

        var assignedAgentCount = await _roles.CountAssignedAgentsAsync(cmd.WorkflowRoleId, ct);
        var referencingStageCount = await _stageChecker.CountReferencingStagesAsync(cmd.WorkflowRoleId, ct);

        if (assignedAgentCount > 0 || referencingStageCount > 0)
        {
            // Blocked — do NOT delete. Caller (controller) surfaces the counts to the UI
            // per FR-RP-006 ("show the affected agents and/or stages").
            return Result<WorkflowRoleDeletionBlockedInfo?>.Success(
                new WorkflowRoleDeletionBlockedInfo(assignedAgentCount, referencingStageCount));
        }

        role.SoftDelete(cmd.ActorId);
        _roles.Update(role);
        await _roles.SaveChangesAsync(ct);

        await _audit.WriteAsync(
            tableName: "workflow_roles",
            recordId: role.Id,
            changeType: "WorkflowRoleDeleted",
            actorId: cmd.ActorId,
            oldValues: $"{{\"name\":\"{role.Name}\"}}",
            newValues: null,
            ct: ct);

        return Result<WorkflowRoleDeletionBlockedInfo?>.Success(null); // null = not blocked, deletion succeeded
    }
}
