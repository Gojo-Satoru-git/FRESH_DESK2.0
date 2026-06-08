using Adrenalin.Modules.Auth.Domain.Entities;
using FluentAssertions;

namespace Adrenalin.UnitTests.RBAC.Domain.Permission;

public sealed class PermissionEntityTests
{
    private static readonly Guid ActorId = Guid.NewGuid();

    private static Adrenalin.Modules.Auth.Domain.Entities.Permission MakePermission(
        string resource = "ticket",
        string action = "read",
        string? description = null,
        Guid? createdBy = null)
        => Adrenalin.Modules.Auth.Domain.Entities.Permission.Create(resource, action, description, createdBy ?? ActorId);

    // ── Permission.Create – Happy Path ────────────────────────────────────────

    [Fact]
    public void Create_Should_Create_Permission_When_Data_Is_Valid()
    {
        var perm = MakePermission("ticket", "read");

        perm.Should().NotBeNull();
        perm.Resource.Should().Be("ticket");
        perm.Action.Should().Be("read");
    }

    [Fact]
    public void Create_Should_Normalize_Resource_To_Lowercase()
    {
        var perm = MakePermission("TICKET");

        perm.Resource.Should().Be("ticket");
    }

    [Fact]
    public void Create_Should_Normalize_Action_To_Lowercase()
    {
        var perm = MakePermission(action: "READ");

        perm.Action.Should().Be("read");
    }

    [Fact]
    public void Create_Should_Trim_Resource_Whitespace()
    {
        var perm = MakePermission("  ticket  ");

        perm.Resource.Should().Be("ticket");
    }

    [Fact]
    public void Create_Should_Trim_Action_Whitespace()
    {
        var perm = MakePermission(action: "  read  ");

        perm.Action.Should().Be("read");
    }

    [Fact]
    public void Create_Should_Set_Description()
    {
        var perm = MakePermission(description: "Read tickets");

        perm.Description.Should().Be("Read tickets");
    }

    [Fact]
    public void Create_Should_Set_IsDeleted_False()
    {
        var perm = MakePermission();

        perm.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_Should_Set_CreatedBy()
    {
        var actorId = Guid.NewGuid();
        var perm = MakePermission(createdBy: actorId);

        perm.CreatedBy.Should().Be(actorId);
    }

    [Fact]
    public void Create_Should_Set_CreatedAt_To_Recent_Utc()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var perm = MakePermission();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        perm.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void Create_Should_Set_NonEmpty_Id()
    {
        var perm = MakePermission();

        perm.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_Should_Initialize_Empty_RolePermissions_Collection()
    {
        var perm = MakePermission();

        perm.RolePermissions.Should().BeEmpty();
    }

    // ── Permission.Create – Validation Failures ───────────────────────────────

    [Fact]
    public void Create_Should_Throw_When_Resource_IsEmpty()
    {
        var act = () => MakePermission(resource: string.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Resource is required*");
    }

    [Fact]
    public void Create_Should_Throw_When_Resource_IsWhitespace()
    {
        var act = () => MakePermission(resource: "   ");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Resource is required*");
    }

    [Fact]
    public void Create_Should_Throw_When_Action_IsEmpty()
    {
        var act = () => MakePermission(action: string.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Action is required*");
    }

    [Fact]
    public void Create_Should_Throw_When_Action_IsWhitespace()
    {
        var act = () => MakePermission(action: "   ");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Action is required*");
    }

    // ── Permission.ToKey ──────────────────────────────────────────────────────

    [Fact]
    public void ToKey_Should_Return_Resource_Colon_Action()
    {
        var perm = MakePermission("ticket", "read");

        perm.ToKey().Should().Be("ticket:read");
    }

    [Fact]
    public void ToKey_Should_Return_Lowercase_Key()
    {
        var perm = MakePermission("TICKET", "READ");

        perm.ToKey().Should().Be("ticket:read");
    }

    // ── Permission.SoftDelete – Happy Path ───────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Set_IsDeleted_True()
    {
        var perm = MakePermission();

        perm.SoftDelete(ActorId);

        perm.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedBy()
    {
        var perm = MakePermission();
        var actorId = Guid.NewGuid();

        perm.SoftDelete(actorId);

        perm.UpdatedBy.Should().Be(actorId);
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedAt()
    {
        var perm = MakePermission();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        perm.SoftDelete(ActorId);

        perm.UpdatedAt.Should().NotBeNull();
        perm.UpdatedAt!.Value.Should().BeAfter(before);
    }

    // ── Permission.SoftDelete – Business Rule Violations ─────────────────────

    [Fact]
    public void SoftDelete_Should_Throw_When_Already_Deleted()
    {
        var perm = MakePermission();
        perm.SoftDelete(ActorId);

        var act = () => perm.SoftDelete(ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already deleted*");
    }
}
