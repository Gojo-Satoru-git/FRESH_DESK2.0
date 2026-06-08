using Adrenalin.Modules.Auth.Application.Handlers;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Adrenalin.UnitTests.RBAC.Handlers.Queries;

// ═══════════════════════════════════════════════════════════════════════════
// GetAllRolesQueryHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetAllRolesQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly GetAllRolesQueryHandler _sut;

    public GetAllRolesQueryHandlerTests()
        => _sut = new GetAllRolesQueryHandler(_roleRepo.Object);

    [Fact]
    public async Task GetAllRoles_Should_Return_All_Roles()
    {
        var roles = new List<Role>
        {
            Role.Create("Agent", null, Guid.NewGuid()),
            Role.Create("Admin", "System admin", Guid.NewGuid())
        };
        _roleRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(roles);

        var result = await _sut.Handle(new GetAllRolesQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllRoles_Should_Return_Empty_List_When_No_Roles()
    {
        _roleRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Role>());

        var result = await _sut.Handle(new GetAllRolesQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRoles_Should_Map_Name_Correctly()
    {
        _roleRepo.Setup(r => r.GetAllAsync(default))
                 .ReturnsAsync(new List<Role> { Role.Create("Support", "Handles", Guid.NewGuid()) });

        var result = await _sut.Handle(new GetAllRolesQuery(), default);

        result.Value!.Single().Name.Should().Be("Support");
    }

    [Fact]
    public async Task GetAllRoles_Should_Map_IsSystemRole_Correctly()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());
        _roleRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Role> { role });

        var result = await _sut.Handle(new GetAllRolesQuery(), default);

        result.Value!.Single().IsSystemRole.Should().BeFalse();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// GetRoleByIdQueryHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetRoleByIdQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly GetRoleByIdQueryHandler _sut;

    public GetRoleByIdQueryHandlerTests()
        => _sut = new GetRoleByIdQueryHandler(_roleRepo.Object);

    [Fact]
    public async Task GetRoleById_Should_Return_Role_When_Found()
    {
        var role = Role.Create("Agent", "desc", Guid.NewGuid());
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);

        var result = await _sut.Handle(new GetRoleByIdQuery(role.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(role.Id);
        result.Value.Name.Should().Be("Agent");
    }

    [Fact]
    public async Task GetRoleById_Should_ReturnFailure_When_Role_Not_Found()
    {
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(new GetRoleByIdQuery(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetRoleById_Should_Map_Description_Correctly()
    {
        var role = Role.Create("Agent", "My description", Guid.NewGuid());
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);

        var result = await _sut.Handle(new GetRoleByIdQuery(role.Id), default);

        result.Value!.Description.Should().Be("My description");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// GetRoleWithPermissionsQueryHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetRoleWithPermissionsQueryHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly GetRoleWithPermissionsQueryHandler _sut;

    public GetRoleWithPermissionsQueryHandlerTests()
        => _sut = new GetRoleWithPermissionsQueryHandler(_roleRepo.Object);

    [Fact]
    public async Task GetRoleWithPermissions_Should_Return_Active_Permissions_Only()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());
        var activePerm = Permission.Create("ticket", "read", null, Guid.NewGuid());
        var deletedPerm = Permission.Create("ticket", "delete", null, Guid.NewGuid());

        var activeRp = RolePermission.Assign(role.Id, activePerm.Id, Guid.NewGuid());
        var deletedRp = RolePermission.Assign(role.Id, deletedPerm.Id, Guid.NewGuid());

        // Set Permission navigation properties via reflection
        SetNav(activeRp, "Permission", activePerm);
        SetNav(deletedRp, "Permission", deletedPerm);
        deletedRp.SoftDelete(Guid.NewGuid());

        SetNav(role, "RolePermissions", new List<RolePermission> { activeRp, deletedRp });

        _roleRepo.Setup(r => r.GetWithPermissionsAsync(role.Id, default)).ReturnsAsync(role);

        var result = await _sut.Handle(new GetRoleWithPermissionsQuery(role.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Permissions.Should().HaveCount(1);
        result.Value.Permissions.Single().Resource.Should().Be("ticket");
        result.Value.Permissions.Single().Action.Should().Be("read");
    }

    [Fact]
    public async Task GetRoleWithPermissions_Should_ReturnFailure_When_Role_Not_Found()
    {
        _roleRepo.Setup(r => r.GetWithPermissionsAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(new GetRoleWithPermissionsQuery(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
    }

    private static void SetNav<T>(object obj, string propertyName, T value)
    {
        var prop = obj.GetType().GetProperty(propertyName);
        prop?.SetValue(obj, value);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// GetAllPermissionsQueryHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetAllPermissionsQueryHandlerTests
{
    private readonly Mock<IPermissionRepository> _permRepo = new();
    private readonly GetAllPermissionsQueryHandler _sut;

    public GetAllPermissionsQueryHandlerTests()
        => _sut = new GetAllPermissionsQueryHandler(_permRepo.Object);

    [Fact]
    public async Task GetAllPermissions_Should_Return_All_Permissions()
    {
        var perms = new List<Permission>
        {
            Permission.Create("ticket", "read", null, Guid.NewGuid()),
            Permission.Create("ticket", "write", null, Guid.NewGuid())
        };
        _permRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(perms);

        var result = await _sut.Handle(new GetAllPermissionsQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllPermissions_Should_Return_Empty_List_When_None_Exist()
    {
        _permRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Permission>());

        var result = await _sut.Handle(new GetAllPermissionsQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllPermissions_Should_Map_Resource_And_Action_Correctly()
    {
        _permRepo.Setup(r => r.GetAllAsync(default))
                 .ReturnsAsync(new List<Permission> { Permission.Create("user", "delete", null, Guid.NewGuid()) });

        var result = await _sut.Handle(new GetAllPermissionsQuery(), default);

        var dto = result.Value!.Single();
        dto.Resource.Should().Be("user");
        dto.Action.Should().Be("delete");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// GetPermissionsByRoleQueryHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetPermissionsByRoleQueryHandlerTests
{
    private readonly Mock<IRolePermissionRepository> _rpRepo = new();
    private readonly GetPermissionsByRoleQueryHandler _sut;

    public GetPermissionsByRoleQueryHandlerTests()
        => _sut = new GetPermissionsByRoleQueryHandler(_rpRepo.Object);

    [Fact]
    public async Task GetPermissionsByRole_Should_Return_Permissions_For_Role()
    {
        var roleId = Guid.NewGuid();
        var perm = Permission.Create("ticket", "read", null, Guid.NewGuid());
        var rp = RolePermission.Assign(roleId, perm.Id, Guid.NewGuid());
        typeof(RolePermission).GetProperty("Permission")!.SetValue(rp, perm);

        _rpRepo.Setup(r => r.GetByRoleWithPermissionsAsync(roleId, default))
               .ReturnsAsync(new List<RolePermission> { rp });

        var result = await _sut.Handle(new GetPermissionsByRoleQuery(roleId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value!.Single().Resource.Should().Be("ticket");
    }

    [Fact]
    public async Task GetPermissionsByRole_Should_Return_Empty_List_When_No_Permissions()
    {
        _rpRepo.Setup(r => r.GetByRoleWithPermissionsAsync(It.IsAny<Guid>(), default))
               .ReturnsAsync(new List<RolePermission>());

        var result = await _sut.Handle(new GetPermissionsByRoleQuery(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// GetUsersQueryHandler (pagination)
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly GetUsersQueryHandler _sut;

    public GetUsersQueryHandlerTests()
        => _sut = new GetUsersQueryHandler(_userRepo.Object);

    private static User CreateUser(string email = "a@b.com")
    {
        var user = (User)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(User));
        typeof(User).GetProperty("Id")!.SetValue(user, Guid.NewGuid());
        typeof(User).GetProperty("Email")!.SetValue(user, email);
        typeof(User).GetProperty("IsActive")!.SetValue(user, true);
        return user;
    }

    [Fact]
    public async Task GetUsers_Should_Return_Paged_Results()
    {
        var users = new List<User> { CreateUser("a@b.com"), CreateUser("c@d.com") };
        _userRepo.Setup(r => r.GetPagedAsync(null, null, 1, 20, default)).ReturnsAsync((users, 2));

        var result = await _sut.Handle(new GetUsersQuery(null, null, 1, 20), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetUsers_Should_Return_Empty_Page_When_No_Users()
    {
        _userRepo.Setup(r => r.GetPagedAsync(It.IsAny<string?>(), It.IsAny<bool?>(), 1, 20, default))
                 .ReturnsAsync((new List<User>(), 0));

        var result = await _sut.Handle(new GetUsersQuery(null, null, 1, 20), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetUsers_Should_Pass_Filter_Parameters_To_Repository()
    {
        _userRepo.Setup(r => r.GetPagedAsync("john", true, 2, 10, default))
                 .ReturnsAsync((new List<User>(), 0));

        await _sut.Handle(new GetUsersQuery("john", true, 2, 10), default);

        _userRepo.Verify(r => r.GetPagedAsync("john", true, 2, 10, default), Times.Once);
    }

    [Fact]
    public async Task GetUsers_Should_Map_Email_Correctly()
    {
        var user = CreateUser("test@example.com");
        _userRepo.Setup(r => r.GetPagedAsync(null, null, 1, 20, default))
                 .ReturnsAsync((new List<User> { user }, 1));

        var result = await _sut.Handle(new GetUsersQuery(null, null, 1, 20), default);

        result.Value!.Items.Single().Email.Should().Be("test@example.com");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// GetUserEffectivePermissionsQueryHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetUserEffectivePermissionsQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly GetUserEffectivePermissionsQueryHandler _sut;

    public GetUserEffectivePermissionsQueryHandlerTests()
        => _sut = new GetUserEffectivePermissionsQueryHandler(_userRepo.Object);

    [Fact]
    public async Task GetUserEffectivePermissions_Should_Return_Permission_Keys()
    {
        var userId = Guid.NewGuid();
        var perms = new List<string> { "ticket:read", "ticket:write", "user:read" };
        _userRepo.Setup(r => r.GetEffectivePermissionsAsync(userId, default)).ReturnsAsync(perms);

        var result = await _sut.Handle(new GetUserEffectivePermissionsQuery(userId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(perms);
    }

    [Fact]
    public async Task GetUserEffectivePermissions_Should_Return_Empty_When_User_Has_No_Roles()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetEffectivePermissionsAsync(userId, default))
                 .ReturnsAsync(new List<string>());

        var result = await _sut.Handle(new GetUserEffectivePermissionsQuery(userId), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// GetAllGroupsQueryHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetAllGroupsQueryHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly GetAllGroupsQueryHandler _sut;

    public GetAllGroupsQueryHandlerTests()
        => _sut = new GetAllGroupsQueryHandler(_groupRepo.Object);

    [Fact]
    public async Task GetAllGroups_Should_Return_All_Groups()
    {
        var groups = new List<Group>
        {
            Group.Create("Team A", "EU", "T1", 30, Guid.NewGuid()),
            Group.Create("Team B", null, null, 60, Guid.NewGuid())
        };
        _groupRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(groups);

        var result = await _sut.Handle(new GetAllGroupsQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllGroups_Should_Return_Empty_List_When_None_Exist()
    {
        _groupRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(new List<Group>());

        var result = await _sut.Handle(new GetAllGroupsQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllGroups_Should_Map_Name_Correctly()
    {
        _groupRepo.Setup(r => r.GetAllAsync(default))
                  .ReturnsAsync(new List<Group> { Group.Create("Support", null, null, 15, Guid.NewGuid()) });

        var result = await _sut.Handle(new GetAllGroupsQuery(), default);

        result.Value!.Single().Name.Should().Be("Support");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// GetGroupByIdQueryHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class GetGroupByIdQueryHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly GetGroupByIdQueryHandler _sut;

    public GetGroupByIdQueryHandlerTests()
        => _sut = new GetGroupByIdQueryHandler(_groupRepo.Object);

    [Fact]
    public async Task GetGroupById_Should_Return_Group_When_Found()
    {
        var group = Group.Create("Support", "EU", "T1", 30, Guid.NewGuid());
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);

        var result = await _sut.Handle(new GetGroupByIdQuery(group.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(group.Id);
        result.Value.Name.Should().Be("Support");
    }

    [Fact]
    public async Task GetGroupById_Should_ReturnFailure_When_Not_Found()
    {
        _groupRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Group?)null);

        var result = await _sut.Handle(new GetGroupByIdQuery(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}
