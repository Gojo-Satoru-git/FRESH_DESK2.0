using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.KB.Domain.Entities;

/// <summary>
/// Time-boxed banner on the customer portal. Table: kb.portal_banners
/// Schema columns: id, title, message, active_from, active_to, is_active,
///                 created_by, updated_by, created_at, updated_at
/// Note: No is_deleted or row_version in schema.
/// IsActive = false is the manual kill switch (overrides schedule).
/// </summary>
public sealed class PortalBanner : BaseEntity
{
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public DateTimeOffset? ActiveFrom { get; private set; }
    public DateTimeOffset? ActiveTo { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private PortalBanner() { }

    public static PortalBanner Create(string title, string message,
        DateTimeOffset? activeFrom, DateTimeOffset? activeTo, Guid? createdBy)
    {
        ValidateSchedule(activeFrom, activeTo);
        ValidateTitle(title);
        ValidateMessage(message);
        return new PortalBanner
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            Message = message.Trim(),
            ActiveFrom = activeFrom,
            ActiveTo = activeTo,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string title, string message, DateTimeOffset? activeFrom,
        DateTimeOffset? activeTo, Guid? updatedBy)
    {
        ValidateSchedule(activeFrom, activeTo);
        ValidateTitle(title);
        ValidateMessage(message);
        Title = title.Trim();
        Message = message.Trim();
        ActiveFrom = activeFrom;
        ActiveTo = activeTo;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Activate(Guid? updatedBy) { IsActive = true; UpdatedBy = updatedBy; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Deactivate(Guid? updatedBy) { IsActive = false; UpdatedBy = updatedBy; UpdatedAt = DateTimeOffset.UtcNow; }

    /// <summary>True when banner should show: IsActive=true AND within schedule window.</summary>
    public bool IsCurrentlyVisible(DateTimeOffset now)
    {
        if (!IsActive) return false;
        if (ActiveFrom.HasValue && now < ActiveFrom.Value) return false;
        if (ActiveTo.HasValue && now > ActiveTo.Value) return false;
        return true;
    }

    private static void ValidateSchedule(DateTimeOffset? from, DateTimeOffset? to)
    {
        if (from.HasValue && to.HasValue && to.Value <= from.Value)
            throw new ArgumentException("active_to must be after active_from.");
    }

    private static void ValidateTitle(string t)
    {
        if (string.IsNullOrWhiteSpace(t)) throw new ArgumentException("Banner title cannot be blank.");
        if (t.Length > 200) throw new ArgumentException("Banner title cannot exceed 200 characters.");
    }

    private static void ValidateMessage(string m)
    {
        if (string.IsNullOrWhiteSpace(m)) throw new ArgumentException("Banner message cannot be blank.");
    }
}
