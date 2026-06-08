using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Handlers;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Adrenalin.UnitTests.RBAC.Handlers.Commands;

// ═══════════════════════════════════════════════════════════════════════════
// GrantPermissionToRoleCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GrantPermissionToRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPermissionRepository> _permRepo = new();
    private readonly Mock<IRolePermissionRepository> _rpRepo = new();
    private readonly GrantPermissionToRoleCommandHandler _sut;

    public GrantPermissionToRoleCommandHandlerTests()
        => _sut = new GrantPermissionToRoleCommandHandler(_roleRepo.Object, _permRepo.Object, _rpRepo.Object);

    private static Role MakeRole() => Role.Create("Agent", null, Guid.NewGuid());
    private static Permission MakePerm() => Permission.Create("ticket", "read", null, Guid.NewGuid());

    [Fact]
    public async Task GrantPermissionToRoleCommandHandler_Should_Assign_Permission_When_Valid()
    {
        var role = MakeRole();
        var perm = MakePerm();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _permRepo.Setup(r => r.GetByIdAsync(perm.Id, default)).ReturnsAsync(perm);
        _rpRepo.Setup(r => r.GetAsync(role.Id, perm.Id, default)).ReturnsAsync((RolePermission?)null);

        var result = await _sut.Handle(new GrantPermissionToRoleCommand(role.Id, perm.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GrantPermissionToRoleCommandHandler_Should_Add_RolePermission()
    {
        var role = MakeRole();
        var perm = MakePerm();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _permRepo.Setup(r => r.GetByIdAsync(perm.Id, default)).ReturnsAsync(perm);
        _rpRepo.Setup(r => r.GetAsync(role.Id, perm.Id, default)).ReturnsAsync((RolePermission?)null);

        await _sut.Handle(new GrantPermissionToRoleCommand(role.Id, perm.Id, Guid.NewGuid()), default);

        _rpRepo.Verify(r => r.Add(It.IsAny<RolePermission>()), Times.Once);
    }

    [Fact]
    public async Task GrantPermissionToRoleCommandHandler_Should_Be_Idempotent_When_Permission_Already_Granted()
    {
        var role = MakeRole();
        var perm = MakePerm();
        var existing = RolePermission.Assign(role.Id, perm.Id, Guid.NewGuid());
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _permRepo.Setup(r => r.GetByIdAsync(perm.Id, default)).ReturnsAsync(perm);
        _rpRepo.Setup(r => r.GetAsync(role.Id, perm.Id, default)).ReturnsAsync(existing);

        var result = await _sut.Handle(new GrantPermissionToRoleCommand(role.Id, perm.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _rpRepo.Verify(r => r.Add(It.IsAny<RolePermission>()), Times.Never);
    }

    [Fact]
    public async Task GrantPermissionToRoleCommandHandler_Should_ReturnFailure_When_Role_NotFound()
    {
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(
            new GrantPermissionToRoleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GrantPermissionToRoleCommandHandler_Should_ReturnFailure_When_Permission_NotFound()
    {
        var role = MakeRole();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _permRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Permission?)null);

        var result = await _sut.Handle(
            new GrantPermissionToRoleCommand(role.Id, Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// RevokePermissionFromRoleCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class RevokePermissionFromRoleCommandHandlerTests
{
    private readonly Mock<IRolePermissionRepository> _rpRepo = new();
    private readonly RevokePermissionFromRoleCommandHandler _sut;

    public RevokePermissionFromRoleCommandHandlerTests()
        => _sut = new RevokePermissionFromRoleCommandHandler(_rpRepo.Object);

    [Fact]
    public async Task RevokePermissionFromRoleCommandHandler_Should_Revoke_Permission()
    {
        var rp = RolePermission.Assign(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _rpRepo.Setup(r => r.GetAsync(rp.RoleId, rp.PermissionId, default)).ReturnsAsync(rp);

        var result = await _sut.Handle(
            new RevokePermissionFromRoleCommand(rp.RoleId, rp.PermissionId, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        rp.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task RevokePermissionFromRoleCommandHandler_Should_Call_Update()
    {
        var rp = RolePermission.Assign(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        _rpRepo.Setup(r => r.GetAsync(rp.RoleId, rp.PermissionId, default)).ReturnsAsync(rp);

        await _sut.Handle(new RevokePermissionFromRoleCommand(rp.RoleId, rp.PermissionId, Guid.NewGuid()), default);

        _rpRepo.Verify(r => r.Update(It.IsAny<RolePermission>()), Times.Once);
    }

    [Fact]
    public async Task RevokePermissionFromRoleCommandHandler_Should_ReturnFailure_When_Not_Assigned()
    {
        _rpRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
               .ReturnsAsync((RolePermission?)null);

        var result = await _sut.Handle(
            new RevokePermissionFromRoleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not assigned");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// SetRolePermissionsCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class SetRolePermissionsCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPermissionRepository> _permRepo = new();
    private readonly Mock<IRolePermissionRepository> _rpRepo = new();
    private readonly SetRolePermissionsCommandHandler _sut;

    public SetRolePermissionsCommandHandlerTests()
        => _sut = new SetRolePermissionsCommandHandler(_roleRepo.Object, _permRepo.Object, _rpRepo.Object);

    [Fact]
    public async Task SetRolePermissionsCommandHandler_Should_Delete_Old_And_Add_New_Permissions()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());
        var permId1 = Guid.NewGuid();
        var permId2 = Guid.NewGuid();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _permRepo.Setup(r => r.GetByIdAsync(permId1, default)).ReturnsAsync(Permission.Create("t", "r", null, Guid.NewGuid()));
        _permRepo.Setup(r => r.GetByIdAsync(permId2, default)).ReturnsAsync(Permission.Create("t", "w", null, Guid.NewGuid()));

        var result = await _sut.Handle(
            new SetRolePermissionsCommand(role.Id, new[] { permId1, permId2 }.AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _rpRepo.Verify(r => r.SoftDeleteByRoleAsync(role.Id, It.IsAny<Guid>(), default), Times.Once);
        _rpRepo.Verify(r => r.Add(It.IsAny<RolePermission>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SetRolePermissionsCommandHandler_Should_ReturnFailure_When_Role_NotFound()
    {
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(
            new SetRolePermissionsCommand(Guid.NewGuid(), Array.Empty<Guid>().AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SetRolePermissionsCommandHandler_Should_ReturnFailure_When_A_PermissionId_NotFound()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());
        var badPermId = Guid.NewGuid();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _permRepo.Setup(r => r.GetByIdAsync(badPermId, default)).ReturnsAsync((Permission?)null);

        var result = await _sut.Handle(
            new SetRolePermissionsCommand(role.Id, new[] { badPermId }.AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(badPermId.ToString());
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// AssignRoleToUserCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class AssignRoleToUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IUserRoleRepository> _urRepo = new();
    private readonly AssignRoleToUserCommandHandler _sut;

    public AssignRoleToUserCommandHandlerTests()
        => _sut = new AssignRoleToUserCommandHandler(_userRepo.Object, _roleRepo.Object, _urRepo.Object);

    private User MakeUser() => CreateUser();
    private Role MakeRole() => Role.Create("Agent", null, Guid.NewGuid());

    private static User CreateUser()
    {
        // Use reflection since User has private constructor + factory
        var user = (User)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(User));
        typeof(User).GetProperty("Id")!.SetValue(user, Guid.NewGuid());
        typeof(User).GetProperty("Email")!.SetValue(user, "test@example.com");
        return user;
    }

    [Fact]
    public async Task AssignRoleToUserCommandHandler_Should_Assign_Role_When_Not_Already_Assigned()
    {
        var user = MakeUser();
        var role = MakeRole();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _urRepo.Setup(r => r.GetAsync(user.Id, role.Id, default)).ReturnsAsync((UserRole?)null);
        _urRepo.Setup(r => r.GetIncludingDeletedAsync(user.Id, role.Id, default)).ReturnsAsync((UserRole?)null);

        var result = await _sut.Handle(new AssignRoleToUserCommand(user.Id, role.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _urRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToUserCommandHandler_Should_Be_Idempotent_When_Role_Already_Active()
    {
        var user = MakeUser();
        var role = MakeRole();
        var existing = UserRole.Assign(user.Id, role.Id, Guid.NewGuid());
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _urRepo.Setup(r => r.GetAsync(user.Id, role.Id, default)).ReturnsAsync(existing);

        var result = await _sut.Handle(new AssignRoleToUserCommand(user.Id, role.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _urRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserCommandHandler_Should_Restore_Deleted_UserRole()
    {
        var user = MakeUser();
        var role = MakeRole();
        var deleted = UserRole.Assign(user.Id, role.Id, Guid.NewGuid());
        deleted.SoftDelete(Guid.NewGuid());

        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _urRepo.Setup(r => r.GetAsync(user.Id, role.Id, default)).ReturnsAsync((UserRole?)null);
        _urRepo.Setup(r => r.GetIncludingDeletedAsync(user.Id, role.Id, default)).ReturnsAsync(deleted);

        await _sut.Handle(new AssignRoleToUserCommand(user.Id, role.Id, Guid.NewGuid()), default);

        deleted.IsDeleted.Should().BeFalse();
        _urRepo.Verify(r => r.Update(It.IsAny<UserRole>()), Times.Once);
        _urRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserCommandHandler_Should_ReturnFailure_When_User_NotFound()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((User?)null);

        var result = await _sut.Handle(
            new AssignRoleToUserCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task AssignRoleToUserCommandHandler_Should_ReturnFailure_When_Role_NotFound()
    {
        var user = MakeUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(
            new AssignRoleToUserCommand(user.Id, Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// RemoveRoleFromUserCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class RemoveRoleFromUserCommandHandlerTests
{
    private readonly Mock<IUserRoleRepository> _urRepo = new();
    private readonly RemoveRoleFromUserCommandHandler _sut;

    public RemoveRoleFromUserCommandHandlerTests()
        => _sut = new RemoveRoleFromUserCommandHandler(_urRepo.Object);

    [Fact]
    public async Task RemoveRoleFromUserCommandHandler_Should_SoftDelete_UserRole()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var ur = UserRole.Assign(userId, roleId, Guid.NewGuid());
        _urRepo.Setup(r => r.GetAsync(userId, roleId, default)).ReturnsAsync(ur);

        var result = await _sut.Handle(new RemoveRoleFromUserCommand(userId, roleId, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        ur.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveRoleFromUserCommandHandler_Should_ReturnFailure_When_UserRole_NotFound()
    {
        _urRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
               .ReturnsAsync((UserRole?)null);

        var result = await _sut.Handle(
            new RemoveRoleFromUserCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not assigned");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// SetUserRolesCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class SetUserRolesCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IUserRoleRepository> _urRepo = new();
    private readonly SetUserRolesCommandHandler _sut;

    public SetUserRolesCommandHandlerTests()
        => _sut = new SetUserRolesCommandHandler(_userRepo.Object, _roleRepo.Object, _urRepo.Object);

    private static User CreateUser()
    {
        var user = (User)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(User));
        typeof(User).GetProperty("Id")!.SetValue(user, Guid.NewGuid());
        return user;
    }

    [Fact]
    public async Task SetUserRolesCommandHandler_Should_Delete_Old_And_Add_New_Roles()
    {
        var user = CreateUser();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(roleId1, default)).ReturnsAsync(Role.Create("R1", null, Guid.NewGuid()));
        _roleRepo.Setup(r => r.GetByIdAsync(roleId2, default)).ReturnsAsync(Role.Create("R2", null, Guid.NewGuid()));

        var result = await _sut.Handle(
            new SetUserRolesCommand(user.Id, new[] { roleId1, roleId2 }.AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _urRepo.Verify(r => r.SoftDeleteByUserAsync(user.Id, It.IsAny<Guid>(), default), Times.Once);
        _urRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SetUserRolesCommandHandler_Should_ReturnFailure_When_User_NotFound()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((User?)null);

        var result = await _sut.Handle(
            new SetUserRolesCommand(Guid.NewGuid(), Array.Empty<Guid>().AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SetUserRolesCommandHandler_Should_ReturnFailure_When_A_RoleId_NotFound()
    {
        var user = CreateUser();
        var badRoleId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _roleRepo.Setup(r => r.GetByIdAsync(badRoleId, default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(
            new SetUserRolesCommand(user.Id, new[] { badRoleId }.AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain(badRoleId.ToString());
    }

    [Fact]
    public async Task SetUserRolesCommandHandler_Should_Clear_All_Roles_When_RoleIds_Is_Empty()
    {
        var user = CreateUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);

        var result = await _sut.Handle(
            new SetUserRolesCommand(user.Id, Array.Empty<Guid>().AsReadOnly(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _urRepo.Verify(r => r.SoftDeleteByUserAsync(user.Id, It.IsAny<Guid>(), default), Times.Once);
        _urRepo.Verify(r => r.Add(It.IsAny<UserRole>()), Times.Never);
    }
}
