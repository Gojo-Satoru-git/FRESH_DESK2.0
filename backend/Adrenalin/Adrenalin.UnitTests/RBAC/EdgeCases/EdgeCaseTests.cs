using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Handlers;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Adrenalin.UnitTests.RBAC.EdgeCases;

// ═══════════════════════════════════════════════════════════════════════════
// Null / Empty Value Edge Cases
// ═══════════════════════════════════════════════════════════════════════════

public sealed class NullAndEmptyEdgeCaseTests
{
    [Fact]
    public void Role_Create_Should_Accept_Null_Description()
    {
        var act = () => Role.Create("Agent", null, Guid.NewGuid());
        act.Should().NotThrow();
    }

    [Fact]
    public void Permission_Create_Should_Accept_Null_Description()
    {
        var act = () => Permission.Create("ticket", "read", null, Guid.NewGuid());
        act.Should().NotThrow();
    }

    [Fact]
    public void Group_Create_Should_Accept_Null_RegionCode_And_TierCode()
    {
        var act = () => Group.Create("Team", null, null, 30, Guid.NewGuid());
        act.Should().NotThrow();
    }

    [Fact]
    public void Role_Create_Should_Set_Empty_Collections_Not_Null()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());

        role.UserRoles.Should().NotBeNull();
        role.RolePermissions.Should().NotBeNull();
    }

    [Fact]
    public void Permission_Create_Should_Set_Empty_RolePermissions_Not_Null()
    {
        var perm = Permission.Create("ticket", "read", null, Guid.NewGuid());

        perm.RolePermissions.Should().NotBeNull();
    }

    [Fact]
    public void Group_Create_Should_Set_Empty_UserGroups_Not_Null()
    {
        var group = Group.Create("Team", null, null, 30, Guid.NewGuid());

        group.UserGroups.Should().NotBeNull();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Duplicate Assignment Edge Cases
// ═══════════════════════════════════════════════════════════════════════════

public sealed class DuplicateAssignmentEdgeCaseTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPermissionRepository> _permRepo = new();
    private readonly Mock<IRolePermissionRepository> _rpRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IUserRoleRepository> _urRepo = new();

    [Fact]
    public async Task GrantPermission_Should_Be_Idempotent_When_Already_Granted()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());
        var perm = Permission.Create("ticket", "read", null, Guid.NewGuid());
        var existing = RolePermission.Assign(role.Id, perm.Id, Guid.NewGuid());

        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _permRepo.Setup(r => r.GetByIdAsync(perm.Id, default)).ReturnsAsync(perm);
        _rpRepo.Setup(r => r.GetAsync(role.Id, perm.Id, default)).ReturnsAsync(existing);

        var handler = new GrantPermissionToRoleCommandHandler(_roleRepo.Object, _permRepo.Object, _rpRepo.Object);
        var result = await handler.Handle(new GrantPermissionToRoleCommand(role.Id, perm.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _rpRepo.Verify(r => r.Add(It.IsAny<RolePermission>()), Times.Never);
        _rpRepo.Verify(r => r.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUser_Should_Be_Idempotent_When_Already_Active()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var existing = UserRole.Assign(userId, roleId, Guid.NewGuid());
        var user = CreateUser(userId);
        var role = Role.Create("Agent", null, Guid.NewGuid());

        _userRepo.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(roleId, default)).ReturnsAsync(role);
        _urRepo.Setup(r => r.GetAsync(userId, roleId, default)).ReturnsAsync(existing);

        var handler = new AssignRoleToUserCommandHandler(_userRepo.Object, _roleRepo.Object, _urRepo.Object);
        var result = await handler.Handle(new AssignRoleToUserCommand(userId, roleId, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _urRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task CreateRole_Should_Reject_Duplicate_Name()
    {
        _roleRepo.Setup(r => r.ExistsByNameAsync("Agent", default)).ReturnsAsync(true);

        var handler = new CreateRoleCommandHandler(_roleRepo.Object);
        var result = await handler.Handle(new CreateRoleCommand("Agent", null, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Agent");
        _roleRepo.Verify(r => r.Add(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task CreatePermission_Should_Reject_Duplicate_Resource_Action_Pair()
    {
        var permRepo = new Mock<IPermissionRepository>();
        permRepo.Setup(r => r.ExistsAsync("ticket", "read", default)).ReturnsAsync(true);

        var handler = new CreatePermissionCommandHandler(permRepo.Object);
        var result = await handler.Handle(new CreatePermissionCommand("ticket", "read", null, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("ticket:read");
    }

    private static User CreateUser(Guid id)
    {
        var user = (User)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(User));
        typeof(User).GetProperty("Id")!.SetValue(user, id);
        typeof(User).GetProperty("Email")!.SetValue(user, "test@example.com");
        return user;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Deleted / Inactive Entity Edge Cases
// ═══════════════════════════════════════════════════════════════════════════

public sealed class DeletedEntityEdgeCaseTests
{
    [Fact]
    public void Role_Update_Should_Throw_When_Role_Is_Already_Deleted()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());
        role.SoftDelete(Guid.NewGuid());

        var act = () => role.Update("New Name", null, Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot modify a deleted role*");
    }

    [Fact]
    public void Role_SoftDelete_Should_Throw_When_Already_Deleted()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());
        role.SoftDelete(Guid.NewGuid());

        var act = () => role.SoftDelete(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already deleted*");
    }

    [Fact]
    public void Permission_SoftDelete_Should_Throw_When_Already_Deleted()
    {
        var perm = Permission.Create("ticket", "read", null, Guid.NewGuid());
        perm.SoftDelete(Guid.NewGuid());

        var act = () => perm.SoftDelete(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already deleted*");
    }

    [Fact]
    public void RolePermission_SoftDelete_Should_Throw_When_Already_Removed()
    {
        var rp = RolePermission.Assign(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        rp.SoftDelete(Guid.NewGuid());

        var act = () => rp.SoftDelete(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Already removed*");
    }

    [Fact]
    public void UserRole_SoftDelete_Should_Throw_When_Already_Removed()
    {
        var ur = UserRole.Assign(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        ur.SoftDelete(Guid.NewGuid());

        var act = () => ur.SoftDelete(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*UserRole already removed*");
    }

    [Fact]
    public void UserGroup_SoftDelete_Should_Throw_When_Already_Removed()
    {
        var ug = UserGroup.Add(Guid.NewGuid(), Guid.NewGuid(), false, Guid.NewGuid());
        ug.SoftDelete(Guid.NewGuid());

        var act = () => ug.SoftDelete(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Membership already removed*");
    }

    [Fact]
    public void Group_Update_Should_Throw_When_Already_Deleted()
    {
        var group = Group.Create("Team", null, null, 30, Guid.NewGuid());
        group.SoftDelete(Guid.NewGuid());

        var act = () => group.Update("New Name", null, null, 30, Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot modify a deleted group*");
    }

    [Fact]
    public void Group_SoftDelete_Should_Throw_When_Already_Deleted()
    {
        var group = Group.Create("Team", null, null, 30, Guid.NewGuid());
        group.SoftDelete(Guid.NewGuid());

        var act = () => group.SoftDelete(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Group already deleted*");
    }

    [Fact]
    public void UserGroup_SetLead_Should_Throw_When_Membership_Is_Deleted()
    {
        var ug = UserGroup.Add(Guid.NewGuid(), Guid.NewGuid(), false, Guid.NewGuid());
        ug.SoftDelete(Guid.NewGuid());

        var act = () => ug.SetLead(true, Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot modify removed membership*");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Restore (Soft-Delete + Re-Assign) Edge Cases
// ═══════════════════════════════════════════════════════════════════════════

public sealed class RestoreEdgeCaseTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IUserRoleRepository> _urRepo = new();

    [Fact]
    public async Task AssignRoleToUser_Should_Restore_Previously_Deleted_UserRole()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var actorId = Guid.NewGuid();

        var deleted = UserRole.Assign(userId, roleId, Guid.NewGuid());
        deleted.SoftDelete(Guid.NewGuid());

        var user = CreateUser(userId);
        var role = Role.Create("Agent", null, Guid.NewGuid());

        _userRepo.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(roleId, default)).ReturnsAsync(role);
        _urRepo.Setup(r => r.GetAsync(userId, roleId, default)).ReturnsAsync((UserRole?)null);
        _urRepo.Setup(r => r.GetIncludingDeletedAsync(userId, roleId, default)).ReturnsAsync(deleted);

        var handler = new AssignRoleToUserCommandHandler(_userRepo.Object, _roleRepo.Object, _urRepo.Object);
        var result = await handler.Handle(new AssignRoleToUserCommand(userId, roleId, actorId), default);

        result.IsSuccess.Should().BeTrue();
        deleted.IsDeleted.Should().BeFalse();
        deleted.AssignedBy.Should().Be(actorId);
        _urRepo.Verify(r => r.Update(It.IsAny<UserRole>()), Times.Once);
        _urRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Never);
    }

    private static User CreateUser(Guid id)
    {
        var user = (User)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(User));
        typeof(User).GetProperty("Id")!.SetValue(user, id);
        typeof(User).GetProperty("Email")!.SetValue(user, "a@b.com");
        return user;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Maximum Allowed Limits Edge Cases
// ═══════════════════════════════════════════════════════════════════════════

public sealed class BoundaryValueEdgeCaseTests
{
    [Fact]
    public void Role_Create_Should_Accept_80_Character_Name()
    {
        var name = new string('a', 80);
        var act = () => Role.Create(name, null, Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void Role_Create_Should_Trim_Name_With_Whitespace_Padding()
    {
        var name = "  " + new string('a', 78) + "  "; // 82 chars total but 78 content
        var role = Role.Create(name, null, Guid.NewGuid());

        role.Name.Length.Should().Be(78);
    }

    [Fact]
    public void Group_Create_Should_Accept_UnattendedAlertMinutes_Of_1()
    {
        var act = () => Group.Create("Team", null, null, 1, Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void Group_Create_Should_Accept_Large_UnattendedAlertMinutes()
    {
        var act = () => Group.Create("Team", null, null, int.MaxValue, Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public async Task SetRolePermissions_Should_Handle_Single_Permission()
    {
        var roleRepo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        var rpRepo = new Mock<IRolePermissionRepository>();

        var role = Role.Create("Agent", null, Guid.NewGuid());
        var permId = Guid.NewGuid();
        var perm = Permission.Create("ticket", "read", null, Guid.NewGuid());

        roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        permRepo.Setup(r => r.GetByIdAsync(permId, default)).ReturnsAsync(perm);

        var handler = new SetRolePermissionsCommandHandler(roleRepo.Object, permRepo.Object, rpRepo.Object);
        var result = await handler.Handle(
            new SetRolePermissionsCommand(role.Id, new[] { permId }.AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        rpRepo.Verify(r => r.Add(It.IsAny<RolePermission>()), Times.Once);
    }

    [Fact]
    public async Task SetUserRoles_Should_Handle_Empty_RoleIds_As_Clear_All()
    {
        var userRepo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IRoleRepository>();
        var urRepo = new Mock<IUserRoleRepository>();

        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        userRepo.Setup(r => r.GetByIdAsync(userId, default)).ReturnsAsync(user);

        var handler = new SetUserRolesCommandHandler(userRepo.Object, roleRepo.Object, urRepo.Object);
        var result = await handler.Handle(
            new SetUserRolesCommand(userId, Array.Empty<Guid>().AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        urRepo.Verify(r => r.SoftDeleteByUserAsync(userId, It.IsAny<Guid>(), default), Times.Once);
        urRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Never);
    }

    private static User CreateUser(Guid id)
    {
        var user = (User)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(User));
        typeof(User).GetProperty("Id")!.SetValue(user, id);
        typeof(User).GetProperty("Email")!.SetValue(user, "a@b.com");
        return user;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Concurrency / Exception Propagation Edge Cases
// ═══════════════════════════════════════════════════════════════════════════

public sealed class ConcurrencyEdgeCaseTests
{
    [Fact]
    public async Task CreateRole_Should_ReturnFailure_When_SaveChanges_Throws()
    {
        var roleRepo = new Mock<IRoleRepository>();
        roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        roleRepo.Setup(r => r.SaveChangesAsync(default))
                .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

        var handler = new CreateRoleCommandHandler(roleRepo.Object);
        var result = await handler.Handle(new CreateRoleCommand("Agent", null, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Concurrency conflict");
    }

    [Fact]
    public async Task UpdateRole_Should_ReturnFailure_When_SaveChanges_Throws()
    {
        var roleRepo = new Mock<IRoleRepository>();
        var role = Role.Create("Agent", null, Guid.NewGuid());
        roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        roleRepo.Setup(r => r.SaveChangesAsync(default))
                .ThrowsAsync(new InvalidOperationException("DB timeout"));

        var handler = new UpdateRoleCommandHandler(roleRepo.Object);
        var result = await handler.Handle(new UpdateRoleCommand(role.Id, "New Name", null, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("DB timeout");
    }

    [Fact]
    public async Task DeleteRole_Should_ReturnFailure_When_Repository_Throws()
    {
        var roleRepo = new Mock<IRoleRepository>();
        var userRoleRepo = new Mock<IUserRoleRepository>();
        roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
                .ThrowsAsync(new TimeoutException("DB timeout"));

        var handler = new DeleteRoleCommandHandler(roleRepo.Object, userRoleRepo.Object);
        var result = await handler.Handle(new DeleteRoleCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("DB timeout");
    }

    [Fact]
    public async Task GrantPermission_Should_ReturnFailure_When_SaveChanges_Throws()
    {
        var roleRepo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        var rpRepo = new Mock<IRolePermissionRepository>();

        var role = Role.Create("Agent", null, Guid.NewGuid());
        var perm = Permission.Create("ticket", "read", null, Guid.NewGuid());

        roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        permRepo.Setup(r => r.GetByIdAsync(perm.Id, default)).ReturnsAsync(perm);
        rpRepo.Setup(r => r.GetAsync(role.Id, perm.Id, default)).ReturnsAsync((RolePermission?)null);
        rpRepo.Setup(r => r.SaveChangesAsync(default))
              .ThrowsAsync(new Exception("Unique constraint violation"));

        var handler = new GrantPermissionToRoleCommandHandler(roleRepo.Object, permRepo.Object, rpRepo.Object);
        var result = await handler.Handle(
            new GrantPermissionToRoleCommand(role.Id, perm.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Unique constraint violation");
    }
}
