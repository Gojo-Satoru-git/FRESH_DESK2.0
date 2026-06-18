# Notification Module Documentation

**Project:** Adrenalin (FRESH_DESK 2.0 backend)  
**Namespace:** `Adrenalin.Modules.Notification`

---

## Overview

The Notification Module is responsible for tracking, generating, and logging notifications in response to system events. Rather than exposing commands for direct invocation, this module listens to integration events published by other modules (such as Ticketing and SLA) and dispatches both in-app logs and email alerts.

### Key Responsibilities
- Subscribing to domain integration events (Ticket Created, Assigned, Resolved, Closed, Comment Added, SLA Breached).
- Retrieving context-specific HTML and text templates from the database.
- Replacing dynamic template variables (e.g., `{{ticket.id}}`, `{{ticket.url}}`, `{{ticket.agent.name}}`).
- Resolving recipient email addresses from IDs using the `INotificationRepository`.
- Saving a persistent `NotificationLog` record for historical tracking and in-app display.
- Invoking the `IEmailService` (from SharedKernel) to push actual emails out to end-users and agents.

---

## Architecture

The Notification module follows an event-driven architecture, relying heavily on the Mediator pattern and the application's central `IEventBus`.

```
[Other Modules] -> (Publish Event) -> [EventBus] 
                                          |
                                          v
                    [Adrenalin.Modules.Notification Handlers]
                                          |
        +---------------------------------+---------------------------------+
        |                                                                   |
        v                                                                   v
[INotificationRepository]                                           [IEmailService]
 - Fetch template                                                    - Dispatch email via SMTP/Console
 - Resolve recipient emails
 - Save NotificationLog to database
```

### Dependency Injection

The module depends on:
1. `INotificationRepository`: Resolves users, fetching templates, saving logs.
2. `IIntegrationEventLogRepository`: Tracks event idempotency (preventing duplicate notifications).
3. `IEmailService`: Dispatches generated notifications to the end user.

---

## Event Handlers

The module provides handlers located in `Application/IntegrationEvents`.

### 1. Ticket Events (`TicketNotificationHandlers.cs`)

| Event Handler | Template Code Used | Recipients | Description |
|---|---|---|---|
| `TicketCreatedNotificationHandler` | `TICKET_CREATED_REQUESTER` | Requester | Acknowledges ticket creation to the customer. |
| | `TICKET_ASSIGNED_AGENT` | Assigned Agent | Informs the agent they have been assigned if done at creation. |
| | `TICKET_CREATED_REQUESTER` (reused) | Team Leads | Informs supervisors that a new ticket entered the system. |
| `TicketAssignedNotificationHandler` | `TICKET_ASSIGNED_AGENT` | Assigned Agent | Alerts an agent when a ticket is assigned to them post-creation. |
| `TicketGroupAssignedNotificationHandler`| `TICKET_ASSIGNED_GROUP` | Team Leads | Alerts supervisors that a ticket was assigned to a group/bucket. |
| `TicketResolvedNotificationHandler` | `TICKET_RESOLVED_REQUESTER` | Requester | Alerts the customer that the agent has proposed a resolution. |
| `TicketClosedNotificationHandler` | `TICKET_CLOSED_REQUESTER` | Requester | Alerts the customer that the ticket has been finalized and closed. |
| `TicketCommentAddedNotificationHandler` | `AGENT_ADDED_COMMENT` / `REQUESTER_REPLIED` / `NOTE_ADDED_TO_TICKET` | Requester or Agent | Dispatches updates when new comments or internal notes are added. |

### 2. SLA Events (`SlaNotificationHandler.cs`)

| Event Handler | Template Code Used | Recipients | Description |
|---|---|---|---|
| `SlaNotificationHandler` | `SLA_BREACH_ALERT` | Targets specified by the SLA Escalation Rule | Alerts management/agents when a ticket breaches First Response or Resolution SLA timeframes. |

---

## The Notification Pipeline

When an integration event fires, the generic pipeline executed by the handler is:

1. **Idempotency Check:** Verify with `IIntegrationEventLogRepository` if the `EventId` has already been processed. If yes, skip.
2. **Template Retrieval:** Fetch the HTML template using `_repository.GetTemplateByCodeAsync("TEMPLATE_CODE")`.
3. **Variable Replacement:** String manipulation is used to replace placeholders (like `{{ticket.priority}}`) with event data.
4. **Recipient Resolution:** Query the repository to resolve raw User IDs into actual email addresses (`_repository.ResolveEmailAsync`).
5. **Database Logging:** Construct a `NotificationLog` entity and save it via `_repository.AddLogAsync`. This powers the "In-App Notifications" bell icon.
6. **Email Dispatch:** Pass the generated Subject, Body, and Recipient Email to `_emailService.SendAsync()`.

---

## Data Models

### `NotificationTemplate`
Stores the boilerplate subject lines and HTML bodies for notifications. Seeded during database initialization.

### `NotificationLog`
A persistent log of a sent notification.

```csharp
public class NotificationLog : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid TicketId { get; set; }
    public string? TicketNumber { get; set; }
    public Guid TemplateId { get; set; }
    public NotificationTemplate Template { get; set; } = null!;
    
    public string RecipientEmail { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public bool IsFailedDelivery { get; set; }
    public DateTime SentAt { get; set; }
}
```

---

## Recent Updates

- **Dual-Delivery Integration:** The Notification module now fully supports the `IEmailService` architecture. Previously, notifications were only logged to the database. They are now actively pushed to the `CompositeEmailService`, enabling real-time Console/SMTP output depending on `appsettings.json` configuration.
