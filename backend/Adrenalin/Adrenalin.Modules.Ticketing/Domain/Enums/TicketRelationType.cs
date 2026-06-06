namespace Adrenalin.Modules.Ticketing.Domain.Enums;

public enum TicketRelationType
{
    Related = 1,
    Duplicate = 2,
    ParentChild = 3,
    BlockedBy = 4,
    DependsOn = 5,
    MergedInto = 6,
    SplitFrom = 7
}