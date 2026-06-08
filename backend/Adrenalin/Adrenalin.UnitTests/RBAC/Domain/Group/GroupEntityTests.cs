using Adrenalin.Modules.Auth.Domain.Entities;
using FluentAssertions;

namespace Adrenalin.UnitTests.RBAC.Domain.Group;

public sealed class GroupEntityTests
{
    private static readonly Guid ActorId = Guid.NewGuid();

    private static Adrenalin.Modules.Auth.Domain.Entities.Group MakeGroup(
        string name = "Support Team",
        string? regionCode = "EU",
        string? tierCode = "T1",
        int alertMinutes = 30,
        Guid? createdBy = null)
        => Adrenalin.Modules.Auth.Domain.Entities.Group.Create(
            name, regionCode, tierCode, alertMinutes, createdBy ?? ActorId);

    // ── Create – Happy Path ───────────────────────────────────────────────────

    [Fact]
    public void Create_Should_Create_Group_When_Data_Is_Valid()
    {
        var group = MakeGroup();

        group.Should().NotBeNull();
        group.Name.Should().Be("Support Team");
    }

    [Fact]
    public void Create_Should_Normalize_RegionCode_To_Uppercase()
    {
        var group = MakeGroup(regionCode: "eu");

        group.RegionCode.Should().Be("EU");
    }

    [Fact]
    public void Create_Should_Normalize_TierCode_To_Uppercase()
    {
        var group = MakeGroup(tierCode: "t1");

        group.TierCode.Should().Be("T1");
    }

    [Fact]
    public void Create_Should_Trim_Name()
    {
        var group = MakeGroup("  Support  ");

        group.Name.Should().Be("Support");
    }

    [Fact]
    public void Create_Should_Set_IsActive_True()
    {
        var group = MakeGroup();

        group.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_Should_Set_IsDeleted_False()
    {
        var group = MakeGroup();

        group.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_Should_Set_UnattendedAlertMinutes()
    {
        var group = MakeGroup(alertMinutes: 60);

        group.UnattendedAlertMinutes.Should().Be(60);
    }

    [Fact]
    public void Create_Should_Set_CreatedBy()
    {
        var actorId = Guid.NewGuid();
        var group = MakeGroup(createdBy: actorId);

        group.CreatedBy.Should().Be(actorId);
    }

    [Fact]
    public void Create_Should_Set_CreatedAt_To_Recent_Utc()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var group = MakeGroup();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        group.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void Create_Should_Allow_Null_RegionCode()
    {
        var group = MakeGroup(regionCode: null);

        group.RegionCode.Should().BeNull();
    }

    [Fact]
    public void Create_Should_Allow_Null_TierCode()
    {
        var group = MakeGroup(tierCode: null);

        group.TierCode.Should().BeNull();
    }

    [Fact]
    public void Create_Should_Initialize_Empty_UserGroups_Collection()
    {
        var group = MakeGroup();

        group.UserGroups.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_Set_Minimum_UnattendedAlertMinutes_Of_1()
    {
        var group = MakeGroup(alertMinutes: 1);

        group.UnattendedAlertMinutes.Should().Be(1);
    }

    // ── Create – Validation Failures ─────────────────────────────────────────

    [Fact]
    public void Create_Should_Throw_When_Name_IsEmpty()
    {
        var act = () => MakeGroup(string.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Group name required*");
    }

    [Fact]
    public void Create_Should_Throw_When_Name_IsWhitespace()
    {
        var act = () => MakeGroup("   ");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Group name required*");
    }

    [Fact]
    public void Create_Should_Throw_When_AlertMinutes_Is_Zero()
    {
        var act = () => MakeGroup(alertMinutes: 0);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*UnattendedAlertMinutes must be >= 1*");
    }

    [Fact]
    public void Create_Should_Throw_When_AlertMinutes_Is_Negative()
    {
        var act = () => MakeGroup(alertMinutes: -1);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*UnattendedAlertMinutes must be >= 1*");
    }

    // ── Update – Happy Path ───────────────────────────────────────────────────

    [Fact]
    public void Update_Should_Update_All_Properties()
    {
        var group = MakeGroup();
        var updatedBy = Guid.NewGuid();

        group.Update("New Name", "US", "T2", 45, updatedBy);

        group.Name.Should().Be("New Name");
        group.RegionCode.Should().Be("US");
        group.TierCode.Should().Be("T2");
        group.UnattendedAlertMinutes.Should().Be(45);
        group.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Update_Should_Set_UpdatedAt()
    {
        var group = MakeGroup();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        group.Update("New Name", null, null, 10, ActorId);

        group.UpdatedAt.Should().NotBeNull();
        group.UpdatedAt!.Value.Should().BeAfter(before);
    }

    [Fact]
    public void Update_Should_Normalize_RegionCode_To_Uppercase()
    {
        var group = MakeGroup();

        group.Update("Name", "us", null, 10, ActorId);

        group.RegionCode.Should().Be("US");
    }

    // ── Update – Validation Failures ─────────────────────────────────────────

    [Fact]
    public void Update_Should_Throw_When_Name_IsEmpty()
    {
        var group = MakeGroup();

        var act = () => group.Update(string.Empty, null, null, 10, ActorId);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Group name required*");
    }

    [Fact]
    public void Update_Should_Throw_When_AlertMinutes_Is_Zero()
    {
        var group = MakeGroup();

        var act = () => group.Update("Name", null, null, 0, ActorId);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*UnattendedAlertMinutes must be >= 1*");
    }

    // ── Update – Business Rule Violations ────────────────────────────────────

    [Fact]
    public void Update_Should_Throw_When_Group_Is_Deleted()
    {
        var group = MakeGroup();
        group.SoftDelete(ActorId);

        var act = () => group.Update("Name", null, null, 10, ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot modify a deleted group*");
    }

    // ── SoftDelete – Happy Path ───────────────────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Set_IsDeleted_True()
    {
        var group = MakeGroup();

        group.SoftDelete(ActorId);

        group.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_Should_Set_IsActive_False()
    {
        var group = MakeGroup();

        group.SoftDelete(ActorId);

        group.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedBy()
    {
        var group = MakeGroup();
        var actorId = Guid.NewGuid();

        group.SoftDelete(actorId);

        group.UpdatedBy.Should().Be(actorId);
    }

    // ── SoftDelete – Business Rule Violations ─────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Throw_When_Already_Deleted()
    {
        var group = MakeGroup();
        group.SoftDelete(ActorId);

        var act = () => group.SoftDelete(ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Group already deleted*");
    }
}
