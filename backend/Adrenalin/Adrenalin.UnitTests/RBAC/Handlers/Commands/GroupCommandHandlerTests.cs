using Adrenalin.Modules.Auth.Application.Commands;
using Adrenalin.Modules.Auth.Application.Handlers;
using Adrenalin.Modules.Auth.Domain.Entities;
using Adrenalin.Modules.Auth.Domain.Interfaces;
using Adrenalin.UnitTests.RBAC.TestHelpers;
using FluentAssertions;
using Moq;

namespace Adrenalin.UnitTests.RBAC.Handlers.Commands;

// ═══════════════════════════════════════════════════════════════════════════
// CreateGroupCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class CreateGroupCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly CreateGroupCommandHandler _sut;

    public CreateGroupCommandHandlerTests()
        => _sut = new CreateGroupCommandHandler(_groupRepo.Object);

    private static CreateGroupCommand ValidCommand(string name = "Support Team") =>
        new(name, "EU", "T1", 30, Guid.NewGuid());

    [Fact]
    public async Task CreateGroupCommandHandler_Should_Create_Group_When_Valid()
    {
        _groupRepo.Setup(r => r.ExistsByNameAsync("Support Team", default)).ReturnsAsync(false);

        var result = await _sut.Handle(ValidCommand("Support Team"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateGroupCommandHandler_Should_Call_Add()
    {
        _groupRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);

        await _sut.Handle(ValidCommand(), default);

        _groupRepo.Verify(r => r.Add(It.IsAny<Group>()), Times.Once);
    }

    [Fact]
    public async Task CreateGroupCommandHandler_Should_Call_SaveChanges()
    {
        _groupRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);

        await _sut.Handle(ValidCommand(), default);

        _groupRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateGroupCommandHandler_Should_ReturnFailure_When_Name_Already_Exists()
    {
        _groupRepo.Setup(r => r.ExistsByNameAsync("Support Team", default)).ReturnsAsync(true);

        var result = await _sut.Handle(ValidCommand("Support Team"), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Support Team");
        _groupRepo.Verify(r => r.Add(It.IsAny<Group>()), Times.Never);
    }

    [Fact]
    public async Task CreateGroupCommandHandler_Should_ReturnFailure_When_Repository_Throws()
    {
        _groupRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default))
                  .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.Handle(ValidCommand(), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("DB error");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// UpdateGroupCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class UpdateGroupCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly UpdateGroupCommandHandler _sut;

    public UpdateGroupCommandHandlerTests()
        => _sut = new UpdateGroupCommandHandler(_groupRepo.Object);

    [Fact]
    public async Task UpdateGroupCommandHandler_Should_Update_Group_When_Valid()
    {
        var group = new GroupBuilder().WithName("Old Name").Build();
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);
        _groupRepo.Setup(r => r.ExistsByNameAsync("New Name", default)).ReturnsAsync(false);

        var result = await _sut.Handle(
            new UpdateGroupCommand(group.Id, "New Name", "US", "T2", 60, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        group.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task UpdateGroupCommandHandler_Should_Call_Update_And_SaveChanges()
    {
        var group = new GroupBuilder().Build();
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);
        _groupRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(false);

        await _sut.Handle(
            new UpdateGroupCommand(group.Id, "Updated", null, null, 15, Guid.NewGuid()), default);

        _groupRepo.Verify(r => r.Update(It.IsAny<Group>()), Times.Once);
        _groupRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateGroupCommandHandler_Should_ReturnFailure_When_Group_NotFound()
    {
        _groupRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Group?)null);

        var result = await _sut.Handle(
            new UpdateGroupCommand(Guid.NewGuid(), "Name", null, null, 10, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateGroupCommandHandler_Should_ReturnFailure_When_New_Name_Exists()
    {
        var group = new GroupBuilder().WithName("Old Name").Build();
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);
        _groupRepo.Setup(r => r.ExistsByNameAsync("Taken", default)).ReturnsAsync(true);

        var result = await _sut.Handle(
            new UpdateGroupCommand(group.Id, "Taken", null, null, 10, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Taken");
    }

    [Fact]
    public async Task UpdateGroupCommandHandler_Should_Succeed_When_Name_Is_Same_Case_Insensitive()
    {
        var group = new GroupBuilder().WithName("Support").Build();
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);
        _groupRepo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), default)).ReturnsAsync(true);

        var result = await _sut.Handle(
            new UpdateGroupCommand(group.Id, "SUPPORT", null, null, 10, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// DeleteGroupCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class DeleteGroupCommandHandlerTests
{
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly Mock<IUserGroupRepository> _ugRepo = new();
    private readonly DeleteGroupCommandHandler _sut;

    public DeleteGroupCommandHandlerTests()
        => _sut = new DeleteGroupCommandHandler(_groupRepo.Object, _ugRepo.Object);

    [Fact]
    public async Task DeleteGroupCommandHandler_Should_SoftDelete_Group()
    {
        var group = new GroupBuilder().Build();
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);

        var result = await _sut.Handle(new DeleteGroupCommand(group.Id, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        group.IsDeleted.Should().BeTrue();
        group.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteGroupCommandHandler_Should_SoftDelete_UserGroup_Memberships()
    {
        var group = new GroupBuilder().Build();
        var actorId = Guid.NewGuid();
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);

        await _sut.Handle(new DeleteGroupCommand(group.Id, actorId), default);

        _ugRepo.Verify(r => r.SoftDeleteByGroupAsync(group.Id, actorId, default), Times.Once);
    }

    [Fact]
    public async Task DeleteGroupCommandHandler_Should_Call_Update_And_SaveChanges()
    {
        var group = new GroupBuilder().Build();
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);

        await _sut.Handle(new DeleteGroupCommand(group.Id, Guid.NewGuid()), default);

        _groupRepo.Verify(r => r.Update(It.IsAny<Group>()), Times.Once);
        _groupRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteGroupCommandHandler_Should_ReturnFailure_When_Group_NotFound()
    {
        _groupRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Group?)null);

        var result = await _sut.Handle(new DeleteGroupCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// AddUserToGroupCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class AddUserToGroupCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IGroupRepository> _groupRepo = new();
    private readonly Mock<IUserGroupRepository> _ugRepo = new();
    private readonly AddUserToGroupCommandHandler _sut;

    public AddUserToGroupCommandHandlerTests()
        => _sut = new AddUserToGroupCommandHandler(_userRepo.Object, _groupRepo.Object, _ugRepo.Object);

    private User MakeUser() => new UserBuilder().Build();
    private Group MakeGroup() => new GroupBuilder().Build();

    [Fact]
    public async Task AddUserToGroup_Should_Add_User_When_Not_Already_Member()
    {
        var user = MakeUser();
        var group = MakeGroup();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);
        _ugRepo.Setup(r => r.GetAsync(user.Id, group.Id, default)).ReturnsAsync((UserGroup?)null);
        _ugRepo.Setup(r => r.GetIncludingDeletedAsync(user.Id, group.Id, default)).ReturnsAsync((UserGroup?)null);

        var result = await _sut.Handle(
            new AddUserToGroupCommand(user.Id, group.Id, false, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _ugRepo.Verify(r => r.Add(It.IsAny<UserGroup>()), Times.Once);
    }

    [Fact]
    public async Task AddUserToGroup_Should_Be_Idempotent_When_Already_Member()
    {
        var user = MakeUser();
        var group = MakeGroup();
        var existing = UserGroup.Add(user.Id, group.Id, false, Guid.NewGuid());
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);
        _ugRepo.Setup(r => r.GetAsync(user.Id, group.Id, default)).ReturnsAsync(existing);

        var result = await _sut.Handle(
            new AddUserToGroupCommand(user.Id, group.Id, false, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        _ugRepo.Verify(r => r.Add(It.IsAny<UserGroup>()), Times.Never);
    }

    [Fact]
    public async Task AddUserToGroup_Should_Restore_Deleted_Membership()
    {
        var user = MakeUser();
        var group = MakeGroup();
        var deleted = UserGroup.Add(user.Id, group.Id, false, Guid.NewGuid());
        deleted.SoftDelete(Guid.NewGuid());
        var actorId = Guid.NewGuid();

        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _groupRepo.Setup(r => r.GetByIdAsync(group.Id, default)).ReturnsAsync(group);
        _ugRepo.Setup(r => r.GetAsync(user.Id, group.Id, default)).ReturnsAsync((UserGroup?)null);
        _ugRepo.Setup(r => r.GetIncludingDeletedAsync(user.Id, group.Id, default)).ReturnsAsync(deleted);

        var result = await _sut.Handle(
            new AddUserToGroupCommand(user.Id, group.Id, true, actorId), default);

        result.IsSuccess.Should().BeTrue();
        deleted.IsDeleted.Should().BeFalse();
        deleted.IsLead.Should().BeTrue();
        _ugRepo.Verify(r => r.Update(It.IsAny<UserGroup>()), Times.Once);
        _ugRepo.Verify(r => r.Add(It.IsAny<UserGroup>()), Times.Never);
    }

    [Fact]
    public async Task AddUserToGroup_Should_ReturnFailure_When_User_NotFound()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((User?)null);

        var result = await _sut.Handle(
            new AddUserToGroupCommand(Guid.NewGuid(), Guid.NewGuid(), false, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task AddUserToGroup_Should_ReturnFailure_When_Group_NotFound()
    {
        var user = MakeUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        _groupRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Group?)null);

        var result = await _sut.Handle(
            new AddUserToGroupCommand(user.Id, Guid.NewGuid(), false, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// RemoveUserFromGroupCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class RemoveUserFromGroupCommandHandlerTests
{
    private readonly Mock<IUserGroupRepository> _ugRepo = new();
    private readonly RemoveUserFromGroupCommandHandler _sut;

    public RemoveUserFromGroupCommandHandlerTests()
        => _sut = new RemoveUserFromGroupCommandHandler(_ugRepo.Object);

    [Fact]
    public async Task RemoveUserFromGroup_Should_SoftDelete_Membership()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var ug = UserGroup.Add(userId, groupId, false, Guid.NewGuid());
        _ugRepo.Setup(r => r.GetAsync(userId, groupId, default)).ReturnsAsync(ug);

        var result = await _sut.Handle(
            new RemoveUserFromGroupCommand(userId, groupId, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        ug.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveUserFromGroup_Should_Call_Update_And_SaveChanges()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var ug = UserGroup.Add(userId, groupId, false, Guid.NewGuid());
        _ugRepo.Setup(r => r.GetAsync(userId, groupId, default)).ReturnsAsync(ug);

        await _sut.Handle(new RemoveUserFromGroupCommand(userId, groupId, Guid.NewGuid()), default);

        _ugRepo.Verify(r => r.Update(It.IsAny<UserGroup>()), Times.Once);
        _ugRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RemoveUserFromGroup_Should_ReturnFailure_When_Not_Member()
    {
        _ugRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
               .ReturnsAsync((UserGroup?)null);

        var result = await _sut.Handle(
            new RemoveUserFromGroupCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not a member");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// SetGroupLeadCommandHandler
// ═══════════════════════════════════════════════════════════════════════════

public sealed class SetGroupLeadCommandHandlerTests
{
    private readonly Mock<IUserGroupRepository> _ugRepo = new();
    private readonly SetGroupLeadCommandHandler _sut;

    public SetGroupLeadCommandHandlerTests()
        => _sut = new SetGroupLeadCommandHandler(_ugRepo.Object);

    [Fact]
    public async Task SetGroupLead_Should_Update_IsLead_Flag()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var ug = UserGroup.Add(userId, groupId, false, Guid.NewGuid());
        _ugRepo.Setup(r => r.GetAsync(userId, groupId, default)).ReturnsAsync(ug);

        var result = await _sut.Handle(
            new SetGroupLeadCommand(userId, groupId, true, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        ug.IsLead.Should().BeTrue();
    }

    [Fact]
    public async Task SetGroupLead_Should_Call_Update_And_SaveChanges()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var ug = UserGroup.Add(userId, groupId, false, Guid.NewGuid());
        _ugRepo.Setup(r => r.GetAsync(userId, groupId, default)).ReturnsAsync(ug);

        await _sut.Handle(new SetGroupLeadCommand(userId, groupId, true, Guid.NewGuid()), default);

        _ugRepo.Verify(r => r.Update(It.IsAny<UserGroup>()), Times.Once);
        _ugRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task SetGroupLead_Should_ReturnFailure_When_Not_Member()
    {
        _ugRepo.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
               .ReturnsAsync((UserGroup?)null);

        var result = await _sut.Handle(
            new SetGroupLeadCommand(Guid.NewGuid(), Guid.NewGuid(), true, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not a member");
    }

    [Fact]
    public async Task SetGroupLead_Should_Set_IsLead_False()
    {
        var userId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var ug = UserGroup.Add(userId, groupId, true, Guid.NewGuid());
        _ugRepo.Setup(r => r.GetAsync(userId, groupId, default)).ReturnsAsync(ug);

        var result = await _sut.Handle(
            new SetGroupLeadCommand(userId, groupId, false, Guid.NewGuid()), default);

        result.IsSuccess.Should().BeTrue();
        ug.IsLead.Should().BeFalse();
    }
}
