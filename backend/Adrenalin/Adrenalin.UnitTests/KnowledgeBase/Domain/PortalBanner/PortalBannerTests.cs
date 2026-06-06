using Adrenalin.Modules.KB.Domain.Entities;

namespace Adrenalin.UnitTests.KnowledgeBase.Domain.PortalBannerTests;

public sealed class PortalBannerTests
{
    private static DateTimeOffset Now => DateTimeOffset.UtcNow;

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_WithValidArgs_SetsProperties()
    {
        var actorId = Guid.NewGuid();
        var from = Now.AddHours(1);
        var to = Now.AddHours(5);

        var banner = PortalBanner.Create("Maintenance", "System down at midnight.", from, to, actorId);

        Assert.Equal("Maintenance", banner.Title);
        Assert.Equal("System down at midnight.", banner.Message);
        Assert.Equal(from, banner.ActiveFrom);
        Assert.Equal(to, banner.ActiveTo);
        Assert.True(banner.IsActive);          // default is active
        Assert.Equal(actorId, banner.CreatedBy);
        Assert.NotEqual(Guid.Empty, banner.Id);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var banner = PortalBanner.Create("  Title  ", "  Message  ", null, null, null);

        Assert.Equal("Title", banner.Title);
        Assert.Equal("Message", banner.Message);
    }

    [Fact]
    public void Create_NoSchedule_Succeeds()
    {
        var banner = PortalBanner.Create("T", "M", null, null, null);
        Assert.Null(banner.ActiveFrom);
        Assert.Null(banner.ActiveTo);
    }

    [Fact]
    public void Create_ActiveToBeforeActiveFrom_Throws()
    {
        var from = Now.AddHours(5);
        var to = Now.AddHours(1);

        Assert.Throws<ArgumentException>(() =>
            PortalBanner.Create("T", "M", from, to, null));
    }

    [Fact]
    public void Create_ActiveToEqualToActiveFrom_Throws()
    {
        var ts = Now.AddHours(3);
        Assert.Throws<ArgumentException>(() =>
            PortalBanner.Create("T", "M", ts, ts, null));
    }

    [Fact]
    public void Create_BlankTitle_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PortalBanner.Create("   ", "Message", null, null, null));
    }

    [Fact]
    public void Create_TitleExceeds200Chars_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PortalBanner.Create(new string('T', 201), "M", null, null, null));
    }

    [Fact]
    public void Create_BlankMessage_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            PortalBanner.Create("Title", "  ", null, null, null));
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ChangesAllFields()
    {
        var actor = Guid.NewGuid();
        var banner = PortalBanner.Create("Old", "Old message", null, null, actor);
        var newFrom = Now.AddHours(2);
        var newTo = Now.AddHours(6);

        banner.Update("New Title", "New message", newFrom, newTo, actor);

        Assert.Equal("New Title", banner.Title);
        Assert.Equal("New message", banner.Message);
        Assert.Equal(newFrom, banner.ActiveFrom);
        Assert.Equal(newTo, banner.ActiveTo);
        Assert.Equal(actor, banner.UpdatedBy);
        Assert.NotNull(banner.UpdatedAt);
    }

    [Fact]
    public void Update_InvalidSchedule_Throws()
    {
        var banner = PortalBanner.Create("T", "M", null, null, null);

        Assert.Throws<ArgumentException>((Action)(() =>
            banner.Update("T", "M", Now.AddHours(5), Now.AddHours(1), null)));
    }

    // ── Activate / Deactivate ─────────────────────────────────────────────────

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var actor = Guid.NewGuid();
        var banner = PortalBanner.Create("T", "M", null, null, actor);

        banner.Deactivate(actor);

        Assert.False(banner.IsActive);
        Assert.Equal(actor, banner.UpdatedBy);
        Assert.NotNull(banner.UpdatedAt);
    }

    [Fact]
    public void Activate_SetsIsActiveTrue()
    {
        var actor = Guid.NewGuid();
        var banner = PortalBanner.Create("T", "M", null, null, actor);
        banner.Deactivate(actor);

        banner.Activate(actor);

        Assert.True(banner.IsActive);
    }

    // ── IsCurrentlyVisible ────────────────────────────────────────────────────

    [Fact]
    public void IsCurrentlyVisible_Active_NoSchedule_ReturnsTrue()
    {
        var banner = PortalBanner.Create("T", "M", null, null, null);
        Assert.True(banner.IsCurrentlyVisible(Now));
    }

    [Fact]
    public void IsCurrentlyVisible_Inactive_ReturnsFalse()
    {
        var banner = PortalBanner.Create("T", "M", null, null, null);
        banner.Deactivate(null);

        Assert.False(banner.IsCurrentlyVisible(Now));
    }

    [Fact]
    public void IsCurrentlyVisible_BeforeActiveFrom_ReturnsFalse()
    {
        var from = Now.AddHours(2);
        var to = Now.AddHours(6);
        var banner = PortalBanner.Create("T", "M", from, to, null);

        Assert.False(banner.IsCurrentlyVisible(Now));
    }

    [Fact]
    public void IsCurrentlyVisible_AfterActiveTo_ReturnsFalse()
    {
        var from = Now.AddHours(-5);
        var to = Now.AddHours(-1);
        var banner = PortalBanner.Create("T", "M", from, to, null);

        Assert.False(banner.IsCurrentlyVisible(Now));
    }

    [Fact]
    public void IsCurrentlyVisible_WithinWindow_ReturnsTrue()
    {
        var from = Now.AddHours(-1);
        var to = Now.AddHours(1);
        var banner = PortalBanner.Create("T", "M", from, to, null);

        Assert.True(banner.IsCurrentlyVisible(Now));
    }

    [Fact]
    public void IsCurrentlyVisible_ActiveFromOnlyAndNowAfterFrom_ReturnsTrue()
    {
        var banner = PortalBanner.Create("T", "M", Now.AddHours(-1), null, null);
        Assert.True(banner.IsCurrentlyVisible(Now));
    }

    [Fact]
    public void IsCurrentlyVisible_ActiveToOnlyAndNowBeforeTo_ReturnsTrue()
    {
        var banner = PortalBanner.Create("T", "M", null, Now.AddHours(1), null);
        Assert.True(banner.IsCurrentlyVisible(Now));
    }
}