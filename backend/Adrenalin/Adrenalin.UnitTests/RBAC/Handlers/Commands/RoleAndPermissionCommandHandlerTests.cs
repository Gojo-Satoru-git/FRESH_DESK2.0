using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Handlers;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Adrenalin.UnitTests.RBAC.Handlers.Commands;

// ═══════════════════════════════════════════════════════════════════════════
// CreateRoleCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly CreateRoleCommandHandler _sut;

    public CreateRoleCommandHandlerTests()
        => _sut = new CreateRoleCommandHandler(_roleRepo.Object);

    private static CreateRoleCommand ValidCommand(string name = "Agent") =>
        new(name, "desc", Guid.NewGuid());

    [Fact]
    public async Task CreateRoleCommandHandler_Should_Create_Role_When_Valid()
    {
        _roleRepo.Setup(r => r.ExistsByNameAsync("Agent", default)).ReturnsAsync(false);

        var result = await _sut.Handle(ValidCommand("Agent"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateRoleCommandHandler_Should_Call_Add_On_Repository()
    {
        _roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);

        await _sut.Handle(ValidCommand(), default);

        _roleRepo.Verify(r => r.Add(It.IsAny<Role>()), Times.Once);
    }

    [Fact]
    public async Task CreateRoleCommandHandler_Should_Call_SaveChanges()
    {
        _roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);

        await _sut.Handle(ValidCommand(), default);

        _roleRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateRoleCommandHandler_Should_ReturnFailure_When_RoleAlreadyExists()
    {
        _roleRepo.Setup(r => r.ExistsByNameAsync("Agent", default)).ReturnsAsync(true);

        var result = await _sut.Handle(ValidCommand("Agent"), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Agent");
    }

    [Fact]
    public async Task CreateRoleCommandHandler_Should_Not_Call_Add_When_Role_Already_Exists()
    {
        _roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(true);

        await _sut.Handle(ValidCommand(), default);

        _roleRepo.Verify(r => r.Add(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task CreateRoleCommandHandler_Should_ReturnFailure_When_Repository_Throws()
    {
        _roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default))
                 .ThrowsAsync(new InvalidOperationException("DB down"));

        var result = await _sut.Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("DB down");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// UpdateRoleCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class UpdateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly UpdateRoleCommandHandler _sut;

    public UpdateRoleCommandHandlerTests()
        => _sut = new UpdateRoleCommandHandler(_roleRepo.Object);

    private Role ExistingRole(string name = "Old Name")
        => Role.Create(name, null, Guid.NewGuid());

    private static UpdateRoleCommand ValidCommand(Guid roleId, string newName = "New Name") =>
        new(roleId, newName, null, Guid.NewGuid());

    [Fact]
    public async Task UpdateRoleCommandHandler_Should_Update_Role_When_Valid()
    {
        var role = ExistingRole("Old Name");
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _roleRepo.Setup(r => r.ExistsByNameAsync("New Name", default)).ReturnsAsync(false);

        var result = await _sut.Handle(ValidCommand(role.Id, "New Name"), default);

        result.IsSuccess.Should().BeTrue();
        role.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateRoleCommandHandler_Should_Call_Update_On_Repository()
    {
        var role = ExistingRole();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);

        await _sut.Handle(ValidCommand(role.Id), default);

        _roleRepo.Verify(r => r.Update(It.IsAny<Role>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleCommandHandler_Should_Call_SaveChanges()
    {
        var role = ExistingRole();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);

        await _sut.Handle(ValidCommand(role.Id), default);

        _roleRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleCommandHandler_Should_ReturnFailure_When_Role_NotFound()
    {
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(ValidCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateRoleCommandHandler_Should_ReturnFailure_When_New_Name_Already_Exists()
    {
        var role = ExistingRole("Old Name");
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _roleRepo.Setup(r => r.ExistsByNameAsync("Taken", default)).ReturnsAsync(true);

        var result = await _sut.Handle(ValidCommand(role.Id, "Taken"), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Taken");
    }

    [Fact]
    public async Task UpdateRoleCommandHandler_Should_Succeed_When_Name_Is_Same_Case_Insensitive()
    {
        // Same name (case-insensitive match) should not trigger the duplicate check
        var role = ExistingRole("Agent");
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _roleRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(true);

        var result = await _sut.Handle(new UpdateRoleCommand(role.Id, "AGENT", null, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DeleteRoleCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class DeleteRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IUserRoleRepository> _userRoleRepo = new();
    private readonly DeleteRoleCommandHandler _sut;

    public DeleteRoleCommandHandlerTests()
        => _sut = new DeleteRoleCommandHandler(_roleRepo.Object, _userRoleRepo.Object);

    private Role ExistingRole() => Role.Create("Agent", null, Guid.NewGuid());

    [Fact]
    public async Task DeleteRoleCommandHandler_Should_SoftDelete_Role()
    {
        var role = ExistingRole();
        var actorId = Guid.NewGuid();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);

        var result = await _sut.Handle(new DeleteRoleCommand(role.Id, actorId), default);

        result.IsSuccess.Should().BeTrue();
        role.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteRoleCommandHandler_Should_SoftDelete_Associated_UserRoles()
    {
        var role = ExistingRole();
        var actorId = Guid.NewGuid();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);

        await _sut.Handle(new DeleteRoleCommand(role.Id, actorId), default);

        _userRoleRepo.Verify(r => r.SoftDeleteByRoleAsync(role.Id, actorId, default), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleCommandHandler_Should_ReturnFailure_When_Role_NotFound()
    {
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(new DeleteRoleCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteRoleCommandHandler_Should_ReturnFailure_When_Role_Is_SystemRole()
    {
        var role = ExistingRole();
        typeof(Role).GetProperty(nameof(Role.IsSystemRole))!.SetValue(role, true);
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);

        var result = await _sut.Handle(new DeleteRoleCommand(role.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("System roles cannot be deleted");
    }

    [Fact]
    public async Task DeleteRoleCommandHandler_Should_Call_SaveChanges()
    {
        var role = ExistingRole();
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);

        await _sut.Handle(new DeleteRoleCommand(role.Id, Guid.NewGuid()), default);

        _roleRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// CreatePermissionCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class CreatePermissionCommandHandlerTests
{
    private readonly Mock<IPermissionRepository> _permRepo = new();
    private readonly CreatePermissionCommandHandler _sut;

    public CreatePermissionCommandHandlerTests()
        => _sut = new CreatePermissionCommandHandler(_permRepo.Object);

    private static CreatePermissionCommand ValidCommand() =>
        new("ticket", "read", null, Guid.NewGuid());

    [Fact]
    public async Task CreatePermissionCommandHandler_Should_Create_Permission()
    {
        _permRepo.Setup(r => r.ExistsAsync("ticket", "read", default)).ReturnsAsync(false);

        var result = await _sut.Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreatePermissionCommandHandler_Should_Call_Add_On_Repository()
    {
        _permRepo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                 .ReturnsAsync(false);

        await _sut.Handle(ValidCommand(), default);

        _permRepo.Verify(r => r.Add(It.IsAny<Permission>()), Times.Once);
    }

    [Fact]
    public async Task CreatePermissionCommandHandler_Should_ReturnFailure_When_Permission_Already_Exists()
    {
        _permRepo.Setup(r => r.ExistsAsync("ticket", "read", default)).ReturnsAsync(true);

        var result = await _sut.Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("ticket:read");
    }

    [Fact]
    public async Task CreatePermissionCommandHandler_Should_Call_SaveChanges_On_Success()
    {
        _permRepo.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                 .ReturnsAsync(false);

        await _sut.Handle(ValidCommand(), default);

        _permRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DeletePermissionCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class DeletePermissionCommandHandlerTests
{
    private readonly Mock<IPermissionRepository> _permRepo = new();
    private readonly Mock<IRolePermissionRepository> _rpRepo = new();
    private readonly DeletePermissionCommandHandler _sut;

    public DeletePermissionCommandHandlerTests()
        => _sut = new DeletePermissionCommandHandler(_permRepo.Object, _rpRepo.Object);

    [Fact]
    public async Task DeletePermissionCommandHandler_Should_SoftDelete_Permission()
    {
        var perm = Permission.Create("ticket", "read", null, Guid.NewGuid());
        _permRepo.Setup(r => r.GetByIdAsync(perm.Id, default)).ReturnsAsync(perm);

        var result = await _sut.Handle(new DeletePermissionCommand(perm.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        perm.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeletePermissionCommandHandler_Should_SoftDelete_Associated_RolePermissions()
    {
        var perm = Permission.Create("ticket", "read", null, Guid.NewGuid());
        var actorId = Guid.NewGuid();
        _permRepo.Setup(r => r.GetByIdAsync(perm.Id, default)).ReturnsAsync(perm);

        await _sut.Handle(new DeletePermissionCommand(perm.Id, actorId), default);

        _rpRepo.Verify(r => r.SoftDeleteByPermissionAsync(perm.Id, actorId, default), Times.Once);
    }

    [Fact]
    public async Task DeletePermissionCommandHandler_Should_ReturnFailure_When_Permission_NotFound()
    {
        _permRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Permission?)null);

        var result = await _sut.Handle(new DeletePermissionCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}
