
using FluentAssertions;

namespace Adrenalin.UnitTests.RBAC.Domain.UserGroup;

public sealed class UserGroupEntityTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid GroupId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    private static Adrenalin.Modules.Auth.Domain.Entities.UserGroup MakeUserGroup(
        Guid? userId = null, Guid? groupId = null, bool isLead = false, Guid? addedBy = null)
        => Adrenalin.Modules.Auth.Domain.Entities.UserGroup.Add(
            userId ?? UserId, groupId ?? GroupId, isLead, addedBy ?? ActorId);

    // ── Add – Happy Path ──────────────────────────────────────────────────────

    [Fact]
    public void Add_Should_Create_UserGroup_When_Data_Is_Valid()
    {
        var ug = MakeUserGroup();

        ug.UserId.Should().Be(UserId);
        ug.GroupId.Should().Be(GroupId);
    }

    [Fact]
    public void Add_Should_Set_IsLead()
    {
        var ug = MakeUserGroup(isLead: true);

        ug.IsLead.Should().BeTrue();
    }

    [Fact]
    public void Add_Should_Set_IsLead_False_By_Default()
    {
        var ug = MakeUserGroup();

        ug.IsLead.Should().BeFalse();
    }

    [Fact]
    public void Add_Should_Set_IsDeleted_False()
    {
        var ug = MakeUserGroup();

        ug.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Add_Should_Set_CreatedBy()
    {
        var actorId = Guid.NewGuid();
        var ug = MakeUserGroup(addedBy: actorId);

        ug.CreatedBy.Should().Be(actorId);
    }

    // ── Add – Validation Failures ─────────────────────────────────────────────

    [Fact]
    public void Add_Should_Throw_When_UserId_IsEmpty()
    {
        var act = () => MakeUserGroup(userId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*userId required*");
    }

    [Fact]
    public void Add_Should_Throw_When_GroupId_IsEmpty()
    {
        var act = () => MakeUserGroup(groupId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*groupId required*");
    }

    // ── Restore – Happy Path ──────────────────────────────────────────────────

    [Fact]
    public void Restore_Should_Set_IsDeleted_False()
    {
        var ug = MakeUserGroup();
        ug.SoftDelete(ActorId);

        ug.Restore(true, ActorId);

        ug.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Restore_Should_Update_IsLead()
    {
        var ug = MakeUserGroup(isLead: false);
        ug.SoftDelete(ActorId);

        ug.Restore(true, ActorId);

        ug.IsLead.Should().BeTrue();
    }

    [Fact]
    public void Restore_Should_Set_UpdatedBy()
    {
        var ug = MakeUserGroup();
        ug.SoftDelete(ActorId);
        var newActor = Guid.NewGuid();

        ug.Restore(false, newActor);

        ug.UpdatedBy.Should().Be(newActor);
    }

    // ── SetLead – Happy Path ──────────────────────────────────────────────────

    [Fact]
    public void SetLead_Should_Update_IsLead_Flag()
    {
        var ug = MakeUserGroup(isLead: false);

        ug.SetLead(true, ActorId);

        ug.IsLead.Should().BeTrue();
    }

    [Fact]
    public void SetLead_Should_Set_UpdatedBy()
    {
        var ug = MakeUserGroup();
        var actorId = Guid.NewGuid();

        ug.SetLead(true, actorId);

        ug.UpdatedBy.Should().Be(actorId);
    }

    // ── SetLead – Business Rule Violations ───────────────────────────────────

    [Fact]
    public void SetLead_Should_Throw_When_Membership_Is_Deleted()
    {
        var ug = MakeUserGroup();
        ug.SoftDelete(ActorId);

        var act = () => ug.SetLead(true, ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot modify removed membership*");
    }

    // ── SoftDelete – Happy Path ───────────────────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Set_IsDeleted_True()
    {
        var ug = MakeUserGroup();

        ug.SoftDelete(ActorId);

        ug.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedBy()
    {
        var ug = MakeUserGroup();
        var actorId = Guid.NewGuid();

        ug.SoftDelete(actorId);

        ug.UpdatedBy.Should().Be(actorId);
    }

    // ── SoftDelete – Business Rule Violations ─────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Throw_When_Already_Removed()
    {
        var ug = MakeUserGroup();
        ug.SoftDelete(ActorId);

        var act = () => ug.SoftDelete(ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Membership already removed*");
    }
}
