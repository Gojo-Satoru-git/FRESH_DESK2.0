using System.Security.Claims;
using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.DTOs;
using Adrenalin.Modules.Auth.Application.Queries;
using Adrenalin.SharedKernel.Mediator;
using Adrenalin.SharedKernel.Results;
using Adrenalin.unify.API.Controllers.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Adrenalin.UnitTests.RBAC.Controllers;

public sealed class UsersRbacControllerTests
{
    private readonly Mock<IDispatcher> _dispatcher = new();
    private readonly UsersRbacController _sut;
    private readonly Guid _actorId = Guid.NewGuid();

    public UsersRbacControllerTests()
    {
        _sut = new UsersRbacController(_dispatcher.Object);
        SetAuthenticatedUser(_actorId);
    }

    private void SetAuthenticatedUser(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    private void SetUnauthenticatedUser()
    {
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };
    }

    // ── GET /api/rbac/users ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_Should_Return_Ok_With_Paged_Results()
    {
        var pagedResult = new PagedResultDto<UserSummaryDto>(
                new List<UserSummaryDto> { new(Guid.NewGuid(), "a@b.com", "John", "Doe", true) },
                TotalCount: 1, PageNumber: 1, PageSize: 20);

        _dispatcher.Setup(d => d.Send(It.IsAny<GetUsersQuery>(), default))
                   .ReturnsAsync(Result<PagedResultDto<UserSummaryDto>>.Success(pagedResult));

        var result = await _sut.GetAll(null, null, 1, 20);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);
        ok.Value.Should().Be(pagedResult);
    }

    [Fact]
    public async Task GetAll_Should_Return_BadRequest_When_Query_Fails()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<GetUsersQuery>(), default))
                   .ReturnsAsync(Result<PagedResultDto<UserSummaryDto>>.Failure("DB error"));

        var result = await _sut.GetAll(null, null, 1, 20);

        result.Should().BeOfType<BadRequestObjectResult>()
              .Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetAll_Should_Pass_Filter_To_Dispatcher()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<GetUsersQuery>(), default))
                   .ReturnsAsync(Result<PagedResultDto<UserSummaryDto>>.Success(
                       new PagedResultDto<UserSummaryDto>(new List<UserSummaryDto>(), TotalCount: 0, PageNumber: 1, PageSize: 10)));

        await _sut.GetAll("john", true, 2, 10);

        _dispatcher.Verify(d => d.Send(
            It.Is<GetUsersQuery>(q => q.EmailQuery == "john" && q.IsActive == true && q.PageNumber == 2 && q.PageSize == 10),
            default), Times.Once);
    }

    // ── GET /api/rbac/users/{id}/roles ───────────────────────────────────────

    [Fact]
    public async Task GetWithRoles_Should_Return_Ok_When_User_Found()
    {
        var userId = Guid.NewGuid();
        var dto = new UserWithRolesDto(userId, "a@b.com", "John", "Doe", true,
            new List<RoleSummaryDto>(), DateTimeOffset.UtcNow, null);

        _dispatcher.Setup(d => d.Send(It.IsAny<GetUserWithRolesQuery>(), default))
                   .ReturnsAsync(Result<UserWithRolesDto>.Success(dto));

        var result = await _sut.GetWithRoles(userId, default);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetWithRoles_Should_Return_NotFound_When_User_Not_Found()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<GetUserWithRolesQuery>(), default))
                   .ReturnsAsync(Result<UserWithRolesDto>.Failure("User not found."));

        var result = await _sut.GetWithRoles(Guid.NewGuid(), default);

        result.Should().BeOfType<NotFoundObjectResult>()
              .Which.StatusCode.Should().Be(404);
    }

    // ── GET /api/rbac/users/{id}/permissions ─────────────────────────────────

    [Fact]
    public async Task GetEffectivePermissions_Should_Return_Ok_With_Permission_List()
    {
        var userId = Guid.NewGuid();
        var perms = new List<string> { "ticket:read", "ticket:write" };

        _dispatcher.Setup(d => d.Send(It.IsAny<GetUserEffectivePermissionsQuery>(), default))
                   .ReturnsAsync(Result<IReadOnlyList<string>>.Success(perms));

        var result = await _sut.GetEffectivePermissions(userId, default);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(perms);
    }

    [Fact]
    public async Task GetEffectivePermissions_Should_Return_NotFound_On_Failure()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<GetUserEffectivePermissionsQuery>(), default))
                   .ReturnsAsync(Result<IReadOnlyList<string>>.Failure("User not found."));

        var result = await _sut.GetEffectivePermissions(Guid.NewGuid(), default);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── GET /api/rbac/users/{id}/groups ─────────────────────────────────────

    [Fact]
    public async Task GetGroups_Should_Return_Ok_With_Groups()
    {
        var groups = new List<GroupDto>
        {
            new(Guid.NewGuid(), "Team A", "EU", "T1", 30, true, DateTimeOffset.UtcNow, null)
        };

        _dispatcher.Setup(d => d.Send(It.IsAny<GetUserGroupsQuery>(), default))
                   .ReturnsAsync(Result<IReadOnlyList<GroupDto>>.Success(groups));

        var result = await _sut.GetGroups(Guid.NewGuid(), default);

        result.Should().BeOfType<OkObjectResult>()
              .Which.Value.Should().Be(groups);
    }

    // ── POST /api/rbac/users/{id}/roles/assign ───────────────────────────────

    [Fact]
    public async Task AssignRole_Should_Return_NoContent_When_Success()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<AssignRoleToUserCommand>(), default))
                   .ReturnsAsync(Result.Success());

        var result = await _sut.AssignRole(Guid.NewGuid(), new RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<NoContentResult>()
              .Which.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task AssignRole_Should_Return_BadRequest_When_Command_Fails()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<AssignRoleToUserCommand>(), default))
                   .ReturnsAsync(Result.Failure("Role not found."));

        var result = await _sut.AssignRole(Guid.NewGuid(), new RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<BadRequestObjectResult>()
              .Which.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task AssignRole_Should_Return_Unauthorized_When_No_Actor()
    {
        SetUnauthenticatedUser();

        var result = await _sut.AssignRole(Guid.NewGuid(), new RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task AssignRole_Should_Dispatch_With_Correct_UserId_And_RoleId()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        _dispatcher.Setup(d => d.Send(It.IsAny<AssignRoleToUserCommand>(), default))
                   .ReturnsAsync(Result.Success());

        await _sut.AssignRole(userId, new RoleIdRequest(roleId), default);

        _dispatcher.Verify(d => d.Send(
            It.Is<AssignRoleToUserCommand>(c => c.UserId == userId && c.RoleId == roleId),
            default), Times.Once);
    }

    // ── POST /api/rbac/users/{id}/roles/remove ───────────────────────────────

    [Fact]
    public async Task RemoveRole_Should_Return_NoContent_When_Success()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<RemoveRoleFromUserCommand>(), default))
                   .ReturnsAsync(Result.Success());

        var result = await _sut.RemoveRole(Guid.NewGuid(), new RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task RemoveRole_Should_Return_BadRequest_When_Command_Fails()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<RemoveRoleFromUserCommand>(), default))
                   .ReturnsAsync(Result.Failure("Role not assigned."));

        var result = await _sut.RemoveRole(Guid.NewGuid(), new RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RemoveRole_Should_Return_Unauthorized_When_No_Actor()
    {
        SetUnauthenticatedUser();

        var result = await _sut.RemoveRole(Guid.NewGuid(), new RoleIdRequest(Guid.NewGuid()), default);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    // ── PUT /api/rbac/users/{id}/roles ───────────────────────────────────────

    [Fact]
    public async Task SetRoles_Should_Return_NoContent_When_Success()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<SetUserRolesCommand>(), default))
                   .ReturnsAsync(Result.Success());

        var result = await _sut.SetRoles(Guid.NewGuid(),
            new SetRolesRequest(new[] { Guid.NewGuid() }.ToList().AsReadOnly()), default);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task SetRoles_Should_Return_BadRequest_When_Command_Fails()
    {
        _dispatcher.Setup(d => d.Send(It.IsAny<SetUserRolesCommand>(), default))
                   .ReturnsAsync(Result.Failure("Role not found."));

        var result = await _sut.SetRoles(Guid.NewGuid(),
            new SetRolesRequest(new[] { Guid.NewGuid() }.ToList().AsReadOnly()), default);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SetRoles_Should_Return_Unauthorized_When_No_Actor()
    {
        SetUnauthenticatedUser();

        var result = await _sut.SetRoles(Guid.NewGuid(),
            new SetRolesRequest(Array.Empty<Guid>().ToList().AsReadOnly()), default);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task SetRoles_Should_Dispatch_With_Correct_RoleIds()
    {
        var userId = Guid.NewGuid();
        var roleIds = new[] { Guid.NewGuid(), Guid.NewGuid() }.ToList().AsReadOnly();
        _dispatcher.Setup(d => d.Send(It.IsAny<SetUserRolesCommand>(), default))
                   .ReturnsAsync(Result.Success());

        await _sut.SetRoles(userId, new SetRolesRequest(roleIds), default);

        _dispatcher.Verify(d => d.Send(
            It.Is<SetUserRolesCommand>(c => c.UserId == userId && c.RoleIds.SequenceEqual(roleIds)),
            default), Times.Once);
    }
}
