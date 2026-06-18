# Persistence Architecture & Repository Structure

**Project:** Adrenalin (FRESH_DESK 2.0 backend)  
**Namespace:** `Adrenalin.Persistence`

---

## Overview

The `Adrenalin.Persistence` layer is responsible for all database interactions. It implements the Repository interfaces defined in the core Domain modules and handles EF Core Database Contexts, Migrations, and Entity Configurations.

Recent architectural improvements have reorganized the `Repositories` to strictly align with our **Modular Monolith / Domain-Driven Design (DDD)** approach. Instead of a flat, messy directory containing dozens of repositories, the persistence layer now cleanly mirrors the domain feature modules.

---

## Folder Structure

The `Adrenalin.Persistence/Repositories/` directory is logically divided into sub-namespaces, directly mapping to the `Adrenalin.Modules.[Feature]` projects:

```text
Adrenalin.Persistence/
│
├── Context/                 <-- Contains AdrenalinDbContext
├── Migrations/              <-- EF Core Migrations
├── EntityConfigurations/    <-- EF Core Fluent API Maps (IEntityTypeConfiguration)
│
└── Repositories/
    ├── Auth/                <-- Maps to Adrenalin.Modules.Auth
    │   ├── UserRepository.cs
    │   ├── RoleRepository.cs
    │   └── ...
    │
    ├── Company/             <-- Maps to Adrenalin.Modules.Company
    │   ├── CompanyRepository.cs
    │   ├── ContactRepository.cs
    │   └── ...
    │
    ├── Ticketing/           <-- Maps to Adrenalin.Modules.Ticketing
    │   ├── TicketRepository.cs
    │   ├── TicketCommentRepository.cs
    │   └── ...
    │
    ├── SLA/                 <-- Maps to Adrenalin.Modules.SLA
    │   ├── SlaRepository.cs
    │   └── ...
    │
    ├── Notification/        <-- Maps to Adrenalin.Modules.Notification
    │   ├── NotificationRepository.cs
    │   └── ...
    │
    ├── Lookup/              <-- Maps to Adrenalin.Modules.Lookup
    │   ├── LookupRepository.cs
    │   └── ...
    │
    ├── KnowledgeBase/       <-- Maps to Adrenalin.Modules.KnowledgeBase
    │   └── KbRepository.cs
    │
    ├── Workflow/            <-- Maps to Adrenalin.Modules.Workflow
    │   └── WorkflowRepository.cs
    │
    └── SharedKernel/        <-- Core or Cross-cutting Repositories
        ├── IntegrationEventLogRepository.cs
        └── ...
```

---

## Design Principles

1. **Strict Mapping to Domain:** A repository implementation inside `Persistence/Repositories/[Module]/` MUST implement an interface located in `Modules.[Module]/Domain/Interfaces/`. 
2. **Encapsulation:** The rest of the application never instantiates these repositories directly. They depend solely on the abstract domain interfaces, and the Dependency Injection container maps them at runtime.
3. **Unit of Work:** Repositories generally do NOT call `SaveChangesAsync()`. Business operations mutate the entity, the repository tracks the entity, and the central `UnitOfWorkBehavior` (in the MediatR pipeline) commits the transaction at the end of the request.

---

## Dependency Injection Registration

All repositories are registered in the DI container via the `AddPersistence()` extension method located in `Adrenalin.Persistence/DependencyInjection.cs`.

When adding a new repository:
1. Define `IYourRepository` in the Domain layer (`Adrenalin.Modules.YourModule.Domain.Interfaces`).
2. Create `YourRepository.cs` in `Adrenalin.Persistence.Repositories.YourModule`.
3. Add `services.AddScoped<IYourRepository, YourRepository>();` to `DependencyInjection.cs`.

---

## Best Practices

- **Never Leak Persistence logic:** Do not throw SQL errors from Repositories; wrap them in domain-friendly exceptions if necessary.
- **DTOs vs Entities:** Repositories return Domain Entities (`Adrenalin.Modules.*.Domain.Entities`). Mapping to DTOs for the frontend should happen in the Application layer or via separate Query Services (CQRS), not in the repository itself.
- **Async Only:** All repository methods must be strictly asynchronous (`Task<T>`) to prevent thread-blocking under load.
