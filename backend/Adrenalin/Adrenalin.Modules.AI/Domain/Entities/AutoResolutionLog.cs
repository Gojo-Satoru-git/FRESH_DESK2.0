using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.AI.Domain.Entities;

public sealed class AutoResolutionLog : BaseEntity
{
    public Guid TicketId { get; private set; }
    public string Suggestion { get; private set; } = string.Empty;
    public bool Applied { get; private set; }
    public Guid? AppliedBy { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private AutoResolutionLog() { }

    private AutoResolutionLog(Guid ticketId, string suggestion, bool applied, Guid? appliedBy)
    {
        TicketId = ticketId;
        Suggestion = suggestion;
        Applied = applied;
        AppliedBy = appliedBy;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static AutoResolutionLog Create(Guid ticketId, string suggestion, bool applied, Guid? appliedBy)
    {
        return new AutoResolutionLog(ticketId, suggestion, applied, appliedBy);
    }
}