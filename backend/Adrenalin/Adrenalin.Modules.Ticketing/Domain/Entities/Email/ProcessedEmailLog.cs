using Adrenalin.SharedKernel.Entities;

namespace Adrenalin.Modules.Ticketing.Domain.Entities.Email;

public sealed class ProcessedEmailLog : BaseEntity
{
    public string InternetMessageId { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public EmailProcessingState Status { get; set; } = EmailProcessingState.Pending;
    public string? FailureReason { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
