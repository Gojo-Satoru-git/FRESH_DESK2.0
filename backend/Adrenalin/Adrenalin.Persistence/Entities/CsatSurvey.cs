using System;
using System.Collections.Generic;

namespace Adrenalin.Persistence.Entities;

public partial class CsatSurvey
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid? ContactId { get; set; }

    public int Rating { get; set; }

    public string? Feedback { get; set; }

    public DateTime SubmittedAt { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual Ticket Ticket { get; set; } = null!;
}
