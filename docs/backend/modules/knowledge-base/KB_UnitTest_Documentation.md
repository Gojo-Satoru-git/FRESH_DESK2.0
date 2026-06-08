# Knowledge Base — Unit Test Documentation

> **Project:** Adrenalin.UnitTests
> **Framework:** .NET 10 · xUnit 3.1.4 · NSubstitute
> **Pattern:** Domain-Driven Design (CQRS)
> **Last run:** 194 tests · ✅ 194 passed · ❌ 0 failed

---

## Table of Contents

1. [Test Suite Summary](#1-test-suite-summary)
2. [Architecture & Test Design](#2-architecture--test-design)
3. [Domain Layer Tests](#3-domain-layer-tests)
   - 3.1 [PortalBanner](#31-portalbanner--19-tests)
   - 3.2 [KbArticle](#32-kbarticle--35-tests)
   - 3.3 [KbFolder](#33-kbfolder--12-tests)
4. [Validator Tests](#4-validator-tests)
   - 4.1 [Folder Validators](#41-folder-command-validators--15-tests)
   - 4.2 [Article Validators](#42-article-command-validators--8-tests)
   - 4.3 [AutoResolve Validators](#43-autoresolve-command-validators--16-tests)
   - 4.4 [Attachment Validators](#44-attachment-command-validators--13-tests)
   - 4.5 [Banner Validators](#45-portal-banner-command-validators--10-tests)
5. [Handler Tests](#5-handler-tests)
   - 5.1 [Folder Handlers](#51-kbfolder-command-handlers--13-tests)
   - 5.2 [Article Handlers](#52-kbarticle-command-handlers--20-tests)
   - 5.3 [AutoResolve, Attachment & Banner Handlers](#53-autoresolve-attachment--banner-handlers--33-tests)
6. [Key Design Decisions](#6-key-design-decisions)

---

## 1. Test Suite Summary

| Test Suite | File | Total | ✅ Pass | ❌ Fail | Pass % |
|---|---|:---:|:---:|:---:|:---:|
| PortalBanner Domain | `PortalBannerTests.cs` | 19 | 19 | 0 | 100% |
| KbArticle Domain | `KbArticleTests.cs` | 35 | 35 | 0 | 100% |
| KbFolder Domain | `KbFolderTests.cs` | 12 | 12 | 0 | 100% |
| Folder Validators | `KbValidatorTests.cs` | 15 | 15 | 0 | 100% |
| Article Validators | `KbValidatorTests.cs` | 8 | 8 | 0 | 100% |
| AutoResolve Validators | `KbValidatorTests.cs` | 16 | 16 | 0 | 100% |
| Attachment Validators | `KbValidatorTests.cs` | 13 | 13 | 0 | 100% |
| Banner Validators | `KbValidatorTests.cs` | 10 | 10 | 0 | 100% |
| Folder Handlers | `KbAllHandlerTests.cs` | 13 | 13 | 0 | 100% |
| Article Handlers | `KbAllHandlerTests.cs` | 20 | 20 | 0 | 100% |
| AutoResolve & Banner Handlers | `KbAllHandlerTests.cs` | 33 | 33 | 0 | 100% |
| **TOTAL** | | **194** | **194** | **0** | **100%** |

---

## 2. Architecture & Test Design

Tests follow the **Arrange-Act-Assert (AAA)** pattern throughout.

- **Domain tests** are fully isolated — no mocks needed because domain entities are pure C# classes with no infrastructure dependencies.
- **Handler tests** use **NSubstitute** to mock repository and service interfaces, verifying both the returned `Result<T>` and side-effects on domain objects.

### 2.1 Naming Convention

All test methods follow:

```
MethodUnderTest_Scenario_ExpectedOutcome        // domain & validator tests
Handle_Scenario_ExpectedOutcome                 // async handler tests
```

### 2.2 Shared Builders (Handler Tests)

A `file`-scoped static `Builders` class provides pre-built domain objects to keep handler tests concise:

| Builder | Returns |
|---|---|
| `Builders.Draft(folderId?)` | Unpublished `KbArticle` |
| `Builders.Published()` | Published `KbArticle` with cleared domain events |
| `Builders.Folder(isDeleted?)` | `KbFolder`, optionally soft-deleted |
| `Builders.Banner(isActive?)` | `PortalBanner`, optionally deactivated |

### 2.3 Result Pattern

Handlers return a `Result<T>` value object. Tests assert:
- `result.IsSuccess` → happy path
- `result.Error` (string) → failure path, often with a keyword assertion (e.g. `Contains("not found")`)

---

## 3. Domain Layer Tests

Domain tests verify pure business logic: entity factories, state transitions, guard clauses, and domain events. No repositories or infrastructure involved.

### 3.1 PortalBanner — 19 tests

> File: `KnowledgeBase/Domain/PortalBanner/PortalBannerTests.cs`
> Namespace: `Adrenalin.UnitTests.KnowledgeBase.Domain.PortalBannerTests`

Covers the full lifecycle of a time-boxed customer portal banner: creation with optional scheduling, whitespace trimming, all validation guards, activate/deactivate toggle, and `IsCurrentlyVisible` logic with half-open schedule windows.

| Method | Scenario | Outcome | Notes |
|---|---|:---:|---|
| `Create` | Valid args with schedule | ✅ Pass | Sets all props; `IsActive=true` by default |
| `Create` | Whitespace in title/message | ✅ Pass | Both fields trimmed |
| `Create` | No schedule (nulls) | ✅ Pass | `ActiveFrom`/`To` remain null |
| `Create` | `ActiveTo < ActiveFrom` | ❌ Throws `ArgumentException` | Schedule guard fires |
| `Create` | `ActiveTo == ActiveFrom` | ❌ Throws `ArgumentException` | Strict inequality required |
| `Create` | Blank title (whitespace) | ❌ Throws `ArgumentException` | Title guard fires |
| `Create` | Title > 200 chars | ❌ Throws `ArgumentException` | Length cap enforced |
| `Create` | Blank message | ❌ Throws `ArgumentException` | Message guard fires |
| `Update` | Valid new fields | ✅ Pass | All fields updated; `UpdatedAt` set |
| `Update` | Invalid schedule | ❌ Throws `ArgumentException` | Same guard as `Create` |
| `Deactivate` | Active banner | ✅ Pass | `IsActive=false`; `UpdatedBy`/`At` set |
| `Activate` | After deactivate | ✅ Pass | `IsActive` restored to true |
| `IsCurrentlyVisible` | Active, no schedule | ✅ Pass (true) | Always visible |
| `IsCurrentlyVisible` | Inactive banner | ✅ Pass (false) | Kill-switch overrides schedule |
| `IsCurrentlyVisible` | Before `ActiveFrom` | ✅ Pass (false) | Outside window |
| `IsCurrentlyVisible` | After `ActiveTo` | ✅ Pass (false) | Outside window |
| `IsCurrentlyVisible` | Within window | ✅ Pass (true) | Inside window |
| `IsCurrentlyVisible` | `ActiveFrom` only, past it | ✅ Pass (true) | Half-open range |
| `IsCurrentlyVisible` | `ActiveTo` only, before it | ✅ Pass (true) | Half-open range |

---

### 3.2 KbArticle — 35 tests

> File: `KnowledgeBase/Domain/KbArticle/KbArticleTests.cs`
> Namespace: `Adrenalin.UnitTests.KnowledgeBase.Domain.KbArticleTests`

Covers the article lifecycle (Draft → Published → Archived → Deleted), auto-resolution configuration, guardrail exclusion, match counters, attachment management, and domain event emission.

| Method | Scenario | Outcome | Notes |
|---|---|:---:|---|
| `Create` | Default values | ✅ Pass | Draft status, `AutoResolve=false`, counters = 0 |
| `Create` | Raises domain event | ✅ Pass | `KbArticleCreatedDomainEvent` emitted |
| `UpdateContent` | Changes title & body | ✅ Pass | `UpdatedBy`/`At` set |
| `UpdateContent` | Deleted article | ❌ Throws `InvalidOperationException` | Guard fires |
| `UpdateContent` | Archived article | ❌ Throws `InvalidOperationException` | Guard fires |
| `MoveToFolder` | Valid target folder | ✅ Pass | `FolderId` updated |
| `MoveToFolder` | Null target (root) | ✅ Pass | `FolderId` set to null |
| `MoveToFolder` | Deleted article | ❌ Throws `InvalidOperationException` | Guard fires |
| `Publish` | Draft → Published | ✅ Pass | `Status=Published`; `KbArticlePublishedDomainEvent` |
| `Publish` | Already published | ❌ Throws `InvalidOperationException` | Idempotency guard |
| `Publish` | Archived article | ❌ Throws `InvalidOperationException` | Invalid transition |
| `Publish` | Deleted article | ❌ Throws `InvalidOperationException` | Invalid transition |
| `Publish` | AutoResolve with valid keywords | ✅ Pass | Lifecycle check passes |
| `Archive` | Published article | ✅ Pass | `AutoResolve` disabled on archive |
| `Archive` | Already archived | ❌ Throws `InvalidOperationException` | Idempotency guard |
| `RestoreToDraft` | From Archived | ✅ Pass | `Status=Draft` |
| `RestoreToDraft` | From Draft | ❌ Throws `InvalidOperationException` | Invalid transition |
| `RestoreToDraft` | From Published | ❌ Throws `InvalidOperationException` | Invalid transition |
| `SoftDelete` | Existing article | ✅ Pass | `IsDeleted=true`; `AutoResolve` off; event raised |
| `SoftDelete` | Already deleted | ❌ Throws `InvalidOperationException` | Idempotency guard |
| `EnableAutoResolve` | All fields valid | ✅ Pass | Trims `ResolutionText`; sets threshold |
| `EnableAutoResolve` | Deleted article | ❌ Throws `InvalidOperationException` | Guard fires |
| `EnableAutoResolve` | GuardrailExcluded article | ❌ Throws `InvalidOperationException` | Guard fires |
| `EnableAutoResolve` | Empty keywords array | ❌ Throws `ArgumentException` | At least one keyword required |
| `EnableAutoResolve` | Blank resolution text | ❌ Throws `ArgumentException` | Text guard fires |
| `DisableAutoResolve` | After enable | ✅ Pass | `AutoResolve=false`; `UpdatedBy` set |
| `MarkAsGuardrailExcluded` | AutoResolve active | ✅ Pass | `GuardrailExcluded=true`; `AutoResolve` disabled |
| `RecordMatch` | Called twice | ✅ Pass | `TimesMatched` increments correctly |
| `RecordReopenedMatch` | After publish + enable | ✅ Pass | `Reopened++`; threshold raised; event raised |
| `RecordReopenedMatch` | Default delta | ✅ Pass | Default delta = `0.025` |
| `AddAttachment` | Valid URL/filename | ✅ Pass | Added to `Attachments` list |
| `AddAttachment` | Deleted article | ❌ Throws `InvalidOperationException` | Guard fires |
| `RemoveAttachment` | Existing attachment | ✅ Pass | `IsDeleted=true` on attachment |
| `RemoveAttachment` | Non-existent ID | ❌ Throws `InvalidOperationException` | Not found guard |
| `ClearDomainEvents` | After create | ✅ Pass | `DomainEvents` collection empty |

---

### 3.3 KbFolder — 12 tests

> File: `KnowledgeBase/Domain/KbFolder/KbFolderTests.cs`
> Namespace: `Adrenalin.UnitTests.KnowledgeBase.Domain.KbFolderTests`

Covers folder creation with depth enforcement, rename/reorder operations, soft-delete idempotency, and domain event emission.

| Method | Scenario | Outcome | Notes |
|---|---|:---:|---|
| `Create` | Valid name + parent + order | ✅ Pass | All props set; `IsDeleted=false` |
| `Create` | No parent (root) | ✅ Pass | `ParentId=null` |
| `Create` | Raises domain event | ✅ Pass | `KbFolderCreatedDomainEvent` emitted |
| `Create` | Parent at `MaxDepth` | ❌ Throws `InvalidOperationException` | Error message includes `MaxDepth` value |
| `Create` | Parent one below `MaxDepth` | ✅ Pass | Just within limit |
| `Create` | Null parent, any depth value | ✅ Pass | Depth guard skipped for root folders |
| `Rename` | Valid new name | ✅ Pass | Name updated; `UpdatedBy`/`At` set |
| `Rename` | Deleted folder | ❌ Throws `InvalidOperationException` | Guard fires |
| `Reorder` | New display order | ✅ Pass | `DisplayOrder` updated; `UpdatedBy` set |
| `SoftDelete` | Active folder | ✅ Pass | `IsDeleted=true`; `UpdatedBy`; event raised |
| `SoftDelete` | Already deleted | ❌ Throws `InvalidOperationException` | Idempotency guard |
| `ClearDomainEvents` | After create | ✅ Pass | `DomainEvents` collection empty |

---

## 4. Validator Tests

FluentValidation validators are tested in isolation by calling `.Validate()` directly — no handler infrastructure required. Failure cases verify the error is reported on the correct property name.

### 4.1 Folder Command Validators — 15 tests

| Command | Scenario | Outcome | Notes |
|---|---|:---:|---|
| `CreateKbFolderCommand` | Valid command | ✅ Valid | Name, optional parent, order ≥ 0 |
| `CreateKbFolderCommand` | Empty/whitespace name | ❌ Invalid | Name required |
| `CreateKbFolderCommand` | Name > `MaxLength` | ❌ Invalid | `FolderName.MaxLength` cap |
| `CreateKbFolderCommand` | `DisplayOrder < 0` | ❌ Invalid | Must be ≥ 0; error on `DisplayOrder` |
| `CreateKbFolderCommand` | `DisplayOrder = 0` | ✅ Valid | Zero is allowed |
| `RenameKbFolderCommand` | Valid command | ✅ Valid | Non-empty name, valid IDs |
| `RenameKbFolderCommand` | Empty `FolderId` (`Guid.Empty`) | ❌ Invalid | ID required |
| `RenameKbFolderCommand` | Empty new name | ❌ Invalid | Name required |
| `RenameKbFolderCommand` | Empty `ActorId` | ❌ Invalid | Actor required |
| `ReorderKbFolderCommand` | Valid command | ✅ Valid | Order ≥ 0 |
| `ReorderKbFolderCommand` | Negative order | ❌ Invalid | Must be ≥ 0 |
| `ReorderKbFolderCommand` | Empty `FolderId` | ❌ Invalid | ID required |
| `DeleteKbFolderCommand` | Valid command | ✅ Valid | Both IDs present |
| `DeleteKbFolderCommand` | Empty `FolderId` | ❌ Invalid | ID required |
| `DeleteKbFolderCommand` | Empty `ActorId` | ❌ Invalid | Actor required |

---

### 4.2 Article Command Validators — 8 tests

| Command | Scenario | Outcome | Notes |
|---|---|:---:|---|
| `CreateKbArticleCommand` | Valid command (`Faq`) | ✅ Valid | Title + valid enum |
| `CreateKbArticleCommand` | Empty/whitespace title | ❌ Invalid | Title required |
| `CreateKbArticleCommand` | Title > `MaxLength` | ❌ Invalid | `ArticleTitle.MaxLength` cap; error on `Title` |
| `CreateKbArticleCommand` | Unknown `ArticleType` (99) | ❌ Invalid | Enum range enforced |
| `UpdateKbArticleCommand` | Valid command | ✅ Valid | ID + non-empty title |
| `UpdateKbArticleCommand` | Empty `ArticleId` | ❌ Invalid | ID required |
| `UpdateKbArticleCommand` | Empty title | ❌ Invalid | Title required |
| `UpdateKbArticleCommand` | Title > `MaxLength` | ❌ Invalid | Length cap enforced |

---

### 4.3 AutoResolve Command Validators — 16 tests

> `ConfidenceThreshold` boundary values (`Minimum` / `Maximum` constants) are tested with `[Theory]`/`[InlineData]` covering inclusive pass and exclusive fail on both sides.

| Command | Scenario | Outcome | Notes |
|---|---|:---:|---|
| `EnableAutoResolveCommand` | Valid (keywords + text + threshold 0.75) | ✅ Valid | All fields in range |
| `EnableAutoResolveCommand` | Empty keywords array | ❌ Invalid | Error on `Keywords` |
| `EnableAutoResolveCommand` | Blank keyword entry | ❌ Invalid | Each keyword must be non-empty |
| `EnableAutoResolveCommand` | Keyword > 100 chars | ❌ Invalid | Per-keyword length cap |
| `EnableAutoResolveCommand` | Empty resolution text | ❌ Invalid | Error on `ResolutionText` |
| `EnableAutoResolveCommand` | Threshold below minimum | ❌ Invalid | Exclusive lower boundary |
| `EnableAutoResolveCommand` | Threshold above maximum | ❌ Invalid | Exclusive upper boundary |
| `EnableAutoResolveCommand` | Threshold = minimum | ✅ Valid | Inclusive lower boundary |
| `EnableAutoResolveCommand` | Threshold = maximum | ✅ Valid | Inclusive upper boundary |
| `EnableAutoResolveCommand` | Threshold = 0.75 | ✅ Valid | Mid-range value |
| `RecordArticleReopenedMatchCommand` | Valid (delta = 0.025) | ✅ Valid | Positive, within 0.1 cap |
| `RecordArticleReopenedMatchCommand` | Delta = 0 | ❌ Invalid | Must be positive |
| `RecordArticleReopenedMatchCommand` | Negative delta | ❌ Invalid | Must be positive |
| `RecordArticleReopenedMatchCommand` | Delta > 0.1 (0.11) | ❌ Invalid | Cap enforced |
| `RecordArticleReopenedMatchCommand` | Delta = 0.1 (max) | ✅ Valid | Inclusive upper boundary |
| `RecordArticleReopenedMatchCommand` | Empty `ArticleId` | ❌ Invalid | ID required |

---

### 4.4 Attachment Command Validators — 13 tests

> File size limit is **50 MB (52,428,800 bytes)**. Both sides of the boundary are tested. Allowed MIME types are white-listed; an executable (`application/x-msdownload`) is used as the negative example.

| Command | Scenario | Outcome | Notes |
|---|---|:---:|---|
| `AddAttachmentCommand` | Valid PDF, size, MIME | ✅ Valid | All fields within limits |
| `AddAttachmentCommand` | Empty `ArticleId` | ❌ Invalid | ID required |
| `AddAttachmentCommand` | Empty file name | ❌ Invalid | Name required |
| `AddAttachmentCommand` | File name > 255 chars | ❌ Invalid | Length cap |
| `AddAttachmentCommand` | Null `FileStream` | ❌ Invalid | Error on `FileStream` |
| `AddAttachmentCommand` | Size > 50 MB (52,428,801) | ❌ Invalid | Over the limit |
| `AddAttachmentCommand` | Size = 50 MB exactly (52,428,800) | ✅ Valid | Inclusive boundary |
| `AddAttachmentCommand` | Size = 0 | ❌ Invalid | Must be positive |
| `AddAttachmentCommand` | Size = null | ✅ Valid | Size is optional |
| `AddAttachmentCommand` | Disallowed MIME (exe) | ❌ Invalid | Error on `MimeType` |
| `AddAttachmentCommand` | Allowed MIMEs (pdf/png/csv/zip) | ✅ Valid | All 4 types pass |
| `AddAttachmentCommand` | Null MIME | ✅ Valid | MIME is optional |
| `RemoveAttachmentCommand` | Valid command | ✅ Valid | Both IDs present |
| `RemoveAttachmentCommand` | Empty `ArticleId` | ❌ Invalid | ID required |
| `RemoveAttachmentCommand` | Empty `AttachmentId` | ❌ Invalid | ID required |

---

### 4.5 Portal Banner Command Validators — 10 tests

| Command | Scenario | Outcome | Notes |
|---|---|:---:|---|
| `CreatePortalBannerCommand` | Valid, no schedule | ✅ Valid | Title + message; nulls OK |
| `CreatePortalBannerCommand` | Valid, with schedule | ✅ Valid | `ActiveFrom < ActiveTo` |
| `CreatePortalBannerCommand` | Empty title | ❌ Invalid | Required |
| `CreatePortalBannerCommand` | Title > 200 chars | ❌ Invalid | Length cap |
| `CreatePortalBannerCommand` | Empty message | ❌ Invalid | Required |
| `CreatePortalBannerCommand` | `ActiveTo < ActiveFrom` | ❌ Invalid | Error message contains `"active_to"` |
| `CreatePortalBannerCommand` | `ActiveTo == ActiveFrom` | ❌ Invalid | Strict inequality required |
| `UpdatePortalBannerCommand` | Valid command | ✅ Valid | ID + title + message |
| `UpdatePortalBannerCommand` | Empty `BannerId` | ❌ Invalid | ID required |
| `UpdatePortalBannerCommand` | `ActiveTo < ActiveFrom` | ❌ Invalid | Schedule guard |

---

## 5. Handler Tests

Handler tests use **NSubstitute** mocks for all repository and service interfaces. Each test class inherits a base class that wires up the mocks, keeping setup DRY. Tests verify both the `Result` value returned and side-effects on the domain entity.

### 5.1 KbFolder Command Handlers — 13 tests

> File: `KnowledgeBase/Application/Handlers/KbAllHandlerTests.cs`

| Handler | Scenario | Outcome | Assertions / Notes |
|---|---|:---:|---|
| `CreateKbFolderCommandHandler` | No parent | ✅ Pass | Folder created; `repo.Add` called |
| `CreateKbFolderCommandHandler` | Valid parent | ✅ Pass | Depth queried; success |
| `CreateKbFolderCommandHandler` | Parent not found | ❌ Failure | Error contains `"not found"` |
| `CreateKbFolderCommandHandler` | Parent is deleted | ❌ Failure | Error contains `"deleted"` |
| `CreateKbFolderCommandHandler` | Exceeds `MaxDepth` | ❌ Failure | Domain exception propagated |
| `RenameKbFolderCommandHandler` | Folder exists | ✅ Pass | Name updated; `repo.Update` called |
| `RenameKbFolderCommandHandler` | Folder not found | ❌ Failure | Error contains `"not found"` |
| `RenameKbFolderCommandHandler` | Folder is deleted | ❌ Failure | Domain exception propagated |
| `ReorderKbFolderCommandHandler` | Folder exists | ✅ Pass | `DisplayOrder = 5` confirmed |
| `ReorderKbFolderCommandHandler` | Folder not found | ❌ Failure | — |
| `DeleteKbFolderCommandHandler` | Empty folder | ✅ Pass | `IsDeleted=true`; `SaveChanges` called |
| `DeleteKbFolderCommandHandler` | Folder has articles | ❌ Failure | Error contains `"articles"` |
| `DeleteKbFolderCommandHandler` | Folder not found | ❌ Failure | — |

---

### 5.2 KbArticle Command Handlers — 20 tests

| Handler | Scenario | Outcome | Assertions / Notes |
|---|---|:---:|---|
| `CreateKbArticleCommandHandler` | No folder | ✅ Pass | Article created; `repo.Add` called; ID returned |
| `CreateKbArticleCommandHandler` | Valid folder | ✅ Pass | Folder looked up; article created |
| `CreateKbArticleCommandHandler` | Folder not found | ❌ Failure | Error contains `"not found"` |
| `CreateKbArticleCommandHandler` | Folder is deleted | ❌ Failure | Error contains `"deleted"` |
| `UpdateKbArticleCommandHandler` | Article exists | ✅ Pass | Title updated in entity |
| `UpdateKbArticleCommandHandler` | Article not found | ❌ Failure | — |
| `UpdateKbArticleCommandHandler` | Archived article | ❌ Failure | Domain exception propagated |
| `MoveKbArticleCommandHandler` | Valid target folder | ✅ Pass | `FolderId = targetId` |
| `MoveKbArticleCommandHandler` | Null target (root) | ✅ Pass | `FolderId = null` |
| `MoveKbArticleCommandHandler` | Target folder deleted | ❌ Failure | — |
| `PublishKbArticleCommandHandler` | Draft article | ✅ Pass | `Status = Published` |
| `PublishKbArticleCommandHandler` | Already published | ❌ Failure | Domain exception propagated |
| `PublishKbArticleCommandHandler` | Article not found | ❌ Failure | — |
| `ArchiveKbArticleCommandHandler` | Draft article | ✅ Pass | `Status = Archived` |
| `ArchiveKbArticleCommandHandler` | Already archived | ❌ Failure | Domain exception propagated |
| `RestoreKbArticleToDraftCommandHandler` | Archived article | ✅ Pass | `Status = Draft` |
| `RestoreKbArticleToDraftCommandHandler` | Draft article | ❌ Failure | Domain exception propagated |
| `DeleteKbArticleCommandHandler` | Existing article | ✅ Pass | `IsDeleted=true`; `repo.Update` called |
| `DeleteKbArticleCommandHandler` | Article not found | ❌ Failure | — |
| `DeleteKbArticleCommandHandler` | Already deleted | ❌ Failure | Domain exception propagated |

---

### 5.3 AutoResolve, Attachment & Banner Handlers — 33 tests

> **Note on `AddAttachmentCommandHandler`:** tests explicitly verify `repo.AddAttachment()` is called (not `Update`), and that `repo.Update()` is **not** called for attachment removal — confirming correct EF change-tracking usage.

| Handler | Scenario | Outcome | Assertions / Notes |
|---|---|:---:|---|
| `EnableAutoResolveCommandHandler` | Published article | ✅ Pass | `AutoResolve=true` confirmed |
| `EnableAutoResolveCommandHandler` | Draft article | ❌ Failure | Error contains `"Published"` |
| `EnableAutoResolveCommandHandler` | Article not found | ❌ Failure | — |
| `DisableAutoResolveCommandHandler` | Article with AutoResolve on | ✅ Pass | `AutoResolve=false` |
| `DisableAutoResolveCommandHandler` | Article not found | ❌ Failure | — |
| `MarkAsGuardrailExcludedCommandHandler` | Existing article | ✅ Pass | `GuardrailExcluded=true` |
| `MarkAsGuardrailExcludedCommandHandler` | Article not found | ❌ Failure | — |
| `RecordArticleReopenedMatchCommandHandler` | Published article | ✅ Pass | `TimesReopened = 1` |
| `RecordArticleReopenedMatchCommandHandler` | Article not found | ❌ Failure | — |
| `RecordArticleMatchCommandHandler` | Published article | ✅ Pass | `TimesMatched = 1` |
| `RecordArticleMatchCommandHandler` | Article not found | ❌ Failure | — |
| `AddAttachmentCommandHandler` | Valid article + file storage | ✅ Pass | URL stored; `repo.AddAttachment` called; ID returned |
| `AddAttachmentCommandHandler` | Article not found | ❌ Failure | Error contains `"not found"` |
| `RemoveAttachmentCommandHandler` | Valid attachment | ✅ Pass | `IsDeleted=true`; `repo.Update` NOT called |
| `RemoveAttachmentCommandHandler` | Attachment not found | ❌ Failure | — |
| `RemoveAttachmentCommandHandler` | Article not found | ❌ Failure | — |
| `CreatePortalBannerCommandHandler` | Valid command | ✅ Pass | ID returned; `repo.Add` called |
| `CreatePortalBannerCommandHandler` | Invalid schedule | ❌ Failure | Domain guard propagated |
| `UpdatePortalBannerCommandHandler` | Banner exists | ✅ Pass | Title updated in entity |
| `UpdatePortalBannerCommandHandler` | Banner not found | ❌ Failure | — |
| `ActivatePortalBannerCommandHandler` | Inactive banner | ✅ Pass | `IsActive=true` |
| `ActivatePortalBannerCommandHandler` | Banner not found | ❌ Failure | — |
| `DeactivatePortalBannerCommandHandler` | Active banner | ✅ Pass | `IsActive=false` |
| `DeactivatePortalBannerCommandHandler` | Banner not found | ❌ Failure | — |

---

## 6. Key Design Decisions

### 6.1 Namespace Isolation

Test namespaces use the suffix `...Tests` (e.g. `Domain.PortalBannerTests`) to prevent the C# compiler from confusing the namespace segment with the domain class of the same name, which caused 20 `CS0234` errors where `PortalBanner.Create(...)` was unresolvable.

### 6.2 Explicit `Action` Cast for `Assert.Throws`

Where xUnit's overload resolution would otherwise pick the obsolete `Func<Task>` overload, the lambda is explicitly cast to `(Action)`:

```csharp
Assert.Throws<ArgumentException>((Action)(() =>
    banner.Update("T", "M", Now.AddHours(5), Now.AddHours(1), null)));
```

This suppresses `CS0619` without changing test semantics.

### 6.3 Null-Forgiving on Nullable Collections

`Assert.Equal()` comparisons against nullable arrays (e.g. `a.Keywords`) use the null-forgiving operator where the test context guarantees non-null:

```csharp
Assert.Equal(["login", "password"], a.Keywords!);
```

This resolves `CS8604` without suppressing warnings project-wide.

### 6.4 `ArticleType` Enum Alignment with DB Schema

The `ArticleType` enum is kept in strict sync with the database `CHECK` constraint:

```sql
CHECK (article_type IN ('faq', 'release_note', 'user_manual', 'patch', 'process_flow'))
```

No additional enum values are added unless the schema is updated first — ensuring tests never exercise a value that would fail a DB write.

---

*Generated from `Adrenalin.UnitTests` — 194/194 tests passing · .NET 10 · xUnit 3.1.4*
