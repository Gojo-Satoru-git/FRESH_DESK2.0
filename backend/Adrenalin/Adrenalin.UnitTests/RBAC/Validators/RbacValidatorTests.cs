using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Adrenalin.UnitTests.RBAC.Validators;

// ─────────────────────────────────────────────────────────────────────────────
// CreateRoleCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class CreateRoleCommandValidatorTests
{
    private readonly CreateRoleCommandValidator _sut = new();

    private static CreateRoleCommand ValidCommand() =>
        new("Support Agent", "Handles tickets", Guid.NewGuid());

    [Fact]
    public void CreateRoleCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateRoleCommandValidator_Should_Have_Error_When_Name_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { Name = string.Empty });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateRoleCommandValidator_Should_Have_Error_When_Name_Exceeds_80_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { Name = new string('a', 81) });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateRoleCommandValidator_Should_Not_Have_Error_When_Name_Is_Exactly_80_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { Name = new string('a', 80) });
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateRoleCommandValidator_Should_Have_Error_When_Description_Exceeds_500_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { Description = new string('a', 501) });
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateRoleCommandValidator_Should_Not_Have_Error_When_Description_Is_Null()
    {
        var result = _sut.TestValidate(ValidCommand() with { Description = null });
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateRoleCommandValidator_Should_Not_Have_Error_When_Description_Is_Exactly_500_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { Description = new string('a', 500) });
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateRoleCommandValidator_Should_Have_Error_When_ActorId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { ActorId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ActorId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UpdateRoleCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class UpdateRoleCommandValidatorTests
{
    private readonly UpdateRoleCommandValidator _sut = new();

    private static UpdateRoleCommand ValidCommand() =>
        new(Guid.NewGuid(), "Support Agent", null, Guid.NewGuid());

    [Fact]
    public void UpdateRoleCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateRoleCommandValidator_Should_Have_Error_When_RoleId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { RoleId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void UpdateRoleCommandValidator_Should_Have_Error_When_Name_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { Name = string.Empty });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateRoleCommandValidator_Should_Have_Error_When_Name_Exceeds_80_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { Name = new string('a', 81) });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void UpdateRoleCommandValidator_Should_Have_Error_When_ActorId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { ActorId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ActorId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// DeleteRoleCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class DeleteRoleCommandValidatorTests
{
    private readonly DeleteRoleCommandValidator _sut = new();

    private static DeleteRoleCommand ValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void DeleteRoleCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void DeleteRoleCommandValidator_Should_Have_Error_When_RoleId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { RoleId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void DeleteRoleCommandValidator_Should_Have_Error_When_ActorId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { ActorId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ActorId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreatePermissionCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class CreatePermissionCommandValidatorTests
{
    private readonly CreatePermissionCommandValidator _sut = new();

    private static CreatePermissionCommand ValidCommand() =>
        new("ticket", "read", "Read tickets", Guid.NewGuid());

    [Fact]
    public void CreatePermissionCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_Resource_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { Resource = string.Empty });
        result.ShouldHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_Resource_Exceeds_60_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { Resource = new string('a', 61) });
        result.ShouldHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_Resource_Contains_Uppercase()
    {
        var result = _sut.TestValidate(ValidCommand() with { Resource = "Ticket" });
        result.ShouldHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_Resource_Contains_Spaces()
    {
        var result = _sut.TestValidate(ValidCommand() with { Resource = "my ticket" });
        result.ShouldHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Not_Have_Error_When_Resource_Has_Colon()
    {
        // resource allows colons per regex ^[a-z_:]+$
        var result = _sut.TestValidate(ValidCommand() with { Resource = "ticket:sub" });
        result.ShouldNotHaveValidationErrorFor(x => x.Resource);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_Action_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { Action = string.Empty });
        result.ShouldHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_Action_Exceeds_60_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { Action = new string('a', 61) });
        result.ShouldHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_Action_Contains_Uppercase()
    {
        var result = _sut.TestValidate(ValidCommand() with { Action = "Read" });
        result.ShouldHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_Action_Contains_Colon()
    {
        // action regex ^[a-z_]+$ - no colons
        var result = _sut.TestValidate(ValidCommand() with { Action = "read:all" });
        result.ShouldHaveValidationErrorFor(x => x.Action);
    }

    [Fact]
    public void CreatePermissionCommandValidator_Should_Have_Error_When_ActorId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { ActorId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ActorId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SetRolePermissionsCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class SetRolePermissionsCommandValidatorTests
{
    private readonly SetRolePermissionsCommandValidator _sut = new();

    private static SetRolePermissionsCommand ValidCommand() =>
        new(Guid.NewGuid(), new[] { Guid.NewGuid(), Guid.NewGuid() }.ToList().AsReadOnly(), Guid.NewGuid());

    [Fact]
    public void SetRolePermissionsCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SetRolePermissionsCommandValidator_Should_Have_Error_When_RoleId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { RoleId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void SetRolePermissionsCommandValidator_Should_Have_Error_When_PermissionIds_Contains_EmptyGuid()
    {
        var result = _sut.TestValidate(ValidCommand() with
        {
            PermissionIds = new[] { Guid.Empty }.ToList().AsReadOnly()
        });
        result.ShouldHaveValidationErrorFor(x => x.PermissionIds);
    }

    [Fact]
    public void SetRolePermissionsCommandValidator_Should_Not_Have_Error_For_Empty_PermissionIds_List()
    {
        // Empty list is valid (clears all permissions)
        var result = _sut.TestValidate(ValidCommand() with
        {
            PermissionIds = Array.Empty<Guid>().ToList().AsReadOnly()
        });
        result.ShouldNotHaveValidationErrorFor(x => x.PermissionIds);
    }

    [Fact]
    public void SetRolePermissionsCommandValidator_Should_Have_Error_When_ActorId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { ActorId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ActorId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// AssignRoleToUserCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class AssignRoleToUserCommandValidatorTests
{
    private readonly AssignRoleToUserCommandValidator _sut = new();

    private static AssignRoleToUserCommand ValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void AssignRoleToUserCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AssignRoleToUserCommandValidator_Should_Have_Error_When_UserId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { UserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void AssignRoleToUserCommandValidator_Should_Have_Error_When_RoleId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { RoleId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void AssignRoleToUserCommandValidator_Should_Have_Error_When_ActorId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { ActorId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ActorId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// SetUserRolesCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class SetUserRolesCommandValidatorTests
{
    private readonly SetUserRolesCommandValidator _sut = new();

    private static SetUserRolesCommand ValidCommand() =>
        new(Guid.NewGuid(), new[] { Guid.NewGuid() }.ToList().AsReadOnly(), Guid.NewGuid());

    [Fact]
    public void SetUserRolesCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SetUserRolesCommandValidator_Should_Have_Error_When_RoleIds_Contains_EmptyGuid()
    {
        var result = _sut.TestValidate(ValidCommand() with
        {
            RoleIds = new[] { Guid.Empty }.ToList().AsReadOnly()
        });
        result.ShouldHaveValidationErrorFor(x => x.RoleIds);
    }

    [Fact]
    public void SetUserRolesCommandValidator_Should_Have_Error_When_UserId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { UserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CreateGroupCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class CreateGroupCommandValidatorTests
{
    private readonly CreateGroupCommandValidator _sut = new();

    private static CreateGroupCommand ValidCommand() =>
        new("Support Team", "EU", "T1", 30, 0, null, Guid.NewGuid());

    [Fact]
    public void CreateGroupCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Have_Error_When_Name_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { Name = string.Empty });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Have_Error_When_Name_Exceeds_100_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { Name = new string('a', 101) });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Have_Error_When_RegionCode_Exceeds_20_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { RegionCode = new string('a', 21) });
        result.ShouldHaveValidationErrorFor(x => x.RegionCode);
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Not_Have_Error_When_RegionCode_Is_Null()
    {
        var result = _sut.TestValidate(ValidCommand() with { RegionCode = null });
        result.ShouldNotHaveValidationErrorFor(x => x.RegionCode);
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Have_Error_When_TierCode_Exceeds_10_Characters()
    {
        var result = _sut.TestValidate(ValidCommand() with { TierCode = new string('a', 11) });
        result.ShouldHaveValidationErrorFor(x => x.TierCode);
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Have_Error_When_UnattendedAlertMinutes_Is_Zero()
    {
        var result = _sut.TestValidate(ValidCommand() with { UnattendedAlertMinutes = 0 });
        result.ShouldHaveValidationErrorFor(x => x.UnattendedAlertMinutes);
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Have_Error_When_UnattendedAlertMinutes_Is_Negative()
    {
        var result = _sut.TestValidate(ValidCommand() with { UnattendedAlertMinutes = -5 });
        result.ShouldHaveValidationErrorFor(x => x.UnattendedAlertMinutes);
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Not_Have_Error_When_UnattendedAlertMinutes_Is_1()
    {
        var result = _sut.TestValidate(ValidCommand() with { UnattendedAlertMinutes = 1 });
        result.ShouldNotHaveValidationErrorFor(x => x.UnattendedAlertMinutes);
    }

    [Fact]
    public void CreateGroupCommandValidator_Should_Have_Error_When_ActorId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { ActorId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ActorId);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// AddUserToGroupCommandValidator
// ─────────────────────────────────────────────────────────────────────────────

public sealed class AddUserToGroupCommandValidatorTests
{
    private readonly AddUserToGroupCommandValidator _sut = new();

    private static AddUserToGroupCommand ValidCommand() =>
        new(Guid.NewGuid(), Guid.NewGuid(), false, Guid.NewGuid());

    [Fact]
    public void AddUserToGroupCommandValidator_Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void AddUserToGroupCommandValidator_Should_Have_Error_When_UserId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { UserId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void AddUserToGroupCommandValidator_Should_Have_Error_When_GroupId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { GroupId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.GroupId);
    }

    [Fact]
    public void AddUserToGroupCommandValidator_Should_Have_Error_When_ActorId_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { ActorId = Guid.Empty });
        result.ShouldHaveValidationErrorFor(x => x.ActorId);
    }
}
