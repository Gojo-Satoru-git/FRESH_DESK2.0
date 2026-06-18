using Adrenalin.Modules.Ticketing.Domain.Enums;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

/// <summary>
/// Defines a routing rule that maps ticket attributes for a company to a target group.
/// Rules are evaluated in <c>RulePriority</c> order (lower = evaluated first).
/// The first fully-matching rule wins. 
/// A rule with <c>IsDefault = true</c> is the fallback when no specific rules match.
/// </summary>
public sealed class CompanyRoutingRule : SoftDeleteEntity
{
    public Guid CompanyId { get; private set; }

    public Guid GroupId { get; private set; }

    public Guid? ModuleId { get; private set; }

    public string? RegionCode { get; private set; }

    public string? TierCode { get; private set; }

    public TicketPriority? Priority { get; private set; }

    public TicketType? TicketType { get; private set; }

    public string? Keywords { get; private set; }

    public int RulePriority { get; private set; }

    public bool IsDefault { get; private set; }

    private CompanyRoutingRule() { }

    public static CompanyRoutingRule Create(
        Guid companyId,
        Guid groupId,
        Guid? moduleId,
        string? regionCode,
        string? tierCode,
        TicketPriority? priority,
        TicketType? ticketType,
        string? keywords,
        int rulePriority,
        bool isDefault,
        Guid createdBy)
    {
        if (companyId == Guid.Empty) throw new ArgumentException("CompanyId is required.");
        if (groupId == Guid.Empty) throw new ArgumentException("GroupId is required.");
        if (rulePriority < 0) throw new ArgumentException("RulePriority must be >= 0.");

        return new CompanyRoutingRule
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            GroupId = groupId,
            ModuleId = moduleId,
            RegionCode = regionCode?.Trim().ToUpperInvariant(),
            TierCode = tierCode?.Trim().ToUpperInvariant(),
            Priority = priority,
            TicketType = ticketType,
            Keywords = keywords?.Trim(),
            RulePriority = rulePriority,
            IsDefault = isDefault,
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(
        Guid groupId,
        Guid? moduleId,
        string? regionCode,
        string? tierCode,
        TicketPriority? priority,
        TicketType? ticketType,
        string? keywords,
        int rulePriority,
        bool isDefault,
        Guid updatedBy)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot modify a deleted routing rule.");
        if (groupId == Guid.Empty) throw new ArgumentException("GroupId is required.");
        if (rulePriority < 0) throw new ArgumentException("RulePriority must be >= 0.");

        GroupId = groupId;
        ModuleId = moduleId;
        RegionCode = regionCode?.Trim().ToUpperInvariant();
        TierCode = tierCode?.Trim().ToUpperInvariant();
        Priority = priority;
        TicketType = ticketType;
        Keywords = keywords?.Trim();
        RulePriority = rulePriority;
        IsDefault = isDefault;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("Routing rule already deleted.");
        IsDeleted = true;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
