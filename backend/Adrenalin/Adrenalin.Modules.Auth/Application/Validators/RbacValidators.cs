using Adrenalin.Modules.Auth.Application.Commands;
using FluentValidation;

namespace Adrenalin.Modules.Auth.Application.Validators;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description is not null);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Resource).NotEmpty().MaximumLength(60)
            .Matches("^[a-z_:]+$").WithMessage("Resource must be lowercase letters, underscores, or colons.");
        RuleFor(x => x.Action).NotEmpty().MaximumLength(60)
            .Matches("^[a-z_]+$").WithMessage("Action must be lowercase letters and underscores.");
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class DeletePermissionCommandValidator : AbstractValidator<DeletePermissionCommand>
{
    public DeletePermissionCommandValidator()
    {
        RuleFor(x => x.PermissionId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class GrantPermissionToRoleCommandValidator : AbstractValidator<GrantPermissionToRoleCommand>
{
    public GrantPermissionToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class RevokePermissionFromRoleCommandValidator : AbstractValidator<RevokePermissionFromRoleCommand>
{
    public RevokePermissionFromRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class SetRolePermissionsCommandValidator : AbstractValidator<SetRolePermissionsCommand>
{
    public SetRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionIds).NotNull()
            .Must(ids => ids.All(id => id != Guid.Empty)).WithMessage("All permission IDs must be valid.");
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class RemoveRoleFromUserCommandValidator : AbstractValidator<RemoveRoleFromUserCommand>
{
    public RemoveRoleFromUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class SetUserAccessLevelCommandValidator : AbstractValidator<SetUserAccessLevelCommand>
{
    public SetUserAccessLevelCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.AccessLevelId).NotNull()
            .Must(id => id != Guid.Empty).WithMessage("Access level ID must be valid.");
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RegionCode).MaximumLength(20).When(x => x.RegionCode is not null);
        RuleFor(x => x.TierCode).MaximumLength(10).When(x => x.TierCode is not null);
        RuleFor(x => x.UnattendedAlertMinutes).GreaterThanOrEqualTo(1);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RegionCode).MaximumLength(20).When(x => x.RegionCode is not null);
        RuleFor(x => x.TierCode).MaximumLength(10).When(x => x.TierCode is not null);
        RuleFor(x => x.UnattendedAlertMinutes).GreaterThanOrEqualTo(1);
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class DeleteGroupCommandValidator : AbstractValidator<DeleteGroupCommand>
{
    public DeleteGroupCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class AddUserToGroupCommandValidator : AbstractValidator<AddUserToGroupCommand>
{
    public AddUserToGroupCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class RemoveUserFromGroupCommandValidator : AbstractValidator<RemoveUserFromGroupCommand>
{
    public RemoveUserFromGroupCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}

public sealed class SetGroupLeadCommandValidator : AbstractValidator<SetGroupLeadCommand>
{
    public SetGroupLeadCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.ActorId).NotEmpty();
    }
}
