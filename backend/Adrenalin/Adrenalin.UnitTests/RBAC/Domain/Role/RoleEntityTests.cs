using Adrenalin.Modules.Auth.Domain.Entities;
using FluentAssertions;

namespace Adrenalin.UnitTests.RBAC.Domain.Role;

public sealed class RoleEntityTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static readonly Guid ActorId = Guid.NewGuid();

    private static Adrenalin.Modules.Auth.Domain.Entities.Role MakeRole(
        string name = "Support Agent",
        string? description = "Handles tickets",
        Guid? createdBy = null)
        => Adrenalin.Modules.Auth.Domain.Entities.Role.Create(name, description, createdBy ?? ActorId);

    // ── Role.Create – Happy Path ──────────────────────────────────────────────

    [Fact]
    public void Create_Should_Create_Role_When_Data_Is_Valid()
    {
        var role = MakeRole("Support Agent", "Handles tickets");

        role.Should().NotBeNull();
        role.Name.Should().Be("Support Agent");
        role.Description.Should().Be("Handles tickets");
    }

    [Fact]
    public void Create_Should_Set_Id_To_NonEmpty_Guid()
    {
        var role = MakeRole();

        role.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_Should_Set_IsSystemRole_To_False()
    {
        var role = MakeRole();

        role.IsSystemRole.Should().BeFalse();
    }

    [Fact]
    public void Create_Should_Set_IsDeleted_To_False()
    {
        var role = MakeRole();

        role.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_Should_Set_CreatedBy()
    {
        var actorId = Guid.NewGuid();
        var role = MakeRole(createdBy: actorId);

        role.CreatedBy.Should().Be(actorId);
    }

    [Fact]
    public void Create_Should_Set_CreatedAt_To_Recent_Utc()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        var role = MakeRole();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        role.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void Create_Should_Trim_Name_Whitespace()
    {
        var role = MakeRole("  Support  ");

        role.Name.Should().Be("Support");
    }

    [Fact]
    public void Create_Should_Trim_Description_Whitespace()
    {
        var role = MakeRole(description: "  desc  ");

        role.Description.Should().Be("desc");
    }

    [Fact]
    public void Create_Should_Allow_Null_Description()
    {
        var role = MakeRole(description: null);

        role.Description.Should().BeNull();
    }

    [Fact]
    public void Create_Should_Initialize_Empty_UserRoles_Collection()
    {
        var role = MakeRole();

        role.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void Create_Should_Initialize_Empty_RolePermissions_Collection()
    {
        var role = MakeRole();

        role.RolePermissions.Should().BeEmpty();
    }

    // ── Role.Create – Validation Failures ─────────────────────────────────────

    [Fact]
    public void Create_Should_ThrowException_When_Name_IsEmpty()
    {
        var act = () => MakeRole(string.Empty);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Role name is required*");
    }

    [Fact]
    public void Create_Should_ThrowException_When_Name_IsWhitespace()
    {
        var act = () => MakeRole("   ");

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Role name is required*");
    }

    // ── Role.Update – Happy Path ──────────────────────────────────────────────

    [Fact]
    public void Update_Should_Update_Name_And_Description()
    {
        var role = MakeRole();
        var updatedBy = Guid.NewGuid();

        role.Update("New Name", "New Description", updatedBy);

        role.Name.Should().Be("New Name");
        role.Description.Should().Be("New Description");
    }

    [Fact]
    public void Update_Should_Set_UpdatedBy()
    {
        var role = MakeRole();
        var updatedBy = Guid.NewGuid();

        role.Update("New Name", null, updatedBy);

        role.UpdatedBy.Should().Be(updatedBy);
    }

    [Fact]
    public void Update_Should_Set_UpdatedAt_To_Recent_Utc()
    {
        var role = MakeRole();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        role.Update("New Name", null, ActorId);

        role.UpdatedAt.Should().NotBeNull();
        role.UpdatedAt!.Value.Should().BeAfter(before);
    }

    [Fact]
    public void Update_Should_Trim_Name()
    {
        var role = MakeRole();

        role.Update("  Trimmed  ", null, ActorId);

        role.Name.Should().Be("Trimmed");
    }

    [Fact]
    public void Update_Should_Trim_Description()
    {
        var role = MakeRole();

        role.Update("Name", "  desc  ", ActorId);

        role.Description.Should().Be("desc");
    }

    [Fact]
    public void Update_Should_Allow_Null_Description()
    {
        var role = MakeRole(description: "old desc");

        role.Update("Name", null, ActorId);

        role.Description.Should().BeNull();
    }

    // ── Role.Update – Validation Failures ────────────────────────────────────

    [Fact]
    public void Update_Should_ThrowException_When_Name_IsEmpty()
    {
        var role = MakeRole();

        var act = () => role.Update(string.Empty, null, ActorId);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Role name is required*");
    }

    [Fact]
    public void Update_Should_ThrowException_When_Name_IsWhitespace()
    {
        var role = MakeRole();

        var act = () => role.Update("   ", null, ActorId);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*Role name is required*");
    }

    // ── Role.Update – Business Rule Violations ────────────────────────────────

    [Fact]
    public void Update_Should_Throw_When_Role_Is_Deleted()
    {
        var role = MakeRole();
        role.SoftDelete(ActorId);

        var act = () => role.Update("New Name", null, ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Cannot modify a deleted role*");
    }

    // ── Role.SoftDelete – Happy Path ──────────────────────────────────────────

    [Fact]
    public void SoftDelete_Should_Set_IsDeleted_To_True()
    {
        var role = MakeRole();

        role.SoftDelete(ActorId);

        role.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedBy()
    {
        var role = MakeRole();
        var actorId = Guid.NewGuid();

        role.SoftDelete(actorId);

        role.UpdatedBy.Should().Be(actorId);
    }

    [Fact]
    public void SoftDelete_Should_Set_UpdatedAt()
    {
        var role = MakeRole();
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);

        role.SoftDelete(ActorId);

        role.UpdatedAt.Should().NotBeNull();
        role.UpdatedAt!.Value.Should().BeAfter(before);
    }

    // ── Role.SoftDelete – Business Rule Violations ────────────────────────────

    [Fact]
    public void DeleteRole_Should_Throw_When_Role_IsSystemRole()
    {
        // Arrange: use reflection to set IsSystemRole = true (private setter)
        var role = MakeRole();
        typeof(Adrenalin.Modules.Auth.Domain.Entities.Role)
            .GetProperty(nameof(Adrenalin.Modules.Auth.Domain.Entities.Role.IsSystemRole))!
            .SetValue(role, true);

        var act = () => role.SoftDelete(ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*System roles cannot be deleted*");
    }

    [Fact]
    public void SoftDelete_Should_Throw_When_Role_Already_Deleted()
    {
        var role = MakeRole();
        role.SoftDelete(ActorId);

        var act = () => role.SoftDelete(ActorId);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*already deleted*");
    }
}
