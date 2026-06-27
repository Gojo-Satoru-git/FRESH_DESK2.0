using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Auth.Domain.Entities;

public sealed class Group : ActiveSoftDeleteEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? RegionCode { get; private set; }
    public string? TierCode { get; private set; }
    public int UnattendedAlertMinutes { get; private set; }

    /// <summary>
    /// The auto-assignment strategy used when tickets are routed to this group.
    /// Stored as integer in DB. Default: 0 (LeastLoaded).
    /// Extensible — teammates can add new strategies by implementing IAgentAssignmentStrategy.
    /// 4 = FactorBased (role+hard-filter+scoring) is intentionally stubbed — see
    /// FactorBasedAssignmentStrategy. Setting a group to it means "queue everything
    /// for manual dispatch until the Workflow module ships factor data".
    /// </summary>
    public int AssignmentStrategy { get; private set; }

    /// <summary>
    /// Optional escalation target. When this group's queue overflows or an escalation rule fires,
    /// tickets can be rerouted to this fallback group.
    /// </summary>
    public Guid? FallbackGroupId { get; private set; }

    /// <summary>
    /// Which ticket types (TicketType enum names, e.g. "Bug", "Incident") this group is
    /// scoped to handle. Stored as plain strings rather than a reference to
    /// Adrenalin.Modules.Ticketing.Domain.Enums.TicketType deliberately — the Auth module's
    /// Domain layer must have zero dependency on the Ticketing module (see folder-structure.md
    /// module boundary rules). The Ticketing module is the one that knows what these strings mean
    /// and validates/parses them.
    ///
    /// Empty list = no restriction (group accepts any ticket type) — this preserves existing
    /// behaviour for every group that already exists in the DB.
    /// </summary>
    public List<string> SupportedTicketTypes { get; private set; } = [];

    public ICollection<UserGroup> UserGroups { get; private set; } = [];

    private Group() { }

    public static Group Create(string name, string? regionCode, string? tierCode,
        int unattendedAlertMinutes, Guid createdBy,
        int assignmentStrategy = 0, Guid? fallbackGroupId = null,
        IEnumerable<string>? supportedTicketTypes = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name required.");
        if (unattendedAlertMinutes < 1) throw new ArgumentException("UnattendedAlertMinutes must be >= 1.");
        return new Group
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            RegionCode = regionCode?.Trim().ToUpperInvariant(),
            TierCode = tierCode?.Trim().ToUpperInvariant(),
            UnattendedAlertMinutes = unattendedAlertMinutes,
            AssignmentStrategy = assignmentStrategy,
            FallbackGroupId = fallbackGroupId,
            SupportedTicketTypes = NormalizeTicketTypes(supportedTicketTypes),
            IsActive = true,
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Update(string name, string? regionCode, string? tierCode,
        int unattendedAlertMinutes, Guid updatedBy,
        int? assignmentStrategy = null, Guid? fallbackGroupId = null,
        IEnumerable<string>? supportedTicketTypes = null)
    {
        if (IsDeleted) throw new InvalidOperationException("Cannot modify a deleted group.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name required.");
        if (unattendedAlertMinutes < 1) throw new ArgumentException("UnattendedAlertMinutes must be >= 1.");
        Name = name.Trim();
        RegionCode = regionCode?.Trim().ToUpperInvariant();
        TierCode = tierCode?.Trim().ToUpperInvariant();
        UnattendedAlertMinutes = unattendedAlertMinutes;
        if (assignmentStrategy.HasValue)
            AssignmentStrategy = assignmentStrategy.Value;
        FallbackGroupId = fallbackGroupId;
        if (supportedTicketTypes is not null)
            SupportedTicketTypes = NormalizeTicketTypes(supportedTicketTypes);
        UpdatedBy = updatedBy;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// True if this group accepts the given ticket type — either because it has no
    /// restriction configured (empty list = accepts anything) or because the type
    /// is explicitly in its supported list. Comparison is case-insensitive since
    /// values are persisted as plain strings (e.g. "Bug", "Incident").
    /// </summary>
    public bool SupportsTicketType(string ticketType)
    {
        if (SupportedTicketTypes.Count == 0) return true;
        return SupportedTicketTypes.Any(t => string.Equals(t, ticketType, StringComparison.OrdinalIgnoreCase));
    }

    public void SoftDelete(Guid actorId)
    {
        if (IsDeleted) throw new InvalidOperationException("Group already deleted.");
        IsDeleted = true;
        IsActive = false;
        UpdatedBy = actorId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static List<string> NormalizeTicketTypes(IEnumerable<string>? types)
        => types?
               .Where(t => !string.IsNullOrWhiteSpace(t))
               .Select(t => t.Trim())
               .Distinct(StringComparer.OrdinalIgnoreCase)
               .ToList()
           ?? [];
}
