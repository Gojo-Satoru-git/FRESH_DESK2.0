using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Handlers;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Adrenalin.UnitTests.RBAC.Authorization;

// ═══════════════════════════════════════════════════════════════════════════
// Permission Evaluation — effective permissions resolution
// ═══════════════════════════════════════════════════════════════════════════

public sealed class EffectivePermissionsAuthorizationTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly GetUserEffectivePermissionsQueryHandler _sut;

    public EffectivePermissionsAuthorizationTests()
        => _sut = new Adrenalin.Modules.Auth.Application.Handlers.GetUserEffectivePermissionsQueryHandler(_userRepo.Object);

    [Fact]
    public async Task HasPermission_Should_ReturnTrue_When_UserHasPermission()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetEffectivePermissionsAsync(userId, default))
                 .ReturnsAsync(new List<string> { "ticket:read", "ticket:write" });

        var result = await _sut.Handle(
            new Adrenalin.Modules.Auth.Application.Queries.GetUserEffectivePermissionsQuery(userId), default);

        result.Value.Should().Contain("ticket:read");
    }

    [Fact]
    public async Task HasPermission_Should_ReturnFalse_When_UserLacksPermission()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetEffectivePermissionsAsync(userId, default))
                 .ReturnsAsync(new List<string> { "ticket:read" });

        var result = await _sut.Handle(
            new Adrenalin.Modules.Auth.Application.Queries.GetUserEffectivePermissionsQuery(userId), default);

        result.Value.Should().NotContain("ticket:delete");
    }

    [Fact]
    public async Task HasPermission_Should_Return_Empty_When_User_Has_No_Roles()
    {
        var userId = Guid.NewGuid();
        _userRepo.Setup(r => r.GetEffectivePermissionsAsync(userId, default))
                 .ReturnsAsync(new List<string>());

        var result = await _sut.Handle(
            new Adrenalin.Modules.Auth.Application.Queries.GetUserEffectivePermissionsQuery(userId), default);

        result.Value.Should().BeEmpty();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// System Role Protection
// ═══════════════════════════════════════════════════════════════════════════

public sealed class SystemRoleProtectionTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IUserRoleRepository> _userRoleRepo = new();
    private readonly DeleteRoleCommandHandler _sut;

    public SystemRoleProtectionTests()
        => _sut = new DeleteRoleCommandHandler(_roleRepo.Object, _userRoleRepo.Object);

    [Fact]
    public async Task DeleteRole_Should_Prevent_Deletion_Of_SystemRole()
    {
        var role = Role.Create("SuperAdmin", null, Guid.NewGuid());
        typeof(Role).GetProperty(nameof(Role.IsSystemRole))!.SetValue(role, true);
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);

        var result = await _sut.Handle(new DeleteRoleCommand(role.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("System roles cannot be deleted");
    }

    [Fact]
    public async Task DeleteRole_Should_Not_Cascade_Delete_UserRoles_When_System_Role_Prevents_Delete()
    {
        var role = Role.Create("SuperAdmin", null, Guid.NewGuid());
        typeof(Role).GetProperty(nameof(Role.IsSystemRole))!.SetValue(role, true);
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);

        await _sut.Handle(new DeleteRoleCommand(role.Id, Guid.NewGuid()), default);

        // SoftDeleteByRoleAsync should NOT have been called since the role throws before reaching that line
        _userRoleRepo.Verify(r => r.SoftDeleteByRoleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default), Times.Never);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Permission Escalation Prevention
// ═══════════════════════════════════════════════════════════════════════════

public sealed class PermissionEscalationTests
{
    // Verifies that granting a permission requires the role to exist —
    // you cannot smuggle in unknown role IDs to create phantom associations.

    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPermissionRepository> _permRepo = new();
    private readonly Mock<IRolePermissionRepository> _rpRepo = new();
    private readonly GrantPermissionToRoleCommandHandler _sut;

    public PermissionEscalationTests()
        => _sut = new GrantPermissionToRoleCommandHandler(_roleRepo.Object, _permRepo.Object, _rpRepo.Object);

    [Fact]
    public async Task GrantPermission_Should_Reject_Unknown_RoleId()
    {
        _roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

        var result = await _sut.Handle(
            new GrantPermissionToRoleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        _rpRepo.Verify(r => r.Add(It.IsAny<RolePermission>()), Times.Never);
    }

    [Fact]
    public async Task GrantPermission_Should_Reject_Unknown_PermissionId()
    {
        var role = Role.Create("Agent", null, Guid.NewGuid());
        _roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        _permRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Permission?)null);

        var result = await _sut.Handle(
            new GrantPermissionToRoleCommand(role.Id, Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        _rpRepo.Verify(r => r.Add(It.IsAny<RolePermission>()), Times.Never);
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Claims Extraction — controller actor resolution
// ═══════════════════════════════════════════════════════════════════════════

public sealed class ClaimsExtractionTests
{
    // These tests exercise the private GetActorId() logic via the controller's
    // public action methods. We verify that missing / malformed claims prevent mutation.

    private readonly Mock<Adrenalin.SharedKernel.Mediator.IDispatcher> _dispatcher = new();
    private readonly Adrenalin.unify.API.Controllers.Auth.UsersRbacController _sut;

    public ClaimsExtractionTests()
    {
        _sut = new Adrenalin.unify.API.Controllers.Auth.UsersRbacController(_dispatcher.Object);
    }

    [Fact]
    public async Task Controller_Should_Return_Unauthorized_When_Sub_Claim_Is_Missing()
    {
        _sut.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity()) // no sub claim
            }
        };

        var result = await _sut.AssignRole(Guid.NewGuid(),
            new Adrenalin.unify.API.Controllers.Auth.RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>();
    }

    [Fact]
    public async Task Controller_Should_Return_Unauthorized_When_Sub_Claim_Is_Not_A_Guid()
    {
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "not-a-guid") };
        _sut.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(claims, "TestAuth"))
            }
        };

        var result = await _sut.AssignRole(Guid.NewGuid(),
            new Adrenalin.unify.API.Controllers.Auth.RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.UnauthorizedResult>();
    }

    [Fact]
    public async Task Controller_Should_Accept_Sub_Claim_From_Jwt_Convention()
    {
        var actorId = Guid.NewGuid();
        var claims = new[] { new System.Security.Claims.Claim("sub", actorId.ToString()) };
        _sut.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(
                    new System.Security.Claims.ClaimsIdentity(claims, "TestAuth"))
            }
        };
        _dispatcher.Setup(d => d.Send(It.IsAny<AssignRoleToUserCommand>(), default))
                   .ReturnsAsync(Adrenalin.SharedKernel.Results.Result.Success());

        var result = await _sut.AssignRole(Guid.NewGuid(),
            new Adrenalin.unify.API.Controllers.Auth.RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.NoContentResult>();
        _dispatcher.Verify(d => d.Send(
            It.Is<AssignRoleToUserCommand>(c => c.ActorId == actorId), default), Times.Once);
    }
}
