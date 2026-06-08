using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Moq;

namespace Adrenalin.UnitTests.RBAC.TestHelpers;

// ─────────────────────────────────────────────────────────────────────────────
// Domain Object Builders
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Fluent builder for Role domain entities.
/// </summary>
public sealed class RoleBuilder
{
    private string _name = "Default Role";
    private string? _description;
    private Guid _createdBy = Guid.NewGuid();
    private bool _isSystemRole;
    private bool _isDeleted;

    public RoleBuilder WithName(string name) { _name = name; return this; }
    public RoleBuilder WithDescription(string? description) { _description = description; return this; }
    public RoleBuilder WithCreatedBy(Guid createdBy) { _createdBy = createdBy; return this; }

    public RoleBuilder AsSystemRole()
    {
        _isSystemRole = true;
        return this;
    }

    public RoleBuilder AsDeleted()
    {
        _isDeleted = true;
        return this;
    }

    public Role Build()
    {
        var role = Role.Create(_name, _description, _createdBy);

        if (_isSystemRole)
            typeof(Role).GetProperty(nameof(Role.IsSystemRole))!.SetValue(role, true);

        if (_isDeleted)
            role.SoftDelete(_createdBy);

        return role;
    }
}

/// <summary>
/// Fluent builder for Permission domain entities.
/// </summary>
public sealed class PermissionBuilder
{
    private string _resource = "ticket";
    private string _action = "read";
    private string? _description;
    private Guid _createdBy = Guid.NewGuid();
    private bool _isDeleted;

    public PermissionBuilder WithResource(string resource) { _resource = resource; return this; }
    public PermissionBuilder WithAction(string action) { _action = action; return this; }
    public PermissionBuilder WithDescription(string? description) { _description = description; return this; }
    public PermissionBuilder WithCreatedBy(Guid createdBy) { _createdBy = createdBy; return this; }

    public PermissionBuilder AsDeleted()
    {
        _isDeleted = true;
        return this;
    }

    public Permission Build()
    {
        var perm = Permission.Create(_resource, _action, _description, _createdBy);

        if (_isDeleted)
            perm.SoftDelete(_createdBy);

        return perm;
    }
}

/// <summary>
/// Fluent builder for Group domain entities.
/// </summary>
public sealed class GroupBuilder
{
    private string _name = "Default Group";
    private string? _regionCode = "EU";
    private string? _tierCode = "T1";
    private int _alertMinutes = 30;
    private Guid _createdBy = Guid.NewGuid();
    private bool _isDeleted;

    public GroupBuilder WithName(string name) { _name = name; return this; }
    public GroupBuilder WithRegionCode(string? regionCode) { _regionCode = regionCode; return this; }
    public GroupBuilder WithTierCode(string? tierCode) { _tierCode = tierCode; return this; }
    public GroupBuilder WithAlertMinutes(int minutes) { _alertMinutes = minutes; return this; }
    public GroupBuilder WithCreatedBy(Guid createdBy) { _createdBy = createdBy; return this; }

    public GroupBuilder AsDeleted()
    {
        _isDeleted = true;
        return this;
    }

    public Group Build()
    {
        var group = Group.Create(_name, _regionCode, _tierCode, _alertMinutes, _createdBy);

        if (_isDeleted)
            group.SoftDelete(_createdBy);

        return group;
    }
}

/// <summary>
/// Fluent builder for User domain entities (via reflection since User has private ctor).
/// </summary>
public sealed class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _email = "default@example.com";
    private string? _firstName = "John";
    private string? _lastName = "Doe";
    private bool _isActive = true;

    public UserBuilder WithId(Guid id) { _id = id; return this; }
    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder WithFirstName(string? firstName) { _firstName = firstName; return this; }
    public UserBuilder WithLastName(string? lastName) { _lastName = lastName; return this; }
    public UserBuilder AsInactive() { _isActive = false; return this; }

    public User Build()
    {
        var user = (User)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(User));
        var t = typeof(User);
        t.GetProperty("Id")!.SetValue(user, _id);
        t.GetProperty("Email")!.SetValue(user, _email);
        t.GetProperty("FirstName")!.SetValue(user, _firstName);
        t.GetProperty("LastName")!.SetValue(user, _lastName);
        t.GetProperty("IsActive")!.SetValue(user, _isActive);
        return user;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Repository Mock Factories
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Creates pre-configured repository mocks for use in handler tests.
/// </summary>
public static class MockRepositoryFactory
{
    public static Mock<IRoleRepository> RoleRepository(Role? roleToReturn = null, bool nameExists = false)
    {
        var mock = new Mock<IRoleRepository>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(roleToReturn);
        mock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(
            roleToReturn is null ? new List<Role>() : new List<Role> { roleToReturn });
        mock.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(nameExists);
        return mock;
    }

    public static Mock<IPermissionRepository> PermissionRepository(
        Permission? permToReturn = null, bool exists = false)
    {
        var mock = new Mock<IPermissionRepository>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(permToReturn);
        mock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(
            permToReturn is null ? new List<Permission>() : new List<Permission> { permToReturn });
        mock.Setup(r => r.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), default)).ReturnsAsync(exists);
        return mock;
    }

    public static Mock<IRolePermissionRepository> RolePermissionRepository(
        RolePermission? rpToReturn = null,
        IReadOnlyList<RolePermission>? listToReturn = null)
    {
        var mock = new Mock<IRolePermissionRepository>();
        mock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default)).ReturnsAsync(rpToReturn);
        mock.Setup(r => r.GetByRoleWithPermissionsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(listToReturn ?? new List<RolePermission>());
        return mock;
    }

    public static Mock<IUserRoleRepository> UserRoleRepository(
        UserRole? activeRole = null,
        UserRole? deletedRole = null)
    {
        var mock = new Mock<IUserRoleRepository>();
        mock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default)).ReturnsAsync(activeRole);
        mock.Setup(r => r.GetIncludingDeletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default)).ReturnsAsync(deletedRole);
        return mock;
    }

    public static Mock<IGroupRepository> GroupRepository(Group? groupToReturn = null, bool nameExists = false)
    {
        var mock = new Mock<IGroupRepository>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(groupToReturn);
        mock.Setup(r => r.GetWithMembersAsync(It.IsAny<Guid>(), default)).ReturnsAsync(groupToReturn);
        mock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(
            groupToReturn is null ? new List<Group>() : new List<Group> { groupToReturn });
        mock.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(nameExists);
        return mock;
    }

    public static Mock<IUserGroupRepository> UserGroupRepository(
        UserGroup? ugToReturn = null,
        UserGroup? deletedUgToReturn = null)
    {
        var mock = new Mock<IUserGroupRepository>();
        mock.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default)).ReturnsAsync(ugToReturn);
        mock.Setup(r => r.GetIncludingDeletedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default)).ReturnsAsync(deletedUgToReturn);
        mock.Setup(r => r.GetByUserAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(ugToReturn is null ? new List<UserGroup>() : new List<UserGroup> { ugToReturn });
        return mock;
    }

    public static Mock<IUserRepository> UserRepository(User? userToReturn = null)
    {
        var mock = new Mock<IUserRepository>();
        mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync(userToReturn);
        mock.Setup(r => r.GetWithRolesAsync(It.IsAny<Guid>(), default)).ReturnsAsync(userToReturn);
        mock.Setup(r => r.GetEffectivePermissionsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new List<string>());
        mock.Setup(r => r.GetPagedAsync(It.IsAny<string?>(), It.IsAny<bool?>(),
                It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync((new List<User>(), 0));
        return mock;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Test Data Factories
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Static factory for frequently used test data combinations.
/// </summary>
public static class TestData
{
    public static (Role role, Permission permission, RolePermission rp) RoleWithPermission()
    {
        var role = new RoleBuilder().WithName("Test Role").Build();
        var permission = new PermissionBuilder().WithResource("ticket").WithAction("read").Build();
        var rp = RolePermission.Assign(role.Id, permission.Id, Guid.NewGuid());
        return (role, permission, rp);
    }

    public static (User user, Role role, UserRole ur) UserWithRole()
    {
        var user = new UserBuilder().WithEmail("user@example.com").Build();
        var role = new RoleBuilder().WithName("Agent").Build();
        var ur = UserRole.Assign(user.Id, role.Id, Guid.NewGuid());
        return (user, role, ur);
    }

    public static (User user, Group group, UserGroup ug) UserWithGroup(bool isLead = false)
    {
        var user = new UserBuilder().WithEmail("member@example.com").Build();
        var group = new GroupBuilder().WithName("Support Team").Build();
        var ug = UserGroup.Add(user.Id, group.Id, isLead, Guid.NewGuid());
        return (user, group, ug);
    }

    public static IReadOnlyList<Permission> MultiplePermissions(int count = 3)
    {
        return Enumerable.Range(0, count)
            .Select(i => new PermissionBuilder()
                .WithResource("ticket")
                .WithAction($"action_{i}")
                .Build())
            .ToList()
            .AsReadOnly();
    }

    public static IReadOnlyList<Role> MultipleRoles(int count = 3)
    {
        return Enumerable.Range(0, count)
            .Select(i => new RoleBuilder().WithName($"Role_{i}").Build())
            .ToList()
            .AsReadOnly();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Reflection Helpers
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Helpers for setting navigation properties and private fields via reflection
/// (required because EF Core nav properties have private setters).
/// </summary>
public static class ReflectionHelper
{
    public static void SetProperty<T>(object obj, string propertyName, T value)
    {
        var prop = obj.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        prop?.SetValue(obj, value);
    }

    public static T? GetProperty<T>(object obj, string propertyName)
    {
        var prop = obj.GetType().GetProperty(propertyName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (T?)prop?.GetValue(obj);
    }

    public static void SetNavigationCollection<TEntity>(object owner, string propertyName, ICollection<TEntity> items)
    {
        SetProperty(owner, propertyName, items);
    }

    public static void AttachPermissionToRolePermission(RolePermission rp, Permission perm)
        => SetProperty(rp, "Permission", perm);

    public static void AttachRoleToRolePermission(RolePermission rp, Role role)
        => SetProperty(rp, "Role", role);

    public static void AttachRolePermissionsToRole(Role role, ICollection<RolePermission> rps)
        => SetProperty(role, "RolePermissions", rps);

    public static void AttachUserRolesToUser(User user, ICollection<UserRole> userRoles)
        => SetProperty(user, "UserRoles", userRoles);

    public static void AttachUserGroupsToGroup(Group group, ICollection<UserGroup> userGroups)
        => SetProperty(group, "UserGroups", userGroups);
}
