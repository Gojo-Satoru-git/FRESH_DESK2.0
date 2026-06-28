// FILE: Adrenalin/Adrenalin.Modules.Auth/Application/Validators/WorkflowRoleCommandValidators.cs
// NEW FILE

using Adrenalin.Modules.Auth.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Auth.Application.Validators;

public sealed class CreateWorkflowRoleCommandValidator : AbstractValidator<CreateWorkflowRoleCommand>
{
    public CreateWorkflowRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(2, 80)
            .WithMessage("Role Name must be 2-80 characters.");

        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class RenameWorkflowRoleCommandValidator : AbstractValidator<RenameWorkflowRoleCommand>
{
    public RenameWorkflowRoleCommandValidator()
    {
        RuleFor(x => x.WorkflowRoleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().Length(2, 80);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class DeactivateWorkflowRoleCommandValidator : AbstractValidator<DeactivateWorkflowRoleCommand>
{
    public DeactivateWorkflowRoleCommandValidator()
    {
        RuleFor(x => x.WorkflowRoleId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class ReactivateWorkflowRoleCommandValidator : AbstractValidator<ReactivateWorkflowRoleCommand>
{
    public ReactivateWorkflowRoleCommandValidator()
    {
        RuleFor(x => x.WorkflowRoleId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class DeleteWorkflowRoleCommandValidator : AbstractValidator<DeleteWorkflowRoleCommand>
{
    public DeleteWorkflowRoleCommandValidator()
    {
        RuleFor(x => x.WorkflowRoleId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
