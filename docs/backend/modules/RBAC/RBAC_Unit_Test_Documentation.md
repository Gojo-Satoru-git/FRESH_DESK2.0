# RBAC Unit Test Documentation

**Project:** Adrenalin.UnitTests — RBAC Module  
**Framework:** .NET 10.0  
**Total RBAC Tests:** 327 (all passing)  
**Date:** June 2026

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Test Project Architecture](#2-test-project-architecture)
3. [Testing Frameworks and Libraries](#3-testing-frameworks-and-libraries)
4. [Complete File-by-File Documentation](#4-complete-file-by-file-documentation)
5. [Domain Test Documentation](#5-domain-test-documentation)
6. [Validator Test Documentation](#6-validator-test-documentation)
7. [Command Handler Test Documentation](#7-command-handler-test-documentation)
8. [Query Handler Test Documentation](#8-query-handler-test-documentation)
9. [Authorization Test Documentation](#9-authorization-test-documentation)
10. [Controller Test Documentation](#10-controller-test-documentation)
11. [Mocking Strategy](#11-mocking-strategy)
12. [Test Fixtures and Builders](#12-test-fixtures-and-builders)
13. [Coverage Analysis](#13-coverage-analysis)
14. [Test Scenarios Catalog](#14-test-scenarios-catalog)
15. [Edge Cases Covered](#15-edge-cases-covered)
16. [Security Test Coverage](#16-security-test-coverage)
17. [Test Execution Guide](#17-test-execution-guide)
18. [Best Practices Implemented](#18-best-practices-implemented)
19. [Maintenance Guide](#19-maintenance-guide)

---

## 1. Executive Summary

### Purpose

The RBAC Unit Test suite validates the Role-Based Access Control subsystem of the Adrenalin platform. This system governs the lifecycle of **Roles**, **Permissions**, **Groups**, and the assignment relationships between Users and these constructs. The test suite verifies that every domain invariant, business rule, command handler, query handler, validator, controller endpoint, and authorization policy behaves correctly in isolation.

### Testing Strategy

The suite is structured around the **CQRS + Clean Architecture** model of the application. Tests are layered to match the domain model hierarchy:

- **Domain Layer tests** — exercise entity factory methods, state mutations, and domain invariant enforcement (exceptions thrown on invalid state).
- **Application Layer tests** — exercise command handlers (writes) and query handlers (reads) with fully mocked repository dependencies.
- **Presentation Layer tests** — exercise ASP.NET Core MVC controller actions, HTTP status code mapping, claims extraction, and dispatcher delegation.
- **Cross-cutting tests** — authorization security, edge cases, and boundary values are tested as first-class scenarios, not afterthoughts.

### Testing Philosophy

The suite follows these core principles:

- **Arrange–Act–Assert (AAA):** Every test has a clearly structured setup, execution, and verification phase.
- **Test isolation:** Each test is fully self-contained. No shared mutable state exists between tests.
- **No magic numbers, no implicit setup:** Valid data is produced by explicit factory methods and fluent builders.
- **Domain methods are tested as they live in production:** Entities are created via their own factory methods, not raw constructors.
- **Idempotency is explicitly verified:** Duplicate assignments must succeed without side effects.
- **Security rules are tested directly in handler tests and dedicated authorization test classes.**

### Frameworks Used

| Framework | Version | Role |
|-----------|---------|------|
| xUnit | 2.9.3 | Test runner and `[Fact]` test discovery |
| FluentAssertions | 6.12.0 | Expressive assertion library |
| Moq | 4.18.4 | Mocking framework for repository and dispatcher interfaces |
| FluentValidation.TestHelper | 12.1.1 | Validator-specific assertion helpers (`ShouldHaveValidationErrorFor`) |
| NSubstitute | 5.x | Available (partially used as alternative mock framework) |
| coverlet.collector | 6.0.4 | Code coverage collection |
| Microsoft.NET.Test.Sdk | 17.14.1 | Test execution host |

### Coverage Goals

The test suite aims to achieve:
- **100% method coverage** of all domain entity factory methods and state-changing methods.
- **100% branch coverage** of all command handler success and failure paths.
- **100% endpoint coverage** of the `UsersRbacController`.
- **100% rule coverage** of all FluentValidation validators.
- **All edge cases** (null, empty, duplicate, deleted, boundary) explicitly tested.

---

## 2. Test Project Architecture

### Project Structure

```
Adrenalin.UnitTests/
│
├── Adrenalin.UnitTests.csproj          ← Project file, package references
│
└── RBAC/
    ├── Authorization/
    │   └── RbacAuthorizationTests.cs   ← Permission evaluation, system role protection,
    │                                      escalation prevention, JWT claims extraction
    │
    ├── Controllers/
    │   └── UsersRbacControllerTests.cs ← HTTP endpoint coverage for UsersRbacController
    │
    ├── Domain/
    │   ├── Group/
    │   │   ├── GroupEntityTests.cs     ← Group entity factory, update, soft-delete
    │   │   └── UserGroupEntityTests.cs ← UserGroup membership lifecycle
    │   ├── Permission/
    │   │   └── PermissionEntityTests.cs← Permission entity factory, key generation, delete
    │   ├── Role/
    │   │   └── RoleEntityTests.cs      ← Role entity factory, update, soft-delete
    │   └── RolePermission/
    │       ├── RolePermissionEntityTests.cs  ← RolePermission assignment entity
    │       └── UserRoleEntityTests.cs        ← UserRole assignment entity
    │
    ├── EdgeCases/
    │   └── EdgeCaseTests.cs            ← Null/empty, duplicate, deleted entity,
    │                                      restore, boundary, concurrency edge cases
    │
    ├── Handlers/
    │   ├── Commands/
    │   │   ├── GroupCommandHandlerTests.cs                        ← Group CRUD handlers
    │   │   ├── RoleAndPermissionCommandHandlerTests.cs            ← Role + Permission handlers
    │   │   └── RolePermissionAndUserRoleCommandHandlerTests.cs    ← Assignment handlers
    │   └── Queries/
    │       └── RbacQueryHandlerTests.cs ← All read/query handlers
    │
    ├── TestHelpers/
    │   └── TestHelpers.cs              ← RoleBuilder, PermissionBuilder, GroupBuilder,
    │                                      UserBuilder, MockRepositoryFactory,
    │                                      TestData, ReflectionHelper
    │
    └── Validators/
        └── RbacValidatorTests.cs       ← All FluentValidation command validators
```

### Dependency Flow

```
                       ┌────────────────────────────────┐
                       │   RBAC Unit Test Project        │
                       └───────────────┬────────────────┘
                                       │
            ┌──────────────────────────┼──────────────────────────┐
            │                          │                           │
    ┌───────▼───────┐        ┌─────────▼──────┐         ┌─────────▼──────┐
    │ Domain Tests  │        │ App Layer Tests │         │ API Layer Tests │
    │               │        │                │          │                │
    │ Entity .Create│        │ Handlers via   │          │ Controller via  │
    │ Entity .Update│        │ Mock<IRepo>    │          │ Mock<IDispatcher│
    │ Entity .Delete│        │                │          │                │
    └───────────────┘        └────────────────┘          └────────────────┘
            │                          │
            └──────────────────────────┘
                          │
              ┌───────────▼───────────┐
              │   TestHelpers Layer    │
              │                        │
              │ RoleBuilder            │
              │ PermissionBuilder      │
              │ GroupBuilder           │
              │ UserBuilder            │
              │ MockRepositoryFactory  │
              │ TestData               │
              │ ReflectionHelper       │
              └────────────────────────┘
```

### Testing Layers Summary

| Layer | Folder | Dependencies Mocked |
|-------|--------|---------------------|
| Domain | `Domain/` | None — direct entity instantiation |
| Validators | `Validators/` | None — validators instantiated directly |
| Command Handlers | `Handlers/Commands/` | Repository interfaces via `Mock<IRepo>` |
| Query Handlers | `Handlers/Queries/` | Repository interfaces via `Mock<IRepo>` |
| Authorization | `Authorization/` | Repository interfaces, `IDispatcher` |
| Controllers | `Controllers/` | `IDispatcher` |
| Edge Cases | `EdgeCases/` | Repository interfaces (case by case) |

---

## 3. Testing Frameworks and Libraries

### xUnit 2.9.3

**Purpose:** Primary test runner and test discovery framework.

**Usage:** All test methods are decorated with `[Fact]`. Test classes are `public sealed` by convention. xUnit instantiates a new class instance per test, providing automatic test isolation without any `Setup` or `TearDown` orchestration.

**Benefits:**
- Zero shared state between test methods.
- Parallel execution support.
- Native support for `async Task` test methods, critical for testing async handlers.
- Clean, attribute-based discovery compatible with `dotnet test` and Visual Studio Test Explorer.

---

### FluentAssertions 6.12.0

**Purpose:** Expressive, human-readable assertion library.

**Usage throughout the project:**

```csharp
// Object state assertions
role.Name.Should().Be("Support Agent");
role.IsDeleted.Should().BeFalse();
role.CreatedAt.Should().BeAfter(before).And.BeBefore(after);

// Result pattern assertions
result.IsSuccess.Should().BeTrue();
result.Error.Should().Contain("Role name is required");

// Collection assertions
result.Value.Should().HaveCount(2);
result.Value.Should().BeEmpty();
result.Value.Should().BeEquivalentTo(perms);

// Exception assertions
act.Should().Throw<ArgumentException>().WithMessage("*Role name is required*");
act.Should().Throw<InvalidOperationException>().WithMessage("*already deleted*");
act.Should().NotThrow();

// HTTP result type assertions
result.Should().BeOfType<OkObjectResult>();
result.Should().BeOfType<NotFoundObjectResult>();
result.Should().BeOfType<UnauthorizedResult>();
```

**Benefits:**
- Wildcard message matching (`*text*`) for flexible exception message assertions.
- Chained `.And.` assertions for temporal bounds verification.
- `.BeEquivalentTo()` for collection comparison.
- Type-safe `BeOfType<T>().Subject` for further assertions on strongly typed results.

---

### Moq 4.18.4

**Purpose:** Mock object framework for isolating dependencies.

**Usage throughout the project:**

```csharp
// Setup returning a value
_roleRepo.Setup(r => r.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
_roleRepo.Setup(r => r.ExistsByNameAsync("Agent", default)).ReturnsAsync(false);

// Setup returning null (not-found scenarios)
_roleRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Role?)null);

// Setup throwing an exception
_roleRepo.Setup(r => r.SaveChangesAsync(default))
         .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

// Verification that a method was called once
_roleRepo.Verify(r => r.Add(It.IsAny<Role>()), Times.Once);
_roleRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);

// Verification that a method was never called (negative verification)
_roleRepo.Verify(r => r.Add(It.IsAny<Role>()), Times.Never);

// Argument-constrained verification
_dispatcher.Verify(d => d.Send(
    It.Is<AssignRoleToUserCommand>(c => c.ActorId == actorId), default), Times.Once);
```

**Benefits:**
- `It.IsAny<T>()` for wildcard argument matching when the exact value is irrelevant.
- `It.Is<T>(predicate)` for precise argument inspection in `Verify` calls.
- `Times.Once` / `Times.Never` / `Times.Exactly(n)` for strict call count assertions.
- Supports `ReturnsAsync` for `Task<T>` return types, matching the async repository patterns in this codebase.

---

### FluentValidation.TestHelper (via FluentValidation 12.1.1)

**Purpose:** Provides `TestValidate()` and `ShouldHaveValidationErrorFor()` / `ShouldNotHaveValidationErrorFor()` helpers for testing FluentValidation validators.

**Usage:**

```csharp
var result = _sut.TestValidate(ValidCommand() with { Name = string.Empty });
result.ShouldHaveValidationErrorFor(x => x.Name);

var result = _sut.TestValidate(ValidCommand());
result.ShouldNotHaveAnyValidationErrors();
```

**Benefits:**
- Strongly typed property selection avoids magic strings.
- `ShouldNotHaveAnyValidationErrors()` asserts that a fully valid command passes all rules.
- Decouples validator testing from the FluentValidation pipeline middleware.

---

### NSubstitute 5.x

**Purpose:** Included as a package but Moq is the primary mocking framework used in all RBAC tests. NSubstitute is available for test authors who prefer its `Substitute.For<T>()` API style.

---

### coverlet.collector 6.0.4

**Purpose:** Collects code coverage during test runs. Produces reports compatible with `dotnet-coverage`, ReportGenerator, and Azure Pipelines.

**Usage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 4. Complete File-by-File Documentation

### RBAC Module Files

| File | Namespace | Category | Test Count |
|------|-----------|----------|------------|
| `RoleEntityTests.cs` | `RBAC.Domain.Role` | Domain | 27 |
| `PermissionEntityTests.cs` | `RBAC.Domain.Permission` | Domain | 21 |
| `GroupEntityTests.cs` | `RBAC.Domain.Group` | Domain | 27 |
| `UserGroupEntityTests.cs` | `RBAC.Domain.UserGroup` | Domain | 16 |
| `RolePermissionEntityTests.cs` | `RBAC.Domain.RolePermission` | Domain | 11 |
| `UserRoleEntityTests.cs` | `RBAC.Domain.UserRole` | Domain | 16 |
| `RbacValidatorTests.cs` | `RBAC.Validators` | Validators | 53 |
| `RoleAndPermissionCommandHandlerTests.cs` | `RBAC.Handlers.Commands` | Handlers | 24 |
| `RolePermissionAndUserRoleCommandHandlerTests.cs` | `RBAC.Handlers.Commands` | Handlers | 22 |
| `GroupCommandHandlerTests.cs` | `RBAC.Handlers.Commands` | Handlers | 26 |
| `RbacQueryHandlerTests.cs` | `RBAC.Handlers.Queries` | Queries | 25 |
| `RbacAuthorizationTests.cs` | `RBAC.Authorization` | Authorization | 10 |
| `UsersRbacControllerTests.cs` | `RBAC.Controllers` | Controllers | 19 |
| `EdgeCaseTests.cs` | `RBAC.EdgeCases` | Edge Cases | 30 |
| `TestHelpers.cs` | `RBAC.TestHelpers` | Infrastructure | 0 (support) |

**Total RBAC test methods: 327**

---

### File Descriptions

#### `RoleEntityTests.cs`

| Property | Value |
|----------|-------|
| File Name | RoleEntityTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Domain.Role |
| Test Category | Domain Entity |
| Entity Under Test | `Adrenalin.Modules.Auth.Domain.Entities.Role` |
| Dependencies Mocked | None |

Tests the `Role` entity in complete isolation, exercising `Role.Create()`, `Role.Update()`, and `Role.SoftDelete()`. All assertions run against the entity's in-memory state.

---

#### `PermissionEntityTests.cs`

| Property | Value |
|----------|-------|
| File Name | PermissionEntityTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Domain.Permission |
| Test Category | Domain Entity |
| Entity Under Test | `Adrenalin.Modules.Auth.Domain.Entities.Permission` |
| Dependencies Mocked | None |

Tests `Permission.Create()`, `Permission.SoftDelete()`, and `Permission.ToKey()`. Validates lowercase normalization of resource and action strings.

---

#### `GroupEntityTests.cs`

| Property | Value |
|----------|-------|
| File Name | GroupEntityTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Domain.Group |
| Test Category | Domain Entity |
| Entity Under Test | `Adrenalin.Modules.Auth.Domain.Entities.Group` |
| Dependencies Mocked | None |

Tests `Group.Create()`, `Group.Update()`, and `Group.SoftDelete()`. Validates uppercase normalization of `RegionCode` and `TierCode`, and the minimum-value constraint on `UnattendedAlertMinutes`.

---

#### `UserGroupEntityTests.cs`

| Property | Value |
|----------|-------|
| File Name | UserGroupEntityTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Domain.UserGroup |
| Test Category | Domain Entity |
| Entity Under Test | `Adrenalin.Modules.Auth.Domain.Entities.UserGroup` |
| Dependencies Mocked | None |

Tests the `UserGroup.Add()` factory, `Restore()`, `SetLead()`, and `SoftDelete()` lifecycle methods.

---

#### `RolePermissionEntityTests.cs`

| Property | Value |
|----------|-------|
| File Name | RolePermissionEntityTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Domain.RolePermission |
| Test Category | Domain Entity |
| Entity Under Test | `Adrenalin.Modules.Auth.Domain.Entities.RolePermission` |
| Dependencies Mocked | None |

Tests `RolePermission.Assign()` and `SoftDelete()`. Validates empty GUID guards on both `RoleId` and `PermissionId`.

---

#### `UserRoleEntityTests.cs`

| Property | Value |
|----------|-------|
| File Name | UserRoleEntityTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Domain.UserRole |
| Test Category | Domain Entity |
| Entity Under Test | `Adrenalin.Modules.Auth.Domain.Entities.UserRole` |
| Dependencies Mocked | None |

Tests `UserRole.Assign()`, `Restore()`, and `SoftDelete()`. Verifies that `CreatedAt` matches `AssignedAt` and that `Restore()` refreshes `AssignedAt` to the current UTC time.

---

#### `RbacValidatorTests.cs`

| Property | Value |
|----------|-------|
| File Name | RbacValidatorTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Validators |
| Test Category | Validators |
| Validators Under Test | 8 FluentValidation validators |
| Dependencies Mocked | None |

Contains 8 validator test classes, each instantiating its validator directly and using `TestValidate()` for positive and negative assertion.

---

#### `RoleAndPermissionCommandHandlerTests.cs`

| Property | Value |
|----------|-------|
| File Name | RoleAndPermissionCommandHandlerTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Handlers.Commands |
| Test Category | Command Handlers |
| Handlers Under Test | `CreateRoleCommandHandler`, `UpdateRoleCommandHandler`, `DeleteRoleCommandHandler`, `CreatePermissionCommandHandler`, `DeletePermissionCommandHandler` |
| Dependencies Mocked | `IRoleRepository`, `IUserRoleRepository`, `IPermissionRepository`, `IRolePermissionRepository` |

---

#### `RolePermissionAndUserRoleCommandHandlerTests.cs`

| Property | Value |
|----------|-------|
| File Name | RolePermissionAndUserRoleCommandHandlerTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Handlers.Commands |
| Test Category | Command Handlers |
| Handlers Under Test | `GrantPermissionToRoleCommandHandler`, `RevokePermissionFromRoleCommandHandler`, `SetRolePermissionsCommandHandler`, `AssignRoleToUserCommandHandler`, `RemoveRoleFromUserCommandHandler`, `SetUserRolesCommandHandler` |
| Dependencies Mocked | `IRoleRepository`, `IPermissionRepository`, `IRolePermissionRepository`, `IUserRepository`, `IUserRoleRepository` |

---

#### `GroupCommandHandlerTests.cs`

| Property | Value |
|----------|-------|
| File Name | GroupCommandHandlerTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Handlers.Commands |
| Test Category | Command Handlers |
| Handlers Under Test | `CreateGroupCommandHandler`, `UpdateGroupCommandHandler`, `DeleteGroupCommandHandler`, `AddUserToGroupCommandHandler`, `RemoveUserFromGroupCommandHandler`, `SetGroupLeadCommandHandler` |
| Dependencies Mocked | `IGroupRepository`, `IUserGroupRepository`, `IUserRepository` |

---

#### `RbacQueryHandlerTests.cs`

| Property | Value |
|----------|-------|
| File Name | RbacQueryHandlerTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Handlers.Queries |
| Test Category | Query Handlers |
| Handlers Under Test | `GetAllRolesQueryHandler`, `GetRoleByIdQueryHandler`, `GetRoleWithPermissionsQueryHandler`, `GetAllPermissionsQueryHandler`, `GetPermissionsByRoleQueryHandler`, `GetUsersQueryHandler`, `GetUserEffectivePermissionsQueryHandler`, `GetAllGroupsQueryHandler`, `GetGroupByIdQueryHandler` |
| Dependencies Mocked | `IRoleRepository`, `IPermissionRepository`, `IRolePermissionRepository`, `IUserRepository`, `IGroupRepository` |

---

#### `RbacAuthorizationTests.cs`

| Property | Value |
|----------|-------|
| File Name | RbacAuthorizationTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Authorization |
| Test Category | Authorization |
| Components Under Test | `GetUserEffectivePermissionsQueryHandler`, `DeleteRoleCommandHandler`, `GrantPermissionToRoleCommandHandler`, `UsersRbacController` |

---

#### `UsersRbacControllerTests.cs`

| Property | Value |
|----------|-------|
| File Name | UsersRbacControllerTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.Controllers |
| Test Category | Controller |
| Controller Under Test | `Adrenalin.unify.API.Controllers.Auth.UsersRbacController` |
| Dependencies Mocked | `IDispatcher` |

---

#### `EdgeCaseTests.cs`

| Property | Value |
|----------|-------|
| File Name | EdgeCaseTests.cs |
| Namespace | Adrenalin.UnitTests.RBAC.EdgeCases |
| Test Category | Edge Cases |
| Components Under Test | All domain entities, command handlers |

---

#### `TestHelpers.cs`

| Property | Value |
|----------|-------|
| File Name | TestHelpers.cs |
| Namespace | Adrenalin.UnitTests.RBAC.TestHelpers |
| Test Category | Test Infrastructure |
| Contains | `RoleBuilder`, `PermissionBuilder`, `GroupBuilder`, `UserBuilder`, `MockRepositoryFactory`, `TestData`, `ReflectionHelper` |

---

## 5. Domain Test Documentation

### 5.1 Role Entity — `RoleEntityTests.cs`

**Entity under test:** `Role`  
**Methods tested:** `Create()`, `Update()`, `SoftDelete()`

#### `Role.Create()` — Happy Path

| Test Method | Purpose |
|-------------|---------|
| `Create_Should_Create_Role_When_Data_Is_Valid` | Verifies the factory returns a non-null role with the provided name and description |
| `Create_Should_Set_Id_To_NonEmpty_Guid` | Verifies a new unique ID is assigned |
| `Create_Should_Set_IsSystemRole_To_False` | System roles are not created by default |
| `Create_Should_Set_IsDeleted_To_False` | New roles are active on creation |
| `Create_Should_Set_CreatedBy` | The actor GUID is recorded |
| `Create_Should_Set_CreatedAt_To_Recent_Utc` | Timestamp is within a 2-second window around `UtcNow` |
| `Create_Should_Trim_Name_Whitespace` | Leading/trailing spaces are stripped |
| `Create_Should_Trim_Description_Whitespace` | Leading/trailing spaces are stripped from description |
| `Create_Should_Allow_Null_Description` | Optional field; no exception thrown |
| `Create_Should_Initialize_Empty_UserRoles_Collection` | Navigation collection is not null and is empty |
| `Create_Should_Initialize_Empty_RolePermissions_Collection` | Navigation collection is not null and is empty |

**Arrange–Act–Assert pattern:**
```
Arrange: (none — factory is the SUT)
Act:     var role = Role.Create("Support Agent", "Handles tickets", actorId);
Assert:  role.Name.Should().Be("Support Agent");
```

#### `Role.Create()` — Validation Failures

| Test Method | Purpose |
|-------------|---------|
| `Create_Should_ThrowException_When_Name_IsEmpty` | Empty string throws `ArgumentException` with "Role name is required" |
| `Create_Should_ThrowException_When_Name_IsWhitespace` | Whitespace-only string throws `ArgumentException` |

#### `Role.Update()` — Happy Path

| Test Method | Purpose |
|-------------|---------|
| `Update_Should_Update_Name_And_Description` | Both fields are replaced |
| `Update_Should_Set_UpdatedBy` | Actor is recorded on update |
| `Update_Should_Set_UpdatedAt_To_Recent_Utc` | UpdatedAt is set and is recent |
| `Update_Should_Trim_Name` | Whitespace trimming applied on update |
| `Update_Should_Trim_Description` | Whitespace trimming applied on update |
| `Update_Should_Allow_Null_Description` | Clearing description is allowed |

#### `Role.Update()` — Validation and Business Rule Failures

| Test Method | Purpose |
|-------------|---------|
| `Update_Should_ThrowException_When_Name_IsEmpty` | Empty name rejected on update |
| `Update_Should_ThrowException_When_Name_IsWhitespace` | Whitespace name rejected on update |
| `Update_Should_Throw_When_Role_Is_Deleted` | Deleted role cannot be mutated — throws `InvalidOperationException` |

#### `Role.SoftDelete()` — Happy Path

| Test Method | Purpose |
|-------------|---------|
| `SoftDelete_Should_Set_IsDeleted_To_True` | Soft delete flag is raised |
| `SoftDelete_Should_Set_UpdatedBy` | Actor is recorded |
| `SoftDelete_Should_Set_UpdatedAt` | Timestamp is set |

#### `Role.SoftDelete()` — Business Rule Violations

| Test Method | Purpose |
|-------------|---------|
| `DeleteRole_Should_Throw_When_Role_IsSystemRole` | System roles are immutable — cannot be deleted |
| `SoftDelete_Should_Throw_When_Role_Already_Deleted` | Double-delete is rejected with "already deleted" |

---

### 5.2 Permission Entity — `PermissionEntityTests.cs`

**Entity under test:** `Permission`  
**Methods tested:** `Create()`, `SoftDelete()`, `ToKey()`

#### `Permission.Create()` — Happy Path

| Test Method | Purpose |
|-------------|---------|
| `Create_Should_Create_Permission_When_Data_Is_Valid` | Factory returns non-null with correct resource and action |
| `Create_Should_Normalize_Resource_To_Lowercase` | "TICKET" → "ticket" |
| `Create_Should_Normalize_Action_To_Lowercase` | "READ" → "read" |
| `Create_Should_Trim_Resource_Whitespace` | Whitespace stripped |
| `Create_Should_Trim_Action_Whitespace` | Whitespace stripped |
| `Create_Should_Set_Description` | Optional description stored |
| `Create_Should_Set_IsDeleted_False` | Active on creation |
| `Create_Should_Set_CreatedBy` | Actor GUID recorded |
| `Create_Should_Set_CreatedAt_To_Recent_Utc` | Timestamp is recent |
| `Create_Should_Set_NonEmpty_Id` | GUID is not empty |
| `Create_Should_Initialize_Empty_RolePermissions_Collection` | Navigation collection initialized |

#### `Permission.Create()` — Validation Failures

| Test Method | Purpose |
|-------------|---------|
| `Create_Should_Throw_When_Resource_IsEmpty` | "Resource is required" |
| `Create_Should_Throw_When_Resource_IsWhitespace` | Whitespace rejected |
| `Create_Should_Throw_When_Action_IsEmpty` | "Action is required" |
| `Create_Should_Throw_When_Action_IsWhitespace` | Whitespace rejected |

#### `Permission.ToKey()`

| Test Method | Purpose |
|-------------|---------|
| `ToKey_Should_Return_Resource_Colon_Action` | "ticket:read" format |
| `ToKey_Should_Return_Lowercase_Key` | Always lowercase regardless of input case |

#### `Permission.SoftDelete()`

| Test Method | Purpose |
|-------------|---------|
| `SoftDelete_Should_Set_IsDeleted_True` | Deletion flag raised |
| `SoftDelete_Should_Set_UpdatedBy` | Actor recorded |
| `SoftDelete_Should_Set_UpdatedAt` | Timestamp set |
| `SoftDelete_Should_Throw_When_Already_Deleted` | Double-delete rejected |

---

### 5.3 Group Entity — `GroupEntityTests.cs`

**Entity under test:** `Group`  
**Methods tested:** `Create()`, `Update()`, `SoftDelete()`

#### Business Rules Validated

- Group name is required and trimmed.
- `RegionCode` and `TierCode` are uppercased (e.g., "eu" → "EU").
- `UnattendedAlertMinutes` must be ≥ 1.
- A newly created group is `IsActive = true` and `IsDeleted = false`.
- `SoftDelete` also sets `IsActive = false`.
- Deleted groups cannot be updated.
- Double-delete is rejected.

| Test Method | Purpose |
|-------------|---------|
| `Create_Should_Create_Group_When_Data_Is_Valid` | Happy path |
| `Create_Should_Normalize_RegionCode_To_Uppercase` | "eu" → "EU" |
| `Create_Should_Normalize_TierCode_To_Uppercase` | "t1" → "T1" |
| `Create_Should_Trim_Name` | Whitespace trimmed |
| `Create_Should_Set_IsActive_True` | Active on creation |
| `Create_Should_Set_IsDeleted_False` | Not deleted on creation |
| `Create_Should_Set_UnattendedAlertMinutes` | Value is stored |
| `Create_Should_Set_CreatedBy` | Actor GUID recorded |
| `Create_Should_Set_CreatedAt_To_Recent_Utc` | Timestamp is recent |
| `Create_Should_Allow_Null_RegionCode` | Optional field |
| `Create_Should_Allow_Null_TierCode` | Optional field |
| `Create_Should_Initialize_Empty_UserGroups_Collection` | Navigation collection initialized |
| `Create_Should_Set_Minimum_UnattendedAlertMinutes_Of_1` | Boundary minimum accepted |
| `Create_Should_Throw_When_Name_IsEmpty` | "Group name required" |
| `Create_Should_Throw_When_Name_IsWhitespace` | Whitespace rejected |
| `Create_Should_Throw_When_AlertMinutes_Is_Zero` | "UnattendedAlertMinutes must be >= 1" |
| `Create_Should_Throw_When_AlertMinutes_Is_Negative` | Negative value rejected |
| `Update_Should_Update_All_Properties` | All fields updated at once |
| `Update_Should_Set_UpdatedAt` | Timestamp set |
| `Update_Should_Normalize_RegionCode_To_Uppercase` | Applied on update |
| `Update_Should_Throw_When_Name_IsEmpty` | Empty name rejected |
| `Update_Should_Throw_When_AlertMinutes_Is_Zero` | Zero alert minutes rejected |
| `Update_Should_Throw_When_Group_Is_Deleted` | Deleted group cannot be mutated |
| `SoftDelete_Should_Set_IsDeleted_True` | Flag raised |
| `SoftDelete_Should_Set_IsActive_False` | Active flag lowered |
| `SoftDelete_Should_Set_UpdatedBy` | Actor recorded |
| `SoftDelete_Should_Throw_When_Already_Deleted` | "Group already deleted" |

---

### 5.4 UserGroup Entity — `UserGroupEntityTests.cs`

**Entity under test:** `UserGroup`  
**Methods tested:** `Add()`, `Restore()`, `SetLead()`, `SoftDelete()`

| Test Method | Purpose |
|-------------|---------|
| `Add_Should_Create_UserGroup_When_Data_Is_Valid` | Both UserId and GroupId are stored |
| `Add_Should_Set_IsLead` | Lead flag is stored |
| `Add_Should_Set_IsLead_False_By_Default` | Non-lead is the default |
| `Add_Should_Set_IsDeleted_False` | Active on creation |
| `Add_Should_Set_CreatedBy` | Actor recorded |
| `Add_Should_Throw_When_UserId_IsEmpty` | "userId required" |
| `Add_Should_Throw_When_GroupId_IsEmpty` | "groupId required" |
| `Restore_Should_Set_IsDeleted_False` | Restores a deleted membership |
| `Restore_Should_Update_IsLead` | IsLead can be changed on restore |
| `Restore_Should_Set_UpdatedBy` | Actor recorded on restore |
| `SetLead_Should_Update_IsLead_Flag` | Flag toggled correctly |
| `SetLead_Should_Set_UpdatedBy` | Actor recorded |
| `SetLead_Should_Throw_When_Membership_Is_Deleted` | "Cannot modify removed membership" |
| `SoftDelete_Should_Set_IsDeleted_True` | Flag raised |
| `SoftDelete_Should_Set_UpdatedBy` | Actor recorded |
| `SoftDelete_Should_Throw_When_Already_Removed` | "Membership already removed" |

---

### 5.5 RolePermission Entity — `RolePermissionEntityTests.cs`

**Entity under test:** `RolePermission`  
**Methods tested:** `Assign()`, `SoftDelete()`

| Test Method | Purpose |
|-------------|---------|
| `Assign_Should_Create_RolePermission_When_Data_Is_Valid` | RoleId and PermissionId are stored |
| `Assign_Should_Set_NonEmpty_Id` | GUID generated |
| `Assign_Should_Set_IsDeleted_False` | Active on assignment |
| `Assign_Should_Set_CreatedBy` | Actor recorded |
| `Assign_Should_Set_CreatedAt_To_Recent_Utc` | Timestamp recent |
| `Assign_Should_Throw_When_RoleId_IsEmpty` | "roleId must not be empty" |
| `Assign_Should_Throw_When_PermissionId_IsEmpty` | "permissionId must not be empty" |
| `SoftDelete_Should_Set_IsDeleted_True` | Flag raised |
| `SoftDelete_Should_Set_UpdatedBy` | Actor recorded |
| `SoftDelete_Should_Set_UpdatedAt` | Timestamp set |
| `SoftDelete_Should_Throw_When_Already_Removed` | "Already removed" |

---

### 5.6 UserRole Entity — `UserRoleEntityTests.cs`

**Entity under test:** `UserRole`  
**Methods tested:** `Assign()`, `Restore()`, `SoftDelete()`

| Test Method | Purpose |
|-------------|---------|
| `Assign_Should_Create_UserRole_When_Data_Is_Valid` | UserId and RoleId stored |
| `Assign_Should_Set_AssignedAt_To_Recent_Utc` | Assignment timestamp set |
| `Assign_Should_Set_AssignedBy` | Assigning actor recorded |
| `Assign_Should_Set_IsDeleted_False` | Active on assignment |
| `Assign_Should_Set_CreatedBy` | Actor recorded |
| `Assign_Should_Set_CreatedAt_Matches_AssignedAt` | Both timestamps aligned |
| `Assign_Should_Throw_When_UserId_IsEmpty` | "userId required" |
| `Assign_Should_Throw_When_RoleId_IsEmpty` | "roleId required" |
| `Restore_Should_Set_IsDeleted_False` | Restoration clears the flag |
| `Restore_Should_Update_AssignedAt` | New assignment timestamp on restore |
| `Restore_Should_Set_AssignedBy_To_Actor` | New actor recorded as assignor |
| `Restore_Should_Set_UpdatedBy` | UpdatedBy set |
| `SoftDelete_Should_Set_IsDeleted_True` | Deletion flag raised |
| `SoftDelete_Should_Set_UpdatedBy` | Actor recorded |
| `SoftDelete_Should_Set_UpdatedAt` | Timestamp set |
| `SoftDelete_Should_Throw_When_UserRole_Already_Removed` | "UserRole already removed" |

---

## 6. Validator Test Documentation

The validator tests use `FluentValidation.TestHelper.TestValidate()` to validate input commands without invoking a handler or touching infrastructure.

### 6.1 Validation Coverage Table

| Validator | Tests | Empty | Exceeds Max | Invalid Format | Null Allowed | ActorId Empty |
|-----------|-------|-------|-------------|----------------|-------------|---------------|
| `CreateRoleCommandValidator` | 8 | ✅ Name | ✅ Name >80, Desc >500 | — | ✅ Desc | ✅ |
| `UpdateRoleCommandValidator` | 5 | ✅ Name | ✅ Name >80 | — | — | ✅ |
| `DeleteRoleCommandValidator` | 3 | — | — | — | — | ✅ |
| `CreatePermissionCommandValidator` | 11 | ✅ Resource, Action | ✅ Resource >60, Action >60 | ✅ Uppercase, Spaces, Colon-in-Action | — | ✅ |
| `SetRolePermissionsCommandValidator` | 4 | — | — | ✅ Empty GUID in list | ✅ Empty list | ✅ |
| `AssignRoleToUserCommandValidator` | 4 | — | — | — | — | ✅ |
| `SetUserRolesCommandValidator` | 3 | — | — | ✅ Empty GUID in list | — | — |
| `CreateGroupCommandValidator` | 10 | ✅ Name | ✅ Name >100, RegionCode >20, TierCode >10 | ✅ Alert ≤ 0 | ✅ RegionCode, TierCode | ✅ |
| `AddUserToGroupCommandValidator` | 4 | — | — | — | — | ✅ |

### 6.2 Validator Details

#### `CreateRoleCommandValidator`

Valid command: `new("Support Agent", "Handles tickets", Guid.NewGuid())`

| Rule | Test |
|------|------|
| Name required | `Name = ""` → error |
| Name max 80 chars | `Name = 81 chars` → error; `80 chars` → no error |
| Description max 500 chars | `501 chars` → error; `500 chars` → no error |
| Description nullable | `Description = null` → no error |
| ActorId not empty GUID | `ActorId = Guid.Empty` → error |

#### `CreatePermissionCommandValidator`

Valid command: `new("ticket", "read", "Read tickets", Guid.NewGuid())`

The validator enforces a strict format for both `Resource` and `Action`:
- Resource regex: `^[a-z_:]+$` — lowercase, underscores, colons allowed.
- Action regex: `^[a-z_]+$` — lowercase and underscores only; colons are **not** allowed in action.

| Rule | Test |
|------|------|
| Resource required | `Resource = ""` → error |
| Resource max 60 chars | `61 chars` → error |
| Resource lowercase only | `"Ticket"` (uppercase) → error |
| Resource no spaces | `"my ticket"` → error |
| Resource colon allowed | `"ticket:sub"` → no error |
| Action required | `Action = ""` → error |
| Action max 60 chars | `61 chars` → error |
| Action lowercase only | `"Read"` → error |
| Action no colon | `"read:all"` → error |
| ActorId not empty | `Guid.Empty` → error |

#### `CreateGroupCommandValidator`

Valid command: `new("Support Team", "EU", "T1", 30, Guid.NewGuid())`

| Rule | Test |
|------|------|
| Name required | `""` → error |
| Name max 100 chars | `101 chars` → error |
| RegionCode max 20 chars | `21 chars` → error; `null` → no error |
| TierCode max 10 chars | `11 chars` → error |
| UnattendedAlertMinutes ≥ 1 | `0` → error; `-5` → error; `1` → no error |
| ActorId not empty | `Guid.Empty` → error |

#### `SetRolePermissionsCommandValidator`

Noteworthy rule: An **empty** `PermissionIds` list is valid (clears all permissions), but a list containing `Guid.Empty` is invalid.

#### `SetUserRolesCommandValidator`

Similar to `SetRolePermissionsCommand`: empty `RoleIds` list is valid, `Guid.Empty` within the list is not.

---

## 7. Command Handler Test Documentation

All command handlers follow a **Result pattern** — they return `Result` or `Result<Guid>` without throwing exceptions. Exceptions from repositories are caught and wrapped into failure results.

### Handler Flow Diagrams

#### Create Role Flow
```
CreateRoleCommand
       │
       ▼
CreateRoleCommandHandler
       │
       ├─ roleRepo.ExistsByNameAsync()
       │       ├─ true  → Result.Failure("...Agent already exists")
       │       └─ false → Role.Create(...)
       │                       │
       │                       ▼
       │               roleRepo.Add(role)
       │               roleRepo.SaveChangesAsync()
       │                       │
       │                       ▼
       └──────────────── Result.Success(role.Id)
```

#### Assign Role to User Flow
```
AssignRoleToUserCommand
       │
       ▼
AssignRoleToUserCommandHandler
       │
       ├─ userRepo.GetByIdAsync()    → null → Failure("not found")
       ├─ roleRepo.GetByIdAsync()    → null → Failure("not found")
       ├─ urRepo.GetAsync()          → existing active → Success (idempotent)
       ├─ urRepo.GetIncludingDeletedAsync()
       │       ├─ deleted found → deleted.Restore() → urRepo.Update() → Success
       │       └─ null         → UserRole.Assign() → urRepo.Add()    → Success
       └──────────────────────── SaveChangesAsync()
```

### 7.1 Role Command Handlers

#### `CreateRoleCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Create_Role_When_Valid` | Success path returns `IsSuccess = true` with non-empty GUID |
| `Should_Call_Add_On_Repository` | `roleRepo.Add()` called exactly once |
| `Should_Call_SaveChanges` | `SaveChangesAsync()` called once |
| `Should_ReturnFailure_When_RoleAlreadyExists` | Error message contains the duplicate name |
| `Should_Not_Call_Add_When_Role_Already_Exists` | `Add()` not called on failure |
| `Should_ReturnFailure_When_Repository_Throws` | Exception message wrapped in result error |

#### `UpdateRoleCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Update_Role_When_Valid` | Name is updated on the entity |
| `Should_Call_Update_On_Repository` | `Update()` called once |
| `Should_Call_SaveChanges` | `SaveChangesAsync()` called once |
| `Should_ReturnFailure_When_Role_NotFound` | Null from repo → "not found" error |
| `Should_ReturnFailure_When_New_Name_Already_Exists` | Duplicate name → error |
| `Should_Succeed_When_Name_Is_Same_Case_Insensitive` | Renaming to same name (case-insensitive) succeeds |

#### `DeleteRoleCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_SoftDelete_Role` | `role.IsDeleted` is true after command |
| `Should_SoftDelete_Associated_UserRoles` | `SoftDeleteByRoleAsync()` called once |
| `Should_ReturnFailure_When_Role_NotFound` | Not found → failure |
| `Should_ReturnFailure_When_Role_Is_SystemRole` | "System roles cannot be deleted" |
| `Should_Call_SaveChanges` | `SaveChangesAsync()` called |

#### `CreatePermissionCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Create_Permission` | Success with GUID return |
| `Should_Call_Add_On_Repository` | `Add()` called once |
| `Should_ReturnFailure_When_Permission_Already_Exists` | Error contains "ticket:read" key |
| `Should_Call_SaveChanges_On_Success` | `SaveChangesAsync()` called |

#### `DeletePermissionCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_SoftDelete_Permission` | `perm.IsDeleted` true |
| `Should_SoftDelete_Associated_RolePermissions` | `SoftDeleteByPermissionAsync()` called |
| `Should_ReturnFailure_When_Permission_NotFound` | Not found → failure |

### 7.2 RolePermission and UserRole Command Handlers

#### `GrantPermissionToRoleCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Assign_Permission_When_Valid` | Success |
| `Should_Add_RolePermission` | `rpRepo.Add()` called |
| `Should_Be_Idempotent_When_Permission_Already_Granted` | Existing non-deleted assignment → success, no `Add()` |
| `Should_ReturnFailure_When_Role_NotFound` | Role not found |
| `Should_ReturnFailure_When_Permission_NotFound` | Permission not found |

#### `RevokePermissionFromRoleCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Revoke_Permission` | Assignment soft-deleted |
| `Should_Call_Update` | `Update()` called |
| `Should_ReturnFailure_When_Not_Assigned` | "not assigned" error |

#### `SetRolePermissionsCommandHandler`

Performs an atomic replace: soft-deletes all existing permissions for the role, then adds the new set.

| Test | Scenario |
|------|---------|
| `Should_Delete_Old_And_Add_New_Permissions` | `SoftDeleteByRoleAsync()` once; `Add()` × 2 |
| `Should_ReturnFailure_When_Role_NotFound` | Role not found |
| `Should_ReturnFailure_When_A_PermissionId_NotFound` | Error contains missing permission GUID |

#### `AssignRoleToUserCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Assign_Role_When_Not_Already_Assigned` | `Add()` called |
| `Should_Be_Idempotent_When_Role_Already_Active` | Active assignment → success, no `Add()` |
| `Should_Restore_Deleted_UserRole` | Deleted → `Restore()` + `Update()`, no `Add()` |
| `Should_ReturnFailure_When_User_NotFound` | User not found |
| `Should_ReturnFailure_When_Role_NotFound` | Role not found |

#### `RemoveRoleFromUserCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_SoftDelete_UserRole` | `ur.IsDeleted` true |
| `Should_ReturnFailure_When_UserRole_NotFound` | "not assigned" error |

#### `SetUserRolesCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Delete_Old_And_Add_New_Roles` | `SoftDeleteByUserAsync()` once; `Add()` × 2 |
| `Should_ReturnFailure_When_User_NotFound` | User not found |
| `Should_ReturnFailure_When_A_RoleId_NotFound` | Error contains missing role GUID |
| `Should_Clear_All_Roles_When_RoleIds_Is_Empty` | `SoftDeleteByUserAsync()` once; `Add()` never |

### 7.3 Group Command Handlers

#### `CreateGroupCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Create_Group_When_Valid` | Success with GUID |
| `Should_Call_Add` | `Add()` once |
| `Should_Call_SaveChanges` | `SaveChangesAsync()` once |
| `Should_ReturnFailure_When_Name_Already_Exists` | Duplicate name rejected; `Add()` not called |
| `Should_ReturnFailure_When_Repository_Throws` | Exception wrapped in failure |

#### `UpdateGroupCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_Update_Group_When_Valid` | Name updated |
| `Should_Call_Update_And_SaveChanges` | Both called once |
| `Should_ReturnFailure_When_Group_NotFound` | Not found |
| `Should_ReturnFailure_When_New_Name_Exists` | Duplicate name rejected |
| `Should_Succeed_When_Name_Is_Same_Case_Insensitive` | Same name case-insensitive match allows update |

#### `DeleteGroupCommandHandler`

| Test | Scenario |
|------|---------|
| `Should_SoftDelete_Group` | `IsDeleted = true`, `IsActive = false` |
| `Should_SoftDelete_UserGroup_Memberships` | `SoftDeleteByGroupAsync()` called |
| `Should_ReturnFailure_When_Group_NotFound` | Not found |
| `Should_Call_SaveChanges` | `SaveChangesAsync()` called |

---

## 8. Query Handler Test Documentation

All query handlers return `Result<T>` where `T` is a DTO or collection. Navigation properties required for mapping (e.g., `RolePermission.Permission`) are injected via reflection in tests where EF Core would normally populate them.

### 8.1 Query Handler Coverage

| Handler | Tests | Success | Not-Found | Empty List | Mapping | Pagination | Filtering |
|---------|-------|---------|-----------|------------|---------|------------|-----------|
| `GetAllRolesQueryHandler` | 4 | ✅ | — | ✅ | ✅ Name, IsSystemRole | — | — |
| `GetRoleByIdQueryHandler` | 3 | ✅ | ✅ | — | ✅ Id, Name, Desc | — | — |
| `GetRoleWithPermissionsQueryHandler` | 2 | ✅ Active only | ✅ | — | ✅ Resource, Action | — | — |
| `GetAllPermissionsQueryHandler` | 3 | ✅ | — | ✅ | ✅ Resource, Action | — | — |
| `GetPermissionsByRoleQueryHandler` | 2 | ✅ | — | ✅ | ✅ Resource | — | — |
| `GetUsersQueryHandler` | 4 | ✅ | — | ✅ | ✅ Email | ✅ | ✅ |
| `GetUserEffectivePermissionsQueryHandler` | 2 | ✅ | — | ✅ | ✅ | — | — |
| `GetAllGroupsQueryHandler` | 3 | ✅ | — | ✅ | ✅ Name | — | — |
| `GetGroupByIdQueryHandler` | 2 | ✅ | ✅ | — | ✅ Id, Name | — | — |

### 8.2 Notable Query Handler Tests

#### `GetRoleWithPermissionsQueryHandler`

This handler filters out soft-deleted `RolePermission` entries. The test sets up both an active and a deleted `RolePermission` via reflection (to simulate EF Core navigation property loading) and asserts that only the active permission is returned in the DTO.

```
Role
 ├── RolePermission (active)  → Permission "ticket:read"   ← included
 └── RolePermission (deleted) → Permission "ticket:delete" ← excluded
```

#### `GetUsersQueryHandler` — Pagination

Tests verify that pagination parameters (`PageNumber`, `PageSize`, `TotalCount`) are correctly passed through and mapped to `PagedResultDto<UserSummaryDto>`.

#### `GetUserEffectivePermissionsQueryHandler`

Returns a flat list of permission keys (`"ticket:read"`, `"ticket:write"`) derived from all active roles assigned to the user. The test asserts both the populated and empty cases.

---

## 9. Authorization Test Documentation

The `RbacAuthorizationTests.cs` file contains four dedicated authorization test classes that verify security invariants across the system.

### 9.1 Effective Permissions Resolution — `EffectivePermissionsAuthorizationTests`

Tests the `GetUserEffectivePermissionsQueryHandler` as an authorization primitive.

| Test | Security Rule Verified |
|------|----------------------|
| `HasPermission_Should_ReturnTrue_When_UserHasPermission` | A user with "ticket:read" in their effective permissions is confirmed to have it |
| `HasPermission_Should_ReturnFalse_When_UserLacksPermission` | A user without "ticket:delete" does not have it |
| `HasPermission_Should_Return_Empty_When_User_Has_No_Roles` | A roleless user has no permissions |

### 9.2 System Role Protection — `SystemRoleProtectionTests`

Tests that system roles cannot be deleted under any circumstances.

| Test | Security Rule Verified |
|------|----------------------|
| `DeleteRole_Should_Prevent_Deletion_Of_SystemRole` | `IsSystemRole = true` → `Result.Failure` with "System roles cannot be deleted" |
| `DeleteRole_Should_Not_Cascade_Delete_UserRoles_When_System_Role_Prevents_Delete` | `SoftDeleteByRoleAsync()` is **never called** — the handler fails before reaching cascade logic |

### 9.3 Permission Escalation Prevention — `PermissionEscalationTests`

Tests that phantom role/permission associations cannot be created by supplying unknown GUIDs.

| Test | Security Rule Verified |
|------|----------------------|
| `GrantPermission_Should_Reject_Unknown_RoleId` | Unknown role → `Result.Failure`; `rpRepo.Add()` never called |
| `GrantPermission_Should_Reject_Unknown_PermissionId` | Unknown permission → `Result.Failure`; `rpRepo.Add()` never called |

### 9.4 JWT Claims Extraction — `ClaimsExtractionTests`

Tests the controller's actor resolution logic (`GetActorId()`) via real `HttpContext` injection.

| Test | Security Rule Verified |
|------|----------------------|
| `Controller_Should_Return_Unauthorized_When_Sub_Claim_Is_Missing` | No `sub` or `NameIdentifier` claim → `401 Unauthorized` |
| `Controller_Should_Return_Unauthorized_When_Sub_Claim_Is_Not_A_Guid` | `"not-a-guid"` as claim value → `401 Unauthorized` |
| `Controller_Should_Accept_Sub_Claim_From_Jwt_Convention` | `"sub"` claim with valid GUID → command dispatched with that `ActorId`; `204 No Content` |

The third test also verifies that the extracted `ActorId` from the JWT `sub` claim is propagated correctly into the command: it uses `It.Is<AssignRoleToUserCommand>(c => c.ActorId == actorId)` in the `Verify` assertion.

---

## 10. Controller Test Documentation

### `UsersRbacControllerTests`

**Controller under test:** `Adrenalin.unify.API.Controllers.Auth.UsersRbacController`  
**Dependencies mocked:** `IDispatcher` (Adrenalin's internal mediator pattern)

The controller test class sets up a default authenticated user in `ControllerContext` via constructor. A helper method `SetUnauthenticatedUser()` removes the claims principal for unauthorized tests.

### Endpoint Coverage Table

| Endpoint | Method | HTTP Success | HTTP Failure | Unauthorized Test |
|----------|--------|-------------|--------------|-------------------|
| `GET /api/rbac/users` | `GetAll` | `200 OK` | `400 Bad Request` | — |
| `GET /api/rbac/users/{id}/roles` | `GetWithRoles` | `200 OK` | `404 Not Found` | — |
| `GET /api/rbac/users/{id}/permissions` | `GetEffectivePermissions` | `200 OK` | `404 Not Found` | — |
| `GET /api/rbac/users/{id}/groups` | `GetGroups` | `200 OK` | — | — |
| `POST /api/rbac/users/{id}/roles/assign` | `AssignRole` | `204 No Content` | `400 Bad Request` | ✅ `401` |
| `POST /api/rbac/users/{id}/roles/remove` | `RemoveRole` | `204 No Content` | `400 Bad Request` | ✅ `401` |
| `PUT /api/rbac/users/{id}/roles` | `SetRoles` | `204 No Content` | `400 Bad Request` | ✅ `401` |

### HTTP Status Code Validation

All write endpoints (`AssignRole`, `RemoveRole`, `SetRoles`) follow this pattern:

```
Dispatcher returns Result.Success()      → 204 No Content
Dispatcher returns Result.Failure("...")  → 400 Bad Request
ClaimsPrincipal has no valid sub claim   → 401 Unauthorized
```

All read endpoints follow:

```
Dispatcher returns Result<T>.Success(dto) → 200 OK with dto body
Dispatcher returns Result<T>.Failure("...") → 404 Not Found
```

The `GetAll` endpoint is the exception — it returns `400 Bad Request` on failure (not 404), as the paged users query is a query-based operation where failure indicates a malformed request rather than a missing resource.

### Filter Propagation Test

`GetAll_Should_Pass_Filter_To_Dispatcher` verifies that the `emailQuery`, `isActive`, `pageNumber`, and `pageSize` query parameters are faithfully forwarded to the `GetUsersQuery` command object passed to the dispatcher.

### Role Assignment Dispatch Verification

`AssignRole_Should_Dispatch_With_Correct_UserId_And_RoleId` verifies that the route parameter `{id}` (userId) and the request body's `RoleId` are correctly combined into the `AssignRoleToUserCommand`.

---

## 11. Mocking Strategy

### Mocked Dependencies

| Interface | Moq Behaviour | Verification Strategy |
|-----------|---------------|----------------------|
| `IRoleRepository` | `GetByIdAsync`, `GetAllAsync`, `ExistsByNameAsync`, `Add`, `Update`, `SaveChangesAsync`, `SoftDeleteByRoleAsync` | `Times.Once` / `Times.Never` on write methods |
| `IPermissionRepository` | `GetByIdAsync`, `GetAllAsync`, `ExistsAsync`, `Add`, `SaveChangesAsync`, `SoftDeleteByPermissionAsync` | Same |
| `IRolePermissionRepository` | `GetAsync`, `GetByRoleWithPermissionsAsync`, `Add`, `Update`, `SoftDeleteByRoleAsync`, `SoftDeleteByPermissionAsync`, `SaveChangesAsync` | Same |
| `IUserRoleRepository` | `GetAsync`, `GetIncludingDeletedAsync`, `Add`, `Update`, `SoftDeleteByUserAsync`, `SoftDeleteByRoleAsync`, `SaveChangesAsync` | Same |
| `IGroupRepository` | `GetByIdAsync`, `GetWithMembersAsync`, `GetAllAsync`, `ExistsByNameAsync`, `Add`, `Update`, `SaveChangesAsync` | Same |
| `IUserGroupRepository` | `GetAsync`, `GetIncludingDeletedAsync`, `GetByUserAsync`, `Add`, `Update`, `SoftDeleteByGroupAsync`, `SaveChangesAsync` | Same |
| `IUserRepository` | `GetByIdAsync`, `GetWithRolesAsync`, `GetEffectivePermissionsAsync`, `GetPagedAsync` | Verified for correct parameter passing |
| `IDispatcher` | `Send(command, ct)` → `Result` | Verified with `It.Is<TCommand>` predicates |

### Why Each Dependency is Mocked

**Repositories:** All domain repositories represent database access. Mocking them is essential to keep tests in-memory, deterministic, and fast. Each mock is configured per-test with exact expected inputs and outputs, making the test's intent explicit.

**IDispatcher:** The controller delegates all business logic to the dispatcher. Mocking it decouples the controller tests entirely from the application layer — only the HTTP routing, status code mapping, and claims extraction are under test.

### Exception Propagation Verification

Handlers are tested with `ThrowsAsync` setups to ensure repository exceptions are caught and wrapped:

```csharp
_roleRepo.Setup(r => r.SaveChangesAsync(default))
         .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));
// Assertion:
result.IsSuccess.Should().BeFalse();
result.Error.Should().Contain("Concurrency conflict");
```

---

## 12. Test Fixtures and Builders

All builders and factories live in `TestHelpers/TestHelpers.cs`.

### 12.1 `RoleBuilder`

**Purpose:** Fluent builder for `Role` domain entities with defaults.

**Default values:** `Name = "Default Role"`, `CreatedBy = Guid.NewGuid()`

**API:**
```csharp
new RoleBuilder()
    .WithName("Support Agent")
    .WithDescription("Handles tickets")
    .WithCreatedBy(actorId)
    .AsSystemRole()     // sets IsSystemRole = true via reflection
    .AsDeleted()        // calls SoftDelete() after creation
    .Build();
```

**Why reflection for `IsSystemRole`:** The `IsSystemRole` property has a private setter (managed only by the system), so tests use `typeof(Role).GetProperty(...).SetValue(...)` to simulate the state without exposing a public mutation method.

---

### 12.2 `PermissionBuilder`

**Purpose:** Fluent builder for `Permission` entities.

**Default values:** `Resource = "ticket"`, `Action = "read"`, `CreatedBy = Guid.NewGuid()`

```csharp
new PermissionBuilder()
    .WithResource("user")
    .WithAction("delete")
    .WithDescription("Delete users")
    .AsDeleted()
    .Build();
```

---

### 12.3 `GroupBuilder`

**Purpose:** Fluent builder for `Group` entities.

**Default values:** `Name = "Default Group"`, `RegionCode = "EU"`, `TierCode = "T1"`, `AlertMinutes = 30`

```csharp
new GroupBuilder()
    .WithName("Support Team")
    .WithRegionCode("US")
    .WithTierCode("T2")
    .WithAlertMinutes(45)
    .AsDeleted()
    .Build();
```

---

### 12.4 `UserBuilder`

**Purpose:** Creates `User` entities that have a private constructor, using `RuntimeHelpers.GetUninitializedObject` and reflection to set properties.

**Default values:** `Email = "default@example.com"`, `FirstName = "John"`, `LastName = "Doe"`, `IsActive = true`

```csharp
new UserBuilder()
    .WithId(specificId)
    .WithEmail("agent@example.com")
    .AsInactive()
    .Build();
```

**Why `GetUninitializedObject`:** The `User` entity has no public factory method accessible from the test project (it lives in the Auth module with a private/internal constructor). This reflection-based approach is the only way to create `User` instances in tests without modifying production code.

---

### 12.5 `MockRepositoryFactory`

**Purpose:** Creates pre-configured `Mock<IRepo>` instances with the most common setup patterns, reducing boilerplate in handler tests.

```csharp
// Create a role repository mock that returns the given role
var mock = MockRepositoryFactory.RoleRepository(roleToReturn: role, nameExists: false);

// Create a user role repository mock with both active and deleted scenarios
var mock = MockRepositoryFactory.UserRoleRepository(activeRole: ur, deletedRole: deletedUr);
```

Each factory method wires up common query methods (`GetByIdAsync`, `GetAllAsync`, `ExistsByNameAsync` etc.) with sensible defaults.

---

### 12.6 `TestData`

**Purpose:** Static factory for composite test data combinations frequently needed together.

```csharp
// A Role with a Permission already assigned
var (role, permission, rp) = TestData.RoleWithPermission();

// A User with a Role assigned
var (user, role, ur) = TestData.UserWithRole();

// A User in a Group
var (user, group, ug) = TestData.UserWithGroup(isLead: true);

// Multiple Permissions
var perms = TestData.MultiplePermissions(count: 5);

// Multiple Roles
var roles = TestData.MultipleRoles(count: 3);
```

---

### 12.7 `ReflectionHelper`

**Purpose:** Centralises reflection-based property access for setting EF Core navigation properties that have private setters.

```csharp
// Attach a Permission to a RolePermission navigation property
ReflectionHelper.AttachPermissionToRolePermission(rp, perm);

// Attach RolePermissions collection to a Role
ReflectionHelper.AttachRolePermissionsToRole(role, new List<RolePermission> { rp });

// Attach UserRoles to a User
ReflectionHelper.AttachUserRolesToUser(user, new List<UserRole> { ur });

// Attach UserGroups to a Group
ReflectionHelper.AttachUserGroupsToGroup(group, new List<UserGroup> { ug });
```

**Why this is necessary:** EF Core populates navigation properties during query materialization. In unit tests (no database), navigation properties must be set via reflection because they have private setters — a deliberate design decision in the domain model to prevent accidental mutation outside EF Core.

---

## 13. Coverage Analysis

### Per-Component Coverage Summary

| Component | Test File(s) | Test Count | Coverage Estimate |
|-----------|-------------|------------|-------------------|
| Role Entity | `RoleEntityTests.cs` | 27 | ~100% methods |
| Permission Entity | `PermissionEntityTests.cs` | 21 | ~100% methods |
| Group Entity | `GroupEntityTests.cs` | 27 | ~100% methods |
| UserGroup Entity | `UserGroupEntityTests.cs` | 16 | ~100% methods |
| RolePermission Entity | `RolePermissionEntityTests.cs` | 11 | ~100% methods |
| UserRole Entity | `UserRoleEntityTests.cs` | 16 | ~100% methods |
| Validators | `RbacValidatorTests.cs` | 53 | ~100% rules |
| Role/Permission Handlers | `RoleAndPermissionCommandHandlerTests.cs` | 24 | ~100% branches |
| RolePermission/UserRole Handlers | `RolePermissionAndUserRoleCommandHandlerTests.cs` | 22 | ~100% branches |
| Group Handlers | `GroupCommandHandlerTests.cs` | 26 | ~100% branches |
| Query Handlers | `RbacQueryHandlerTests.cs` | 25 | ~100% methods |
| Authorization | `RbacAuthorizationTests.cs` | 10 | All security rules |
| Controllers | `UsersRbacControllerTests.cs` | 19 | All endpoints |
| Edge Cases | `EdgeCaseTests.cs` | 30 | All identified edge cases |
| **Total** | **15 files** | **327** | **High** |

### Running Coverage Collection

```bash
dotnet test --collect:"XPlat Code Coverage"
# Generates coverage.cobertura.xml in TestResults/

# Generate HTML report (requires reportgenerator tool):
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

---

## 14. Test Scenarios Catalog

### Role Tests

- Create Role — valid data
- Create Role — empty name (validation)
- Create Role — whitespace name (validation)
- Create Role — duplicate name (handler)
- Create Role — repository throws exception (handler)
- Update Role — valid data
- Update Role — empty name (validation)
- Update Role — name exceeds 80 characters (validation)
- Update Role — duplicate name (handler)
- Update Role — role not found (handler)
- Update Role — same name case-insensitive (handler — allowed)
- Update Role — role is deleted (domain)
- Update Role — repository throws on save (handler)
- Delete Role — happy path
- Delete Role — system role protection
- Delete Role — role not found
- Delete Role — cascades to UserRoles
- Delete Role — repository throws (handler)

### Permission Tests

- Create Permission — valid data
- Create Permission — empty resource (validation + domain)
- Create Permission — empty action (validation + domain)
- Create Permission — uppercase resource (validation)
- Create Permission — uppercase action (validation)
- Create Permission — resource contains spaces (validation)
- Create Permission — action contains colon (validation)
- Create Permission — duplicate resource:action pair
- Create Permission — resource max 60 chars (validation)
- Create Permission — action max 60 chars (validation)
- Permission ToKey — "resource:action" format
- Permission ToKey — always lowercase
- Delete Permission — happy path
- Delete Permission — not found
- Delete Permission — cascades to RolePermissions

### RolePermission Tests

- Assign Permission to Role — new assignment
- Assign Permission to Role — idempotent (already granted)
- Assign Permission to Role — role not found
- Assign Permission to Role — permission not found
- Revoke Permission from Role — happy path
- Revoke Permission from Role — not assigned
- Set Role Permissions — bulk replace (old deleted, new added)
- Set Role Permissions — role not found
- Set Role Permissions — invalid permission ID in list

### UserRole Tests

- Assign Role to User — new assignment
- Assign Role to User — idempotent (already active)
- Assign Role to User — restore deleted assignment
- Assign Role to User — user not found
- Assign Role to User — role not found
- Remove Role from User — happy path
- Remove Role from User — not assigned
- Set User Roles — bulk replace
- Set User Roles — empty list clears all roles
- Set User Roles — user not found
- Set User Roles — invalid role ID in list

### Group Tests

- Create Group — valid data
- Create Group — empty name
- Create Group — name exceeds 100 chars (validation)
- Create Group — duplicate name
- Create Group — UnattendedAlertMinutes zero (validation + domain)
- Create Group — UnattendedAlertMinutes negative (validation + domain)
- Create Group — minimum alert minutes (1)
- Create Group — null RegionCode and TierCode
- Update Group — valid data
- Update Group — group not found
- Update Group — duplicate name
- Update Group — same name case-insensitive
- Update Group — deleted group
- Delete Group — happy path
- Delete Group — cascades to UserGroup memberships
- Delete Group — not found

### UserGroup (Membership) Tests

- Add User to Group — new membership
- Add User to Group — empty UserId (domain)
- Add User to Group — empty GroupId (domain)
- Remove User from Group — happy path
- Remove User from Group — membership not found
- Set Group Lead — promote/demote member
- Set Group Lead — deleted membership (domain)
- Restore Group Membership — re-add previously removed member

### Query Tests

- Get All Roles — returns all roles
- Get All Roles — empty list
- Get Role By Id — found
- Get Role By Id — not found
- Get Role With Permissions — active permissions only
- Get Role With Permissions — not found
- Get All Permissions — returns all
- Get All Permissions — empty list
- Get Permissions By Role — returns permissions for role
- Get Permissions By Role — empty list
- Get Users — paged results
- Get Users — empty page
- Get Users — filter parameters passed correctly
- Get Users — email mapped correctly
- Get User Effective Permissions — returns permission keys
- Get User Effective Permissions — empty (no roles)
- Get All Groups — returns all groups
- Get All Groups — empty list
- Get Group By Id — found
- Get Group By Id — not found

### Authorization Tests

- User has permission → confirmed present in effective permissions
- User lacks permission → confirmed absent
- User has no roles → empty permissions
- System role deletion prevented
- System role deletion does not cascade
- Unknown role ID rejected in grant
- Unknown permission ID rejected in grant
- Missing JWT sub claim → 401
- Invalid JWT sub claim (non-GUID) → 401
- Valid JWT sub claim → command dispatched with correct ActorId

---

## 15. Edge Cases Covered

### Null / Empty Values

| Edge Case | Entity/Handler | Test Class |
|-----------|----------------|------------|
| `null` description on Role | `Role.Create` | `NullAndEmptyEdgeCaseTests` |
| `null` description on Permission | `Permission.Create` | `NullAndEmptyEdgeCaseTests` |
| `null` RegionCode and TierCode on Group | `Group.Create` | `NullAndEmptyEdgeCaseTests` |
| Navigation collections initialized to empty (not null) | Role, Permission, Group | `NullAndEmptyEdgeCaseTests` |
| Empty `PermissionIds` list (clears all) | `SetRolePermissionsCommandHandler` | `BoundaryValueEdgeCaseTests` |
| Empty `RoleIds` list (clears all) | `SetUserRolesCommandHandler` | `BoundaryValueEdgeCaseTests` |

### Duplicate Assignments

| Edge Case | Handler | Test Class |
|-----------|---------|------------|
| Duplicate `GrantPermission` (active) | `GrantPermissionToRoleCommandHandler` | `DuplicateAssignmentEdgeCaseTests` |
| Duplicate `AssignRoleToUser` (active) | `AssignRoleToUserCommandHandler` | `DuplicateAssignmentEdgeCaseTests` |
| Duplicate role name | `CreateRoleCommandHandler` | `DuplicateAssignmentEdgeCaseTests` |
| Duplicate permission resource:action | `CreatePermissionCommandHandler` | `DuplicateAssignmentEdgeCaseTests` |

### Deleted Entity Operations

| Edge Case | Entity | Error Message |
|-----------|--------|---------------|
| Update deleted Role | `Role.Update` | "Cannot modify a deleted role" |
| Double-delete Role | `Role.SoftDelete` | "already deleted" |
| Double-delete Permission | `Permission.SoftDelete` | "already deleted" |
| Double-delete RolePermission | `RolePermission.SoftDelete` | "Already removed" |
| Double-delete UserRole | `UserRole.SoftDelete` | "UserRole already removed" |
| Double-delete UserGroup | `UserGroup.SoftDelete` | "Membership already removed" |
| Update deleted Group | `Group.Update` | "Cannot modify a deleted group" |
| Double-delete Group | `Group.SoftDelete` | "Group already deleted" |
| SetLead on removed UserGroup | `UserGroup.SetLead` | "Cannot modify removed membership" |

### Restore (Soft-Delete + Re-Assign)

| Edge Case | Verified Behaviour |
|-----------|-------------------|
| Re-assign previously deleted `UserRole` | Handler calls `Restore()` + `Update()` — no new entity created |
| Re-add previously deleted `UserGroup` | Handler calls `Restore()` + `Update()` |

### Boundary Values

| Edge Case | Boundary |
|-----------|---------|
| Role name exactly 80 characters | Accepted |
| Role name with whitespace padding | Trimmed, length is of content only |
| Group `UnattendedAlertMinutes = 1` | Minimum accepted |
| Group `UnattendedAlertMinutes = int.MaxValue` | No upper bound enforced |
| Single permission in `SetRolePermissions` | `Add()` called exactly once |

### Concurrency / Exception Propagation

| Edge Case | Scenario |
|-----------|---------|
| `SaveChangesAsync` throws on Create Role | Result.Failure with exception message |
| `SaveChangesAsync` throws on Update Role | Result.Failure with exception message |
| `GetByIdAsync` throws (timeout) on Delete Role | Result.Failure with timeout message |
| `SaveChangesAsync` throws on GrantPermission (unique constraint) | Result.Failure |

### Invalid GUIDs

| Edge Case | Where Tested |
|-----------|-------------|
| `Guid.Empty` as RoleId in `RolePermission.Assign` | `RolePermissionEntityTests` |
| `Guid.Empty` as PermissionId in `RolePermission.Assign` | `RolePermissionEntityTests` |
| `Guid.Empty` as UserId in `UserRole.Assign` | `UserRoleEntityTests` |
| `Guid.Empty` as RoleId in `UserRole.Assign` | `UserRoleEntityTests` |
| `Guid.Empty` as UserId in `UserGroup.Add` | `UserGroupEntityTests` |
| `Guid.Empty` as GroupId in `UserGroup.Add` | `UserGroupEntityTests` |
| `Guid.Empty` in PermissionIds list | `RbacValidatorTests` |
| `Guid.Empty` in RoleIds list | `RbacValidatorTests` |

---

## 16. Security Test Coverage

### Authorization Enforcement

| Security Rule | Tested By | Test Class |
|---------------|-----------|------------|
| System roles cannot be deleted | `DeleteRoleCommandHandler` | `SystemRoleProtectionTests` |
| System role deletion does not cascade UserRole deletions | `DeleteRoleCommandHandler` verify | `SystemRoleProtectionTests` |
| Unknown RoleId cannot receive permission grant | `GrantPermissionToRoleCommandHandler` | `PermissionEscalationTests` |
| Unknown PermissionId cannot be granted to a role | `GrantPermissionToRoleCommandHandler` | `PermissionEscalationTests` |

### JWT Claims Validation

| Security Rule | Tested By |
|---------------|-----------|
| Missing `sub` / `NameIdentifier` claim → 401 | `ClaimsExtractionTests` |
| Non-GUID `sub` claim value → 401 | `ClaimsExtractionTests` |
| Valid `sub` GUID claim → command dispatched with correct ActorId | `ClaimsExtractionTests` |

### Permission Resolution

| Security Rule | Tested By |
|---------------|-----------|
| User with permission X can be verified as having it | `EffectivePermissionsAuthorizationTests` |
| User without permission X is confirmed to lack it | `EffectivePermissionsAuthorizationTests` |
| Roleless user has zero effective permissions | `EffectivePermissionsAuthorizationTests` |

### Write Endpoint Authentication

All three write endpoints (`AssignRole`, `RemoveRole`, `SetRoles`) return `401 Unauthorized` when the `ClaimsPrincipal` contains no valid actor identity. This is verified in `UsersRbacControllerTests`:

```
SetUnauthenticatedUser() → _sut.AssignRole(...) → UnauthorizedResult (401)
```

---

## 17. Test Execution Guide

### Prerequisites

- .NET 10.0 SDK
- All NuGet packages restored (automatic with `dotnet test`)

### Run All Tests

```bash
dotnet test Adrenalin.UnitTests/
```

### Run Only RBAC Tests

```bash
dotnet test Adrenalin.UnitTests/ --filter "FullyQualifiedName~RBAC"
```

Expected output:
```
Test summary: total: 327, failed: 0, succeeded: 327, skipped: 0, duration: ~8s
```

### Run with Code Coverage

```bash
dotnet test Adrenalin.UnitTests/ --collect:"XPlat Code Coverage"
```

Coverage files are written to `TestResults/<guid>/coverage.cobertura.xml`.

### Run Specific Test Category

```bash
# Only domain tests
dotnet test --filter "FullyQualifiedName~RBAC.Domain"

# Only handler tests
dotnet test --filter "FullyQualifiedName~RBAC.Handlers"

# Only validator tests
dotnet test --filter "FullyQualifiedName~RBAC.Validators"

# Only controller tests
dotnet test --filter "FullyQualifiedName~RBAC.Controllers"

# Only edge case tests
dotnet test --filter "FullyQualifiedName~RBAC.EdgeCases"

# Only authorization tests
dotnet test --filter "FullyQualifiedName~RBAC.Authorization"
```

### Run a Single Test Method

```bash
dotnet test --filter "FullyQualifiedName~CreateRoleCommandHandler_Should_Create_Role_When_Valid"
```

### Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Parallel Execution

xUnit runs test classes in parallel by default. To disable:

```bash
dotnet test -- xUnit.MaxParallelThreads=1
```

---

## 18. Best Practices Implemented

### Arrange–Act–Assert (AAA)

Every test is structured in three clearly delimited phases:

```csharp
[Fact]
public async Task CreateRoleCommandHandler_Should_Create_Role_When_Valid()
{
    // Arrange
    _roleRepo.Setup(r => r.ExistsByNameAsync("Agent", default)).ReturnsAsync(false);

    // Act
    var result = await _sut.Handle(ValidCommand("Agent"), default);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeEmpty();
}
```

### Test Isolation

Each test class creates fresh mocks and a fresh System Under Test (SUT) instance in its constructor:

```csharp
public sealed class CreateRoleCommandHandlerTests
{
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly CreateRoleCommandHandler _sut;

    public CreateRoleCommandHandlerTests()
        => _sut = new CreateRoleCommandHandler(_roleRepo.Object);
}
```

No static mutable state. No `[Collection]` fixtures with shared resources. Each test is fully independent.

### Dependency Mocking

All infrastructure dependencies (repositories, dispatcher) are mocked. Domain entities are the only non-mocked dependencies — they are instantiated directly via their own factory methods, which is appropriate because they contain no infrastructure concerns.

### Fluent Builders

The `RoleBuilder`, `PermissionBuilder`, `GroupBuilder`, and `UserBuilder` classes provide a fluent API for constructing test data, making test intent readable:

```csharp
var role = new RoleBuilder()
    .WithName("Support Agent")
    .AsSystemRole()
    .Build();
```

### Valid Command Factories

Handler tests define a `ValidCommand()` static helper that returns a known-good command, with `with` record expressions used for targeted mutation:

```csharp
private static CreateRoleCommand ValidCommand() =>
    new("Support Agent", "Handles tickets", Guid.NewGuid());

// In test:
_sut.TestValidate(ValidCommand() with { Name = string.Empty });
```

### Negative Verification

Handler tests verify not only that the correct methods are called, but also that incorrect methods are **not** called:

```csharp
// When role already exists, Add() must never be called
_roleRepo.Verify(r => r.Add(It.IsAny<Role>()), Times.Never);
```

### Deterministic Testing

No random or time-dependent values without explicit control. `Guid.NewGuid()` in test data is fine (each test gets fresh IDs), and UTC time assertions use before/after bounding windows:

```csharp
var before = DateTimeOffset.UtcNow.AddSeconds(-1);
var role = MakeRole();
var after = DateTimeOffset.UtcNow.AddSeconds(1);

role.CreatedAt.Should().BeAfter(before).And.BeBefore(after);
```

### Result Pattern Consistency

The entire application layer returns a `Result`/`Result<T>` discriminated union. Tests uniformly assert `result.IsSuccess` and `result.Error` without dealing with exceptions at the handler boundary.

---

## 19. Maintenance Guide

### How to Add a New Test

1. Identify the layer: Domain, Validator, Handler (Command/Query), Controller, or Edge Case.
2. Create or append to the appropriate file in the matching `RBAC/` subfolder.
3. Add a new `public sealed class` for each new SUT class being tested.
4. Use `RoleBuilder` / `PermissionBuilder` / `GroupBuilder` / `UserBuilder` from `TestHelpers` for domain entity construction.
5. Use `MockRepositoryFactory` for pre-configured repository mocks, or create `Mock<IRepo>` instances directly for more specific configurations.
6. Follow the AAA pattern. Use `_sut` for the system under test.

### How to Add a New Validator Test

```csharp
public sealed class MyNewCommandValidatorTests
{
    private readonly MyNewCommandValidator _sut = new();

    private static MyNewCommand ValidCommand() =>
        new(Guid.NewGuid(), "valid-value", Guid.NewGuid());

    [Fact]
    public void Should_Not_Have_Error_When_Request_IsValid()
    {
        var result = _sut.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Name_IsEmpty()
    {
        var result = _sut.TestValidate(ValidCommand() with { Name = string.Empty });
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
```

### How to Add a New Handler Test

```csharp
public sealed class MyNewCommandHandlerTests
{
    private readonly Mock<IMyRepository> _repo = new();
    private readonly MyNewCommandHandler _sut;

    public MyNewCommandHandlerTests()
        => _sut = new MyNewCommandHandler(_repo.Object);

    [Fact]
    public async Task Should_Succeed_When_Valid()
    {
        // Arrange
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
             .ReturnsAsync(new MyBuilder().Build());

        // Act
        var result = await _sut.Handle(new MyNewCommand(Guid.NewGuid()), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
```

### How to Update an Existing Test

When a domain rule or handler behaviour changes:

1. Locate the test in the relevant file.
2. Update the `Arrange` section if the mock setup changes.
3. Update the `Assert` section if the expected outcome changes.
4. If an error message changes, update `.WithMessage("*new message*")` — wildcard matching is used, so only the changed substring needs updating.
5. Run `dotnet test --filter` with the class or method name to verify only the changed tests.

### Naming Conventions

Test class names:
```
{SUT Class Name}Tests
```

Test method names follow the pattern:
```
{SUT Method}_{Should|Must}_{ExpectedOutcome}_When_{Condition}
```

Examples:
- `Create_Should_Create_Role_When_Data_Is_Valid`
- `CreateRoleCommandHandler_Should_ReturnFailure_When_RoleAlreadyExists`
- `Controller_Should_Return_Unauthorized_When_Sub_Claim_Is_Missing`

### Folder Conventions

| New Component Type | Folder |
|--------------------|--------|
| New aggregate entity | `RBAC/Domain/{EntityName}/` |
| New validator | Append to `RBAC/Validators/RbacValidatorTests.cs` |
| New command handler | Append to appropriate file in `RBAC/Handlers/Commands/` or create a new file |
| New query handler | Append to `RBAC/Handlers/Queries/RbacQueryHandlerTests.cs` |
| New controller | `RBAC/Controllers/{ControllerName}Tests.cs` |
| New authorization rule | Append to `RBAC/Authorization/RbacAuthorizationTests.cs` |
| New edge case | Append to `RBAC/EdgeCases/EdgeCaseTests.cs` |

### Adding New Builders to TestHelpers

When a new domain entity is introduced, add a corresponding fluent builder to `TestHelpers/TestHelpers.cs` following the existing `RoleBuilder` pattern. If the entity has private setters on key properties, add the appropriate `ReflectionHelper` static methods.

---

*Documentation generated from source code analysis — June 2026*
