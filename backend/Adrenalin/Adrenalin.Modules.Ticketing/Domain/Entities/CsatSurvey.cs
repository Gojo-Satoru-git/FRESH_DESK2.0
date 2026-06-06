using System;
using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities;

public sealed class CsatSurvey : BaseEntity
{
    public Guid TicketId { get; private set; }

    public Guid? ContactId { get; private set; }

    public int Rating { get; private set; }

    public string? Feedback { get; private set; }

    public DateTime SubmittedAt { get; private set; }

    public Ticket Ticket { get; private set; } = null!;
}
