# RabbitMQ Messaging Architecture

## Overview
In the Adrenalin platform, **RabbitMQ** is used as the primary message broker to facilitate an event-driven architecture. By using an event bus, different modules of the system (Ticketing, SLA, Notifications, Auth, etc.) can remain decoupled, highly scalable, and reliable. 

## Why We Use RabbitMQ

1. **Decoupling of Micro-Modules**: 
   Adrenalin is built as a modular monolith (or microservices). When a ticket is created in the Ticketing module, the SLA module needs to attach policies, the Notification module needs to send emails, and the Gamification module might need to award points. Without RabbitMQ, the Ticketing module would need direct references to all these modules, leading to tight coupling. With RabbitMQ, the Ticketing module simply publishes a `TicketCreatedIntegrationEvent`, and any interested module can subscribe to it independently.

2. **Asynchronous Processing**:
   Tasks like sending emails, calculating complex SLAs, or syncing with third-party systems can be time-consuming. By offloading these tasks to RabbitMQ, the main web thread is freed up immediately, resulting in blazing fast API response times for the frontend.

3. **Reliability and Fault Tolerance**:
   If the SMTP server is temporarily down, or an external API is unreachable, synchronous code would fail and the user request would error out. With RabbitMQ, messages can remain in the queue or be retried automatically until the downstream service is healthy again.

4. **Scalability**:
   Heavy workloads (like processing thousands of incoming support emails) can be distributed across multiple background consumer instances. As the system grows, we can simply spin up more consumer workers to process the RabbitMQ queues in parallel.

## The Outbox Pattern

To guarantee that events are not lost if the application crashes immediately after saving to the database but before sending the event to RabbitMQ, we use the **Transactional Outbox Pattern**.

1. **Database Transaction**: When a business action occurs (e.g., updating a ticket), the domain changes AND an integration event (e.g., `TicketStatusChangedEvent`) are saved to the database in the same transaction. The event is stored in the `outbox_messages` table.
2. **Outbox Processor**: A background worker (`OutboxProcessorBackgroundService`) continuously polls the `outbox_messages` table for unprocessed events.
3. **Publish to RabbitMQ**: The outbox processor reads the event and pushes it to the RabbitMQ exchange. Once RabbitMQ acknowledges the message, the processor marks the message as `processed_on` in the database.

This guarantees **At-Least-Once delivery** of events, completely eliminating the dual-write problem.

## Implementation Details

- **EventBus Abstraction**: Located in the `Adrenalin.EventBus` project. It defines the `IEventBus` interface and the specific `RabbitMQEventBus` implementation.
- **Consumer Service**: `RabbitMQConsumerService` is a Hosted Service that connects to RabbitMQ on startup, binds the necessary queues to the exchange, and listens for incoming messages, dispatching them to the appropriate MediatR handlers.
- **Routing**: Events are published to a central exchange using their Type Name (e.g., `EmailReceivedIntegrationEvent`) as the Routing Key. Subscribers bind their queues using these routing keys to receive only the events they care about.

## Key Event Flows

1. **Email Ingestion to Ticket Creation**:
   - `EmailIngestionBackgroundService` polls IMAP/Graph APIs.
   - Saves an `EmailReceivedIntegrationEvent` to the outbox.
   - Outbox processor pushes it to RabbitMQ.
   - `RabbitMQConsumerService` picks it up and routes it to `CreateTicketFromEmailCommandHandler` to generate a ticket.

2. **Ticket Status Changes**:
   - Agent resolves a ticket via HTTP API.
   - API saves the update and a `TicketResolvedIntegrationEvent` to the outbox.
   - Outbox processor pushes to RabbitMQ.
   - SLA module consumes it to stop the SLA timer.
   - Notification module consumes it to send a resolution email to the customer.

## Configuration

RabbitMQ settings are managed in `appsettings.json`:
```json
"RabbitMQ": {
  "Enabled": true,
  "HostName": "localhost",
  "UserName": "guest",
  "Password": "guest"
}
```
If `Enabled` is set to `false`, the system can fallback to an in-memory event bus (useful for simple local development or specific testing scenarios).
