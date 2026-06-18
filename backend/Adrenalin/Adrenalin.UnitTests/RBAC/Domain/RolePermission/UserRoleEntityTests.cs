
using FluentAssertions;

namespace Adrenalin.UnitTests.RBAC.Domain.UserRole;

public sealed class UserRoleEntityTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid RoleId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    private static Adrenalin.Modules.Auth.Domain.Entities.UserRole MakeUserRole(
        Guid? userId = null, Guid? roleId = null, Guid? assignedBy = null)
        => Adrenalin.Modules.Auth.Domain.Entities.UserRole.Assign(
            userId ?? UserId, roleId ?? RoleId, assignedBy ?? ActorId);

    // ── Assign – Happy Path ───────────────────────────────────────────────────

    [Fact]
    public void Assign_Should_Create_UserRole_When_Data_Is_Valid()
    {
        var ur = MakeUserRole();

        ur.UserId.Should().Be(UserId);
        ur.RoleId.Should().Be(RoleId);
    }

    [Fact]
    public void Assign_Should_Set_AssignedAt_To_Recent_Utc()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var ur = MakeUserRole();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        ur.AssignedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void Assign_Should_Set_AssignedBy()
    {
        var actorId = Guid.NewGuid();
        var ur = MakeUserRole(assignedBy: actorId);

        ur.AssignedBy.Should().Be(actorId);
    }

    [Fact]
    public void Assign_Should_Set_IsDeleted_False()
    {
        var ur = MakeUserRole();

        ur.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Assign_Should_Set_CreatedBy()
    {
        var actorId = Guid.NewGuid();
        var ur = MakeUserRole(assignedBy: actorId);

        ur.CreatedBy.Should().Be(actorId);
    }

    [Fact]
    public void Assign_Should_Set_CreatedAt_Matches_AssignedAt()
    {
        var ur = MakeUserRole();

        ur.CreatedAt.Should().BeCloseTo(ur.AssignedAt, TimeSpan.FromSeconds(1));
    }

    // ── Assign – Validation Failures ─────────────────────────────────────────

    [Fact]
    public void Assign_Should_Throw_When_UserId_IsEmpty()
    {
        var act = () => MakeUserRole(userId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*userId required*");
    }

    [Fact]
    public void Assign_Should_Throw_When_RoleId_IsEmpty()
    {
        var act = () => MakeUserRole(roleId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*roleId required*");
    }

    // ── Restore – Happy Path ──────────────────────────────────────────────────

    [Fact]
    public void Restore_Should_Set_IsDeleted_False()
    {
        var ur = MakeUserRole();
        ur.SoftDelete(ActorId);

        ur.Restore(ActorId);

        ur.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Restore_Should_Update_AssignedAt()
    {
        var ur = MakeUserRole();
        ur.SoftDelete(ActorId);
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        ur.Restore(ActorId);

        ur.AssignedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Restore_Should_Set_AssignedBy_To_Actor()
    {
        var ur = MakeUserRole();
        ur.SoftDelete(ActorId);
        var newActor = Guid.NewGuid();

        ur.Restore(newActor);

        ur.AssignedBy.Should().Be(newActor);
    }

    [Fact]
    public void Restore_Should_Set_UpdatedBy()
    {
        var ur = MakeUserRole();
        ur.SoftDelete(ActorId);
        var newActor = Guid.NewGuid();

        ur.Restore(newActor);

        ur.UpdatedBy.Should().Be(newActor);
    }

    // ── SoftDelete – Happy Path ───────────────────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Set_IsDeleted_True()
    {
        var ur = MakeUserRole();

        ur.SoftDelete(ActorId);

        ur.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedBy()
    {
        var ur = MakeUserRole();
        var actorId = Guid.NewGuid();

        ur.SoftDelete(actorId);

        ur.UpdatedBy.Should().Be(actorId);
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedAt()
    {
        var ur = MakeUserRole();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        ur.SoftDelete(ActorId);

        ur.UpdatedAt.Should().NotBeNull();
        ur.UpdatedAt!.Value.Should().BeAfter(before);
    }

    // ── SoftDelete – Business Rule Violations ─────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Throw_When_UserRole_Already_Removed()
    {
        var ur = MakeUserRole();
        ur.SoftDelete(ActorId);

        var act = () => ur.SoftDelete(ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*UserRole already removed*");
    }
}
