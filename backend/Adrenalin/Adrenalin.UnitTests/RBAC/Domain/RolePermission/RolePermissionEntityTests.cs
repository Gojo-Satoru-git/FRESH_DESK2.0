using Adrenalin.Modules.Auth.Domain.Entities;
using FluentAssertions;

namespace Adrenalin.UnitTests.RBAC.Domain.RolePermission;

public sealed class RolePermissionEntityTests
{
    private static readonly Guid RoleId = Guid.NewGuid();
    private static readonly Guid PermissionId = Guid.NewGuid();
    private static readonly Guid ActorId = Guid.NewGuid();

    private static Adrenalin.Modules.Auth.Domain.Entities.RolePermission MakeRp(
        Guid? roleId = null, Guid? permissionId = null, Guid? assignedBy = null)
        => Adrenalin.Modules.Auth.Domain.Entities.RolePermission.Assign(
            roleId ?? RoleId, permissionId ?? PermissionId, assignedBy ?? ActorId);

    // ── Assign – Happy Path ───────────────────────────────────────────────────

    [Fact]
    public void Assign_Should_Create_RolePermission_When_Data_Is_Valid()
    {
        var rp = MakeRp();

        rp.Should().NotBeNull();
        rp.RoleId.Should().Be(RoleId);
        rp.PermissionId.Should().Be(PermissionId);
    }

    [Fact]
    public void Assign_Should_Set_NonEmpty_Id()
    {
        var rp = MakeRp();

        rp.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Assign_Should_Set_IsDeleted_False()
    {
        var rp = MakeRp();

        rp.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Assign_Should_Set_CreatedBy()
    {
        var actorId = Guid.NewGuid();
        var rp = MakeRp(assignedBy: actorId);

        rp.CreatedBy.Should().Be(actorId);
    }

    [Fact]
    public void Assign_Should_Set_CreatedAt_To_Recent_Utc()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var rp = MakeRp();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        rp.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    // ── Assign – Validation Failures ─────────────────────────────────────────

    [Fact]
    public void Assign_Should_Throw_When_RoleId_IsEmpty()
    {
        var act = () => MakeRp(roleId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*roleId must not be empty*");
    }

    [Fact]
    public void Assign_Should_Throw_When_PermissionId_IsEmpty()
    {
        var act = () => MakeRp(permissionId: Guid.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*permissionId must not be empty*");
    }

    // ── SoftDelete – Happy Path ───────────────────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Set_IsDeleted_True()
    {
        var rp = MakeRp();

        rp.SoftDelete(ActorId);

        rp.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedBy()
    {
        var rp = MakeRp();
        var actorId = Guid.NewGuid();

        rp.SoftDelete(actorId);

        rp.UpdatedBy.Should().Be(actorId);
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedAt()
    {
        var rp = MakeRp();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        rp.SoftDelete(ActorId);

        rp.UpdatedAt.Should().NotBeNull();
        rp.UpdatedAt!.Value.Should().BeAfter(before);
    }

    // ── SoftDelete – Business Rule Violations ─────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Throw_When_Already_Removed()
    {
        var rp = MakeRp();
        rp.SoftDelete(ActorId);

        var act = () => rp.SoftDelete(ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Already removed*");
    }
}
