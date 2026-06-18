using System.Diagnostics.Metrics;

namespace Adrenalin.Modules.Ticketing.Application.Services;

public static class EmailMetrics
{
    public static readonly Meter Meter = new("Adrenalin.Ticketing.Email", "1.0.0");

    public static readonly Counter<long> EmailsReceived = Meter.CreateCounter<long>(
        "email_received_total", 
        description: "Total number of inbound emails received");

    public static readonly Counter<long> EmailsIgnored = Meter.CreateCounter<long>(
        "email_ignored_total", 
        description: "Total number of inbound emails ignored (e.g. unresolved company)");

    public static readonly Counter<long> EmailsDeduplicated = Meter.CreateCounter<long>(
        "email_deduplicated_total", 
        description: "Total number of inbound emails deduplicated");

    public static readonly Counter<long> EmailsFailed = Meter.CreateCounter<long>(
        "email_failed_total", 
        description: "Total number of inbound emails that failed processing");

    public static readonly Counter<long> TicketsCreated = Meter.CreateCounter<long>(
        "email_tickets_created_total", 
        description: "Total number of tickets created from email");

    public static readonly Counter<long> RepliesAdded = Meter.CreateCounter<long>(
        "email_replies_added_total", 
        description: "Total number of replies added to tickets from email");

    public static readonly Counter<long> AutoContactsCreated = Meter.CreateCounter<long>(
        "email_auto_contacts_created_total", 
        description: "Total number of contacts auto-created from email");

    public static readonly Counter<long> AttachmentsStored = Meter.CreateCounter<long>(
        "email_attachments_stored_total", 
        description: "Total number of email attachments physically stored");

    public static readonly Counter<long> AttachmentsRejected = Meter.CreateCounter<long>(
        "email_attachments_rejected_total", 
        description: "Total number of email attachments rejected (e.g. size/extension limit)");
}
