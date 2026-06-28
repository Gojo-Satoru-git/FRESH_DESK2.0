# RBAC Frontend Integration Guide

**Project:** Adrenalin Unify Platform  
**Module:** Role-Based Access Control (RBAC)  
**Base URLs:** `/api/rbac` (Access Levels & Permissions) · `/api/workflow-roles` (Workflow Roles)  
**Audience:** Frontend Developers · QA Engineers · API Consumers  
**Document Version:** 2.0.0

---

## Table of Contents

1. [Module Overview](#1-module-overview)
2. [API Endpoints — Roles (Access Levels)](#2-api-endpoints--roles-access-levels)
3. [API Endpoints — Permissions](#3-api-endpoints--permissions)
4. [API Endpoints — Role Permissions](#4-api-endpoints--role-permissions)
5. [API Endpoints — User Roles](#5-api-endpoints--user-roles)
6. [API Endpoints — Workflow Roles](#6-api-endpoints--workflow-roles)
7. [Entity Documentation](#7-entity-documentation)
8. [TypeScript Models](#8-typescript-models)
9. [Frontend API Service Examples](#9-frontend-api-service-examples)
10. [Search, Filtering and Pagination](#10-search-filtering-and-pagination)
11. [Error Handling Guide](#11-error-handling-guide)
12. [API Flow Diagrams](#12-api-flow-diagrams)
13. [Authentication & Authorization Integration](#13-authentication--authorization-integration)
14. [Testing Scenarios](#14-testing-scenarios)
15. [Frontend Integration Checklist](#15-frontend-integration-checklist)
16. [Common Frontend Workflows](#16-common-frontend-workflows)

---

## 1. Module Overview

### Purpose

The RBAC module for the Adrenalin Unify Platform provides a complete Role-Based Access Control system. It manages **two distinct role concepts**:

- **Roles (Access Levels)** — Control what a user is *permitted to do* (system-wide permissions, stamped into the JWT at login).
- **Workflow Roles** — Control which *workflow stages* an agent is eligible to handle. These are operational routing roles and are **not** JWT-embedded.

### Features

**Access Levels (Roles)**
- Create, update, clone, and soft-delete **Roles**
- Create and soft-delete **Permissions** (defined as `resource:action` pairs)
- Grant or revoke individual permissions from a role, or replace the full permission set atomically
- Copy an entire permission matrix from one role to another
- View an effective-permissions summary grouped by module
- Assign or remove a role from a user, or set a user's Access Level atomically
- Query users with pagination and email/active-status filtering
- Retrieve a user's effective permissions (the full resolved list across all roles)
- All destructive operations are **soft-deletes** — records are never physically removed

**Workflow Roles**
- Create, rename, deactivate, reactivate, and delete **Workflow Roles**
- Assign a Primary Workflow Role and zero or more Additional Workflow Roles to an agent (atomic set operation)
- Preview which workflow stages an agent would be eligible for given a proposed role combination

### User Flow

```
User logs in
     ↓
Server resolves user → access-level roles → permissions
     ↓
JWT is issued with:
  - sub  (userId)
  - email
  - role[] claims         ← Access Level role names
  - permission[] claims   ← e.g. "role:read", "ticket:create"
     ↓
Frontend stores JWT
     ↓
Every API request carries:  Authorization: Bearer <JWT>
     ↓
Server validates JWT and checks "permission" claims
     ↓
Access granted or 403 returned
```

> **Workflow Roles are not in the JWT.** They affect routing/stage eligibility at runtime via the Workflow engine — not endpoint authorization.

### Authentication Model

The platform uses **JWT Bearer authentication** (HMAC-SHA256 signed). Tokens are issued at login and contain:

| Claim | Type | Description |
|-------|------|-------------|
| `sub` | string (GUID) | User's unique identifier |
| `email` | string | User's email address |
| `jti` | string (GUID) | Unique token identifier |
| `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` | string[] | One entry per assigned Access Level name |
| `permission` | string[] | One entry per resolved `resource:action` string |
| `exp` | Unix timestamp | Token expiry (configurable via `JwtOptions.ExpiryMinutes`) |

Permissions are **stamped into the JWT at login time** by resolving the user's full role chain through `GetUserEffectivePermissionsQuery`. The JWT is self-contained — the server does not re-query the database on every request.

### Authorization Model

Authorization uses a **dynamic policy provider** pattern. Every endpoint is decorated with:

```csharp
[Authorize(Policy = "resource:action")]
```

The `PermissionPolicyProvider` intercepts any policy name containing a colon and dynamically creates an `AuthorizationPolicy` backed by a `PermissionRequirement`. The `PermissionAuthorizationHandler` then checks whether the authenticated user's JWT contains a `permission` claim matching the required `resource:action` string.

This means **any** `resource:action` string can be used as a policy without pre-registration. The system is fully dynamic.

> **Important:** Permission strings on endpoints use the short form (`role:read`, `role:write`, `user:manage`) — **not** the `rbac:` prefix that may appear in older documentation. Always confirm against the live seed data if in doubt:
> ```sql
> SELECT resource, action FROM auth.permissions ORDER BY resource, action;
> ```

### RBAC Entity Relationship Diagram

```
┌─────────────┐        ┌──────────────────┐        ┌──────────────────┐
│    User     │        │    UserRole       │        │      Role        │
│─────────────│        │──────────────────│        │──────────────────│
│ Id (PK)     │1──────<│ UserId (FK)       │>──────1│ Id (PK)          │
│ Email       │        │ RoleId (FK)       │        │ Name             │
│ FirstName   │        │ AssignedAt        │        │ Description      │
│ LastName    │        │ AssignedBy        │        │ IsSystemRole     │
│ IsActive    │        │ IsDeleted         │        │ IsDeleted        │
│ ...         │        └──────────────────┘        │ CreatedAt        │
└──────┬──────┘                                    │ UpdatedAt        │
       │                                           └──────────────────┘
       │                                                    │ 1
       │                                                    │
       │                                                   <│
       │ 1                                         ┌──────────────────┐
       │                                           │  RolePermission  │
       │                                           │──────────────────│
       │                                           │ RoleId (FK)      │
       │                                           │ PermissionId (FK)│
       │                                           │ IsDeleted        │
       │                                           └──────────────────┘
       │                                                    │>
       │                                                    │ 1
       │                                           ┌──────────────────┐
       │                                           │   Permission     │
       │                                           │──────────────────│
       │                                           │ Id (PK)          │
       │                                           │ Resource         │
       │                                           │ Action           │
       │                                           │ Description      │
       │                                           │ IsDeleted        │
       │                                           └──────────────────┘
       │
       │ 1      ┌──────────────────────┐       ┌──────────────────────┐
       └───────<│  UserWorkflowRole    │>─────1│    WorkflowRole      │
                │──────────────────────│       │──────────────────────│
                │ UserId (FK)          │       │ Id (PK)              │
                │ WorkflowRoleId (FK)  │       │ Name                 │
                │ IsPrimary            │       │ Description          │
                │ IsDeleted            │       │ IsActive             │
                └──────────────────────┘       │ IsSystemDefault      │
                                               │ CreatedAt            │
                                               └──────────────────────┘
```

---

## 2. API Endpoints — Roles (Access Levels)

**Base route:** `/api/rbac/roles`  
**Controller:** `RolesController`

### 2.1 Get All Roles

**HTTP Method:** `GET`  
**Route:** `/api/rbac/roles`  
**Authorization Requirement:** `role:read`

**Success Response:** `200 OK` — Array of `RoleDto`

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Support Agent",
    "description": "Can view and respond to tickets",
    "isSystemRole": false,
    "createdAt": "2026-01-15T09:00:00Z",
    "updatedAt": null
  }
]
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Internal failure returning the list |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | JWT does not contain `role:read` permission |

**Frontend Usage Notes:**
- Use this to populate role dropdowns, role management tables, and user-assignment UI.
- System roles (`isSystemRole: true`) should be displayed as read-only; hide deletion controls for them.

---

### 2.2 Get Role by ID

**HTTP Method:** `GET`  
**Route:** `/api/rbac/roles/{id}`  
**Authorization Requirement:** `role:read`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The role's unique identifier |

**Success Response:** `200 OK` — Single `RoleDto`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Support Agent",
  "description": "Can view and respond to tickets",
  "isSystemRole": false,
  "createdAt": "2026-01-15T09:00:00Z",
  "updatedAt": null
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:read` permission |
| `404 Not Found` | No role found with the given ID |

---

### 2.3 Get Role With Permissions

**HTTP Method:** `GET`  
**Route:** `/api/rbac/roles/{id}/permissions`  
**Authorization Requirement:** `role:read`

**Success Response:** `200 OK` — `RoleWithPermissionsDto`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Support Agent",
  "description": "Can view and respond to tickets",
  "isSystemRole": false,
  "permissions": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
      "resource": "ticket",
      "action": "read",
      "description": "View tickets"
    }
  ],
  "createdAt": "2026-01-15T09:00:00Z",
  "updatedAt": null
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:read` permission |
| `404 Not Found` | Role not found |

---

### 2.4 Create Role

**HTTP Method:** `POST`  
**Route:** `/api/rbac/roles`  
**Authorization Requirement:** `role:write`

**Request Body:** `CreateRoleRequest`

```json
{
  "name": "Billing Manager",
  "description": "Manages billing and invoicing operations"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `name` | string | **Yes** | Non-empty, max 80 characters, must be unique (case-insensitive) |
| `description` | string | No | Max 500 characters |

**Success Response:** `201 Created`

```json
{ "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7" }
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Validation failure or duplicate role name |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:write` permission |

---

### 2.5 Update Role

**HTTP Method:** `PUT`  
**Route:** `/api/rbac/roles/{id}`  
**Authorization Requirement:** `role:write`

**Request Body:** `UpdateRoleRequest`

```json
{
  "name": "Senior Billing Manager",
  "description": "Updated description"
}
```

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Validation failure, role not found, duplicate name, or role is deleted |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:write` permission |

---

### 2.6 Delete Role

**HTTP Method:** `DELETE`  
**Route:** `/api/rbac/roles/{id}`  
**Authorization Requirement:** `role:write`

Soft-deletes a role and cascades soft-deletion to all associated `UserRole` records. System roles cannot be deleted.

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role not found, already deleted, or is a system role |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:write` permission |

---

### 2.7 Clone Role *(new)*

**HTTP Method:** `POST`  
**Route:** `/api/rbac/roles/{id}/clone`  
**Authorization Requirement:** `role:write`

Creates a new role that is a copy of the source role, including its entire permission matrix (FR-RP-023 / BR-RP-010).

**Request Body:** `CloneRoleRequest`

```json
{
  "newRoleName": "Support Agent (APAC)",
  "newRoleDescription": "Clone of Support Agent for APAC region"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `newRoleName` | string | **Yes** | Non-empty, max 80 characters, must be unique |
| `newRoleDescription` | string | No | Max 500 characters |

**Success Response:** `201 Created`

```json
{ "id": "9f1a2b3c-4d5e-6f7a-8b9c-0d1e2f3a4b5c" }
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Source role not found, or duplicate target name |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:write` permission |

**Frontend Usage Notes:**
- After a `201`, navigate to the new role's permissions screen using the returned `id`.
- Use this instead of manually re-assigning permissions when creating similar roles.

---

### 2.8 Copy Permissions From Another Role *(new)*

**HTTP Method:** `POST`  
**Route:** `/api/rbac/roles/{id}/copy-permissions-from`  
**Authorization Requirement:** `role:write`

Overwrites `{id}`'s entire permission set with the permission set of `sourceRoleId`. **Destructive** — all existing permissions on the target role are replaced. The FSD requires a client-side confirmation step before calling this endpoint (FR-RP-033).

**Request Body:** `CopyPermissionsFromRequest`

```json
{
  "sourceRoleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `sourceRoleId` | GUID | **Yes** | Must reference an existing, non-deleted role |

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Source or target role not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:write` permission |

**Frontend Usage Notes:**
- Always show a confirmation dialog: *"This will overwrite [Target Role]'s permissions with those from [Source Role]. Continue?"*
- After `204`, refresh the role's permissions view.

---

### 2.9 Get Effective Permissions Summary *(new)*

**HTTP Method:** `GET`  
**Route:** `/api/rbac/roles/{id}/effective-permissions-summary`  
**Authorization Requirement:** `role:read`

Returns a plain-English summary of the role's resolved permissions, grouped by module (FR-RP-035 / FR-RP-043). Useful for displaying a human-readable overview on the role detail screen.

**Success Response:** `200 OK` — `EffectivePermissionsSummaryDto`

```json
{
  "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "roleName": "Support Agent",
  "modules": [
    {
      "module": "ticket",
      "permissions": ["read", "create", "update"]
    },
    {
      "module": "kb",
      "permissions": ["read"]
    }
  ]
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:read` permission |
| `404 Not Found` | Role not found |

---

## 3. API Endpoints — Permissions

**Base route:** `/api/rbac/permissions`  
**Controller:** `PermissionsController`

### 3.1 Get All Permissions

**HTTP Method:** `GET`  
**Route:** `/api/rbac/permissions`  
**Authorization Requirement:** `permission:read`

**Success Response:** `200 OK` — Array of `PermissionDto`

```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
    "resource": "ticket",
    "action": "read",
    "description": "View tickets"
  }
]
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Internal failure |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `permission:read` permission |

---

### 3.2 Get Permissions by Role

**HTTP Method:** `GET`  
**Route:** `/api/rbac/permissions/by-role/{roleId}`  
**Authorization Requirement:** `permission:read`

Returns all active permissions assigned to a specific role.

**Success Response:** `200 OK` — Array of `PermissionDto`

---

### 3.3 Create Permission

**HTTP Method:** `POST`  
**Route:** `/api/rbac/permissions`  
**Authorization Requirement:** `permission:write`

**Request Body:** `CreatePermissionRequest`

```json
{
  "resource": "report",
  "action": "export",
  "description": "Export reports to CSV or PDF"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `resource` | string | **Yes** | Non-empty, max 60 chars, pattern: `^[a-z_:]+$` |
| `action` | string | **Yes** | Non-empty, max 60 chars, pattern: `^[a-z_]+$` |
| `description` | string | No | Optional |

**Success Response:** `201 Created` — `{ "id": "..." }`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Validation failure or duplicate `resource:action` |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `permission:write` permission |

---

### 3.4 Delete Permission

**HTTP Method:** `DELETE`  
**Route:** `/api/rbac/permissions/{id}`  
**Authorization Requirement:** `permission:write`

Soft-deletes a permission and cascades soft-deletion to all `RolePermission` records that reference it.

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Permission not found or already deleted |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `permission:write` permission |

---

## 4. API Endpoints — Role Permissions

### 4.1 Grant Permission to Role

**HTTP Method:** `POST`  
**Route:** `/api/rbac/roles/{id}/permissions/grant`  
**Authorization Requirement:** `role:write`

Assigns a single permission to a role. **Idempotent** — granting an already-assigned permission returns `204` without error.

**Request Body:** `PermissionIdRequest`

```json
{ "permissionId": "a1b2c3d4-e5f6-7890-abcd-ef0123456789" }
```

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role or permission not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:write` permission |

---

### 4.2 Revoke Permission from Role

**HTTP Method:** `POST`  
**Route:** `/api/rbac/roles/{id}/permissions/revoke`  
**Authorization Requirement:** `role:write`

Removes a single permission from a role (soft-delete of the `RolePermission` record).

**Request Body:** `PermissionIdRequest`

```json
{ "permissionId": "a1b2c3d4-e5f6-7890-abcd-ef0123456789" }
```

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Assignment not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:write` permission |

---

### 4.3 Set Role Permissions (Atomic Replace)

**HTTP Method:** `PUT`  
**Route:** `/api/rbac/roles/{id}/permissions`  
**Authorization Requirement:** `role:write`

Atomically replaces the entire permission set of a role. Sending an empty array removes all permissions.

**Request Body:** `SetPermissionsRequest`

```json
{
  "permissionIds": [
    "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
    "b2c3d4e5-f6a7-8901-bcde-f01234567890"
  ]
}
```

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role not found or any permission ID does not exist |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `role:write` permission |

---

## 5. API Endpoints — User Roles

**Base route:** `/api/rbac/users`  
**Controller:** `UsersRbacController`

### 5.1 Get All Users (with Filtering & Pagination)

**HTTP Method:** `GET`  
**Route:** `/api/rbac/users`  
**Authorization Requirement:** `user:manage`

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `email` | string | No | null | Substring filter on email |
| `isActive` | boolean | No | null | Filter by active status |
| `pageNumber` | integer | No | 1 | Page number (1-based) |
| `pageSize` | integer | No | 20 | Items per page |

**Success Response:** `200 OK` — `PagedResultDto<UserSummaryDto>`

```json
{
  "items": [
    {
      "id": "5e6f7a8b-9c0d-1234-ef01-234567890123",
      "email": "alice@company.com",
      "firstName": "Alice",
      "lastName": "Wong",
      "isActive": true
    }
  ],
  "totalCount": 142,
  "pageNumber": 1,
  "pageSize": 20
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Internal failure |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `user:manage` permission |

---

### 5.2 Get User With Roles

**HTTP Method:** `GET`  
**Route:** `/api/rbac/users/{id}/roles`  
**Authorization Requirement:** `user:manage`

**Success Response:** `200 OK` — `UserWithRolesDto`

```json
{
  "id": "5e6f7a8b-9c0d-1234-ef01-234567890123",
  "email": "alice@company.com",
  "firstName": "Alice",
  "lastName": "Wong",
  "isActive": true,
  "roles": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Support Agent",
      "isSystemRole": false
    }
  ],
  "createdAt": "2026-01-10T08:00:00Z",
  "lastLoginAt": "2026-06-06T14:23:00Z"
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `user:manage` permission |
| `404 Not Found` | User not found |

---

### 5.3 Get User Effective Permissions

**HTTP Method:** `GET`  
**Route:** `/api/rbac/users/{id}/permissions`  
**Authorization Requirement:** `user:manage`

Returns the full resolved list of `resource:action` permission strings for a user.

**Success Response:** `200 OK` — `string[]`

```json
["ticket:read", "ticket:create", "role:read"]
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `user:manage` permission |
| `404 Not Found` | User not found |

---

### 5.4 Get User Groups

**HTTP Method:** `GET`  
**Route:** `/api/rbac/users/{id}/groups`  
**Authorization Requirement:** `user:manage`

Returns all groups the user currently belongs to.

**Success Response:** `200 OK` — `GroupDto[]`

---

### 5.5 Assign Role to User

**HTTP Method:** `POST`  
**Route:** `/api/rbac/users/{id}/roles/assign`  
**Authorization Requirement:** `user:manage`

Assigns a role to a user. **Idempotent** — assigning an already-active role returns `204` without duplication.

**Request Body:** `RoleIdRequest`

```json
{ "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | User or role not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `user:manage` permission |

---

### 5.6 Remove Role from User

**HTTP Method:** `POST`  
**Route:** `/api/rbac/users/{id}/roles/remove`  
**Authorization Requirement:** `user:manage`

Removes (soft-deletes) a role assignment from a user.

**Request Body:** `RoleIdRequest`

```json
{ "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Assignment not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `user:manage` permission |

---

### 5.7 Set User Access Level (Atomic)

**HTTP Method:** `PUT`  
**Route:** `/api/rbac/users/{id}/access-level`  
**Authorization Requirement:** `user:manage`

Atomically sets the user's Access Level (primary role). This replaces the previous `PUT /users/{id}/roles` — the body shape has changed from `roleIds[]` to a single `accessLevelId`.

**Request Body:** `SetRolesRequest`

```json
{ "accessLevelId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `accessLevelId` | GUID | **Yes** | Must reference an existing, non-deleted role |

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | User or role not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `user:manage` permission |

**⚠️ Breaking Change from v1:** The old `PUT /api/rbac/users/{id}/roles` endpoint with a `roleIds` array no longer exists. Use this endpoint instead with a single `accessLevelId`.

---

## 6. API Endpoints — Workflow Roles

**Base route:** `/api/workflow-roles`  
**Controller:** `WorkflowRolesController`

Workflow Roles are operational roles that determine which workflow stages an agent is eligible to handle. They are **separate** from Access Level roles and are not embedded in the JWT.

**Permission strings for this section:** `workflowrole:read` and `workflowrole:write`.

> **Note:** Confirm these strings exist in the permissions seed data before use:
> ```sql
> SELECT resource, action FROM auth.permissions WHERE resource = 'workflowrole';
> ```

---

### 6.1 List Workflow Roles

**HTTP Method:** `GET`  
**Route:** `/api/workflow-roles`  
**Authorization Requirement:** `workflowrole:read`

Returns all Workflow Roles, optionally filtered.

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `isActive` | boolean | No | null | Filter by active status |
| `search` | string | No | null | Substring filter on name |

**Success Response:** `200 OK` — Array of `WorkflowRoleDto`

```json
[
  {
    "id": "aa01bb02-cc03-dd04-ee05-ff0011223344",
    "name": "Tier 1 Support",
    "description": "Handles first-line tickets",
    "isActive": true,
    "isSystemDefault": false,
    "agentsAssignedCount": 12,
    "stagesReferencingCount": 3,
    "lastModified": "2026-05-10T08:00:00Z"
  }
]
```

**`WorkflowRoleDto` fields:**

| Field | Type | Description |
|-------|------|-------------|
| `id` | GUID | Unique identifier |
| `name` | string | Role name |
| `description` | string? | Optional description |
| `isActive` | bool | Whether the role is currently active (visible in pickers) |
| `isSystemDefault` | bool | If `true`, the role cannot be deleted |
| `agentsAssignedCount` | int | How many agents currently hold this role |
| `stagesReferencingCount` | int | How many workflow stages reference this role |
| `lastModified` | DateTimeOffset? | UTC timestamp of last modification |

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Internal failure |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `workflowrole:read` permission |

---

### 6.2 Create Workflow Role

**HTTP Method:** `POST`  
**Route:** `/api/workflow-roles`  
**Authorization Requirement:** `workflowrole:write`

Creates a new Workflow Role. Status defaults to **Active**.

**Request Body:** `CreateWorkflowRoleRequest`

```json
{
  "name": "Escalation Handler",
  "description": "Handles escalated tickets from Tier 1"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `name` | string | **Yes** | Non-empty, must be unique |
| `description` | string | No | Optional |

**Success Response:** `201 Created`

```json
{ "id": "bb12cc34-dd56-ee78-ff90-aabb11223344" }
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Validation failure or duplicate name |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `workflowrole:write` permission |

---

### 6.3 Rename Workflow Role

**HTTP Method:** `PUT`  
**Route:** `/api/workflow-roles/{id}`  
**Authorization Requirement:** `workflowrole:write`

Updates the name and/or description of an existing Workflow Role (FR-RP-003). Can be performed regardless of the role's active/inactive status.

**Request Body:** `RenameWorkflowRoleRequest`

```json
{
  "name": "Senior Escalation Handler",
  "description": "Updated description"
}
```

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role not found or duplicate name |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `workflowrole:write` permission |

---

### 6.4 Deactivate Workflow Role

**HTTP Method:** `POST`  
**Route:** `/api/workflow-roles/{id}/deactivate`  
**Authorization Requirement:** `workflowrole:write`

Sets the Workflow Role to inactive (FR-RP-004). The role is hidden from all pickers and assignment UIs, but **existing agent assignments are preserved**. The role can be reactivated later.

**Request Body:** None

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role not found or already inactive |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `workflowrole:write` permission |

**Frontend Usage Notes:**
- Show a badge or indicator for inactive roles in the management list.
- Do not show inactive roles in agent assignment pickers — the server also filters them, but filtering client-side avoids confusion.

---

### 6.5 Reactivate Workflow Role

**HTTP Method:** `POST`  
**Route:** `/api/workflow-roles/{id}/reactivate`  
**Authorization Requirement:** `workflowrole:write`

Restores a previously deactivated Workflow Role to active status (FR-RP-005). The role is immediately visible in pickers again.

**Request Body:** None

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role not found or already active |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `workflowrole:write` permission |

---

### 6.6 Delete Workflow Role

**HTTP Method:** `DELETE`  
**Route:** `/api/workflow-roles/{id}`  
**Authorization Requirement:** `workflowrole:write`

Permanently soft-deletes a Workflow Role (FR-RP-006 / BR-RP-003). **Blocked** if the role is currently assigned to any agent **or** referenced by any workflow stage configuration.

On a block, the server returns `409 Conflict` with structured counts so the UI can render a meaningful message (e.g. *"12 agents and 3 stages reference this role"*).

**Success Response:** `204 No Content`

**`409 Conflict` Response (role is in use):**

```json
{
  "message": "Cannot delete: this Workflow Role is currently in use.",
  "assignedAgentCount": 12,
  "referencingStageCount": 3
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `204 No Content` | Deleted successfully |
| `400 Bad Request` | Role not found or already deleted |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `workflowrole:write` permission |
| `409 Conflict` | Role is assigned to agents or referenced by stages |

**Frontend Usage Notes:**
- Always show a confirmation dialog before calling delete.
- On `409`, display a message showing the exact counts from the response: *"Cannot delete — this role is currently assigned to 12 agents and referenced by 3 workflow stages. Reassign them first."*
- Do not show the delete button for system-default roles (`isSystemDefault: true`).

---

### 6.7 Set Agent Workflow Role Assignments

**HTTP Method:** `PUT`  
**Route:** `/api/workflow-roles/assignments/{userId}`  
**Authorization Requirement:** `workflowrole:write`

Sets an agent's full Workflow Role assignment in one atomic operation: a single **Primary** Workflow Role and zero or more **Additional** Workflow Roles (FS-05 §7 / FS-03 §3.2).

The operation is **idempotent** — existing assignments are preserved and only the delta is applied.

**Request Body:** `SetAgentWorkflowRolesRequest`

```json
{
  "primaryWorkflowRoleId": "aa01bb02-cc03-dd04-ee05-ff0011223344",
  "additionalWorkflowRoleIds": [
    "bb12cc34-dd56-ee78-ff90-aabb11223344",
    "cc23dd45-ee67-ff89-aa01-bbcc22334455"
  ]
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `primaryWorkflowRoleId` | GUID | **Yes** | Must reference an existing, active Workflow Role |
| `additionalWorkflowRoleIds` | GUID[] | **Yes** | Can be empty `[]`; must not include the primary ID (server deduplicates) |

**Success Response:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Primary role not found, or any additional role not found/inactive |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `workflowrole:write` permission |

**Frontend Usage Notes:**
- Call this to save the agent's full Workflow Role configuration in a single request.
- The primary role must be active; additional roles must also be active.
- The server ignores any overlap between `primaryWorkflowRoleId` and `additionalWorkflowRoleIds` — you do not need to filter the primary out of the additional list.

---

### 6.8 Stage Eligibility Preview

**HTTP Method:** `POST`  
**Route:** `/api/workflow-roles/stage-eligibility-preview`  
**Authorization Requirement:** `workflowrole:read`

Returns a live preview of which workflow stages an agent would be eligible for given a proposed Primary + Additional Workflow Role combination, **without saving** the assignment (FR-RP-044).

Use this to populate the "Stage Eligibility" section on the Agent Editor in real time as the admin selects roles.

**Request Body:** `StageEligibilityPreviewRequest`

```json
{
  "primaryWorkflowRoleId": "aa01bb02-cc03-dd04-ee05-ff0011223344",
  "additionalWorkflowRoleIds": [
    "bb12cc34-dd56-ee78-ff90-aabb11223344"
  ]
}
```

**Success Response:** `200 OK` — `StageEligibilitySummaryDto`

```json
{
  "eligibleStageNames": ["Intake", "Triage", "Escalation Review"],
  "totalEligibleStages": 3
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Primary role not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `workflowrole:read` permission |

**Frontend Usage Notes:**
- Call this on every role selection change in the Agent Editor (debounce by ~300ms).
- Display the `eligibleStageNames` list beneath the role picker as a read-only preview.

---

## 7. Entity Documentation

### 7.1 Role (Access Level)

Inherits from `SoftDeleteEntity`.

| Field | Type | Description |
|-------|------|-------------|
| `Id` | GUID | Unique identifier |
| `Name` | string | Max 80 chars; unique |
| `Description` | string? | Max 500 chars |
| `IsSystemRole` | bool | If `true`, cannot be deleted |
| `IsDeleted` | bool | Soft-delete flag |
| `CreatedAt` | DateTimeOffset | UTC creation timestamp |
| `UpdatedAt` | DateTimeOffset? | UTC last-update timestamp |
| `CreatedBy` | GUID | Actor who created the role |
| `UpdatedBy` | GUID? | Actor who last modified the role |

---

### 7.2 WorkflowRole

| Field | Type | Description |
|-------|------|-------------|
| `Id` | GUID | Unique identifier |
| `Name` | string | Role name; must be unique |
| `Description` | string? | Optional description |
| `IsActive` | bool | Controls visibility in pickers; does not affect existing assignments |
| `IsSystemDefault` | bool | If `true`, cannot be deleted |
| `CreatedAt` | DateTimeOffset | UTC creation timestamp |
| `UpdatedAt` | DateTimeOffset? | UTC last-update timestamp |

---

### 7.3 UserWorkflowRole

Join entity between `User` and `WorkflowRole`.

| Field | Type | Description |
|-------|------|-------------|
| `UserId` | GUID | FK → User.Id |
| `WorkflowRoleId` | GUID | FK → WorkflowRole.Id |
| `IsPrimary` | bool | `true` = Primary Workflow Role; `false` = Additional |
| `IsDeleted` | bool | Soft-delete flag |
| `CreatedAt` | DateTimeOffset | UTC creation timestamp |

There is a unique index on `(UserId, WorkflowRoleId)` — one agent cannot hold the same Workflow Role twice.

---

### 7.4 Permission

| Field | Type | Description |
|-------|------|-------------|
| `Id` | GUID | Unique identifier |
| `Resource` | string | Resource segment; max 60 chars; pattern `^[a-z_:]+$` |
| `Action` | string | Action segment; max 60 chars; pattern `^[a-z_]+$` |
| `Description` | string? | Optional human-readable description |
| `IsDeleted` | bool | Soft-delete flag |

**Computed key:** `Resource + ":" + Action` — this string appears in JWT `permission` claims.

---

### 7.5 UserRole

Join entity between `User` and `Role`.

| Field | Type | Description |
|-------|------|-------------|
| `UserId` | GUID | FK → User.Id |
| `RoleId` | GUID | FK → Role.Id |
| `AssignedAt` | DateTimeOffset | UTC timestamp when role was assigned |
| `AssignedBy` | GUID? | Actor who performed the assignment |
| `IsDeleted` | bool | Soft-delete flag |

---

## 8. TypeScript Models

```typescript
// ─── Access Level (Role) DTOs ─────────────────────────────────────────────────

export interface RoleDto {
  id: string;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  createdAt: string;       // ISO 8601
  updatedAt: string | null;
}

export interface RoleSummaryDto {
  id: string;
  name: string;
  isSystemRole: boolean;
}

export interface RoleWithPermissionsDto {
  id: string;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  permissions: PermissionDto[];
  createdAt: string;
  updatedAt: string | null;
}

export interface EffectivePermissionsSummaryDto {
  roleId: string;
  roleName: string;
  modules: Array<{
    module: string;
    permissions: string[];
  }>;
}

// ─── Workflow Role DTOs ───────────────────────────────────────────────────────

export interface WorkflowRoleDto {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  isSystemDefault: boolean;
  agentsAssignedCount: number;
  stagesReferencingCount: number;
  lastModified: string | null;  // ISO 8601
}

export interface StageEligibilitySummaryDto {
  eligibleStageNames: string[];
  totalEligibleStages: number;
}

// ─── Permission DTOs ──────────────────────────────────────────────────────────

export interface PermissionDto {
  id: string;
  resource: string;
  action: string;
  description: string | null;
}

// ─── User DTOs ────────────────────────────────────────────────────────────────

export interface UserSummaryDto {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  isActive: boolean;
}

export interface UserWithRolesDto {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  isActive: boolean;
  roles: RoleSummaryDto[];
  createdAt: string;
  lastLoginAt: string | null;
}

export interface GroupDto {
  id: string;
  name: string;
  regionCode: string | null;
  tierCode: string | null;
  unattendedAlertMinutes: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string | null;
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

// ─── Request Bodies — Access Levels ──────────────────────────────────────────

export interface CreateRoleRequest {
  name: string;
  description?: string | null;
}

export interface UpdateRoleRequest {
  name: string;
  description?: string | null;
}

export interface CloneRoleRequest {
  newRoleName: string;
  newRoleDescription?: string | null;
}

export interface CopyPermissionsFromRequest {
  sourceRoleId: string;  // GUID
}

export interface PermissionIdRequest {
  permissionId: string;  // GUID
}

export interface SetPermissionsRequest {
  permissionIds: string[];  // GUID[]
}

export interface RoleIdRequest {
  roleId: string;        // GUID
}

/** Used by PUT /api/rbac/users/{id}/access-level */
export interface SetAccessLevelRequest {
  accessLevelId: string; // GUID — single role
}

// ─── Request Bodies — Workflow Roles ─────────────────────────────────────────

export interface CreateWorkflowRoleRequest {
  name: string;
  description?: string | null;
}

export interface RenameWorkflowRoleRequest {
  name: string;
  description?: string | null;
}

export interface SetAgentWorkflowRolesRequest {
  primaryWorkflowRoleId: string;           // GUID
  additionalWorkflowRoleIds: string[];      // GUID[]
}

export interface StageEligibilityPreviewRequest {
  primaryWorkflowRoleId: string;           // GUID
  additionalWorkflowRoleIds: string[];      // GUID[]
}

// ─── Request Bodies — Permissions ────────────────────────────────────────────

export interface CreatePermissionRequest {
  resource: string;
  action: string;
  description?: string | null;
}

// ─── Query Params ─────────────────────────────────────────────────────────────

export interface GetUsersParams {
  email?: string;
  isActive?: boolean;
  pageNumber?: number;   // default: 1
  pageSize?: number;     // default: 20
}

export interface GetWorkflowRolesParams {
  isActive?: boolean;
  search?: string;
}

// ─── API Error ────────────────────────────────────────────────────────────────

export interface ApiErrorResponse {
  error: string;
}

export interface WorkflowRoleDeleteBlockedResponse {
  message: string;
  assignedAgentCount: number;
  referencingStageCount: number;
}

// ─── Utility ──────────────────────────────────────────────────────────────────

export function permissionKey(p: PermissionDto): string {
  return `${p.resource}:${p.action}`;
}
```

---

## 9. Frontend API Service Examples

```typescript
// rbacService.ts — Access Levels & Permissions
const BASE_RBAC = '/api/rbac';

function getToken(): string {
  return localStorage.getItem('access_token') ?? '';
}

async function request<T>(method: string, baseUrl: string, path: string, body?: unknown): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${getToken()}`,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (response.status === 204) return undefined as unknown as T;

  const data = await response.json();

  if (response.status === 409) {
    // WorkflowRole delete blocked — re-throw with the structured payload
    throw Object.assign(new Error((data as WorkflowRoleDeleteBlockedResponse).message), { details: data });
  }

  if (!response.ok) {
    throw new Error((data as ApiErrorResponse).error ?? `HTTP ${response.status}`);
  }

  return data as T;
}

const rbac = (path: string, body?: unknown) => ({
  get: <T>() => request<T>('GET', BASE_RBAC, path),
  post: <T>() => request<T>('POST', BASE_RBAC, path, body),
  put: <T>() => request<T>('PUT', BASE_RBAC, path, body),
  delete: <T>() => request<T>('DELETE', BASE_RBAC, path),
});

// ─── Roles (Access Levels) ────────────────────────────────────────────────────

export const getRoles = (): Promise<RoleDto[]> =>
  request('GET', BASE_RBAC, '/roles');

export const getRoleById = (id: string): Promise<RoleDto> =>
  request('GET', BASE_RBAC, `/roles/${id}`);

export const getRoleWithPermissions = (id: string): Promise<RoleWithPermissionsDto> =>
  request('GET', BASE_RBAC, `/roles/${id}/permissions`);

export const getEffectivePermissionsSummary = (id: string): Promise<EffectivePermissionsSummaryDto> =>
  request('GET', BASE_RBAC, `/roles/${id}/effective-permissions-summary`);

export const createRole = (body: CreateRoleRequest): Promise<{ id: string }> =>
  request('POST', BASE_RBAC, '/roles', body);

export const updateRole = (id: string, body: UpdateRoleRequest): Promise<void> =>
  request('PUT', BASE_RBAC, `/roles/${id}`, body);

export const deleteRole = (id: string): Promise<void> =>
  request('DELETE', BASE_RBAC, `/roles/${id}`);

export const cloneRole = (id: string, body: CloneRoleRequest): Promise<{ id: string }> =>
  request('POST', BASE_RBAC, `/roles/${id}/clone`, body);

export const copyPermissionsFrom = (targetId: string, body: CopyPermissionsFromRequest): Promise<void> =>
  request('POST', BASE_RBAC, `/roles/${targetId}/copy-permissions-from`, body);

// ─── Permissions ──────────────────────────────────────────────────────────────

export const getPermissions = (): Promise<PermissionDto[]> =>
  request('GET', BASE_RBAC, '/permissions');

export const getPermissionsByRole = (roleId: string): Promise<PermissionDto[]> =>
  request('GET', BASE_RBAC, `/permissions/by-role/${roleId}`);

export const createPermission = (body: CreatePermissionRequest): Promise<{ id: string }> =>
  request('POST', BASE_RBAC, '/permissions', body);

export const deletePermission = (id: string): Promise<void> =>
  request('DELETE', BASE_RBAC, `/permissions/${id}`);

// ─── Role ↔ Permission ────────────────────────────────────────────────────────

export const grantPermission = (roleId: string, permissionId: string): Promise<void> =>
  request('POST', BASE_RBAC, `/roles/${roleId}/permissions/grant`, { permissionId });

export const revokePermission = (roleId: string, permissionId: string): Promise<void> =>
  request('POST', BASE_RBAC, `/roles/${roleId}/permissions/revoke`, { permissionId });

export const setRolePermissions = (roleId: string, permissionIds: string[]): Promise<void> =>
  request('PUT', BASE_RBAC, `/roles/${roleId}/permissions`, { permissionIds });

// ─── Users ────────────────────────────────────────────────────────────────────

export const getUsers = (params: GetUsersParams = {}): Promise<PagedResultDto<UserSummaryDto>> => {
  const qs = new URLSearchParams();
  if (params.email !== undefined)      qs.set('email', params.email);
  if (params.isActive !== undefined)   qs.set('isActive', String(params.isActive));
  if (params.pageNumber !== undefined) qs.set('pageNumber', String(params.pageNumber));
  if (params.pageSize !== undefined)   qs.set('pageSize', String(params.pageSize));
  return request('GET', BASE_RBAC, `/users${qs.toString() ? `?${qs}` : ''}`);
};

export const getUserWithRoles = (userId: string): Promise<UserWithRolesDto> =>
  request('GET', BASE_RBAC, `/users/${userId}/roles`);

export const getUserEffectivePermissions = (userId: string): Promise<string[]> =>
  request('GET', BASE_RBAC, `/users/${userId}/permissions`);

export const assignRoleToUser = (userId: string, roleId: string): Promise<void> =>
  request('POST', BASE_RBAC, `/users/${userId}/roles/assign`, { roleId });

export const removeRoleFromUser = (userId: string, roleId: string): Promise<void> =>
  request('POST', BASE_RBAC, `/users/${userId}/roles/remove`, { roleId });

/** Replaces SET /users/{id}/roles from v1. Now accepts a single accessLevelId. */
export const setUserAccessLevel = (userId: string, accessLevelId: string): Promise<void> =>
  request('PUT', BASE_RBAC, `/users/${userId}/access-level`, { accessLevelId });
```

```typescript
// workflowRoleService.ts — Workflow Roles
const BASE_WF = '/api/workflow-roles';

export const getWorkflowRoles = (params: GetWorkflowRolesParams = {}): Promise<WorkflowRoleDto[]> => {
  const qs = new URLSearchParams();
  if (params.isActive !== undefined) qs.set('isActive', String(params.isActive));
  if (params.search)                 qs.set('search', params.search);
  return request('GET', BASE_WF, qs.toString() ? `?${qs}` : '');
};

export const createWorkflowRole = (body: CreateWorkflowRoleRequest): Promise<{ id: string }> =>
  request('POST', BASE_WF, '', body);

export const renameWorkflowRole = (id: string, body: RenameWorkflowRoleRequest): Promise<void> =>
  request('PUT', BASE_WF, `/${id}`, body);

export const deactivateWorkflowRole = (id: string): Promise<void> =>
  request('POST', BASE_WF, `/${id}/deactivate`);

export const reactivateWorkflowRole = (id: string): Promise<void> =>
  request('POST', BASE_WF, `/${id}/reactivate`);

export const deleteWorkflowRole = (id: string): Promise<void> =>
  request('DELETE', BASE_WF, `/${id}`);
// Throws with { details: WorkflowRoleDeleteBlockedResponse } on 409

export const setAgentWorkflowRoles = (
  userId: string,
  body: SetAgentWorkflowRolesRequest
): Promise<void> =>
  request('PUT', BASE_WF, `/assignments/${userId}`, body);

export const previewStageEligibility = (
  body: StageEligibilityPreviewRequest
): Promise<StageEligibilitySummaryDto> =>
  request('POST', BASE_WF, '/stage-eligibility-preview', body);
```

---

## 10. Search, Filtering and Pagination

### Access Level Endpoints

`GET /api/rbac/roles` and `GET /api/rbac/permissions` return **complete, unfiltered lists** with no pagination. Implement client-side filtering and sorting for those lists.

`GET /api/rbac/users` supports server-side filtering and pagination:

| Parameter | Type | Default | Behaviour |
|-----------|------|---------|-----------|
| `email` | string | null | Case-insensitive substring filter |
| `isActive` | boolean | null | Active/inactive filter |
| `pageNumber` | integer | 1 | 1-based page index |
| `pageSize` | integer | 20 | Items per page |

**Total pages:** `Math.ceil(totalCount / pageSize)`

### Workflow Role Endpoints

`GET /api/workflow-roles` supports server-side filtering:

| Parameter | Type | Default | Behaviour |
|-----------|------|---------|-----------|
| `isActive` | boolean | null | Filter by active status |
| `search` | string | null | Substring filter on name |

---

## 11. Error Handling Guide

### Standard Error Response Shape

```json
{ "error": "Human-readable error message" }
```

### Workflow Role Delete Blocked (409)

```json
{
  "message": "Cannot delete: this Workflow Role is currently in use.",
  "assignedAgentCount": 12,
  "referencingStageCount": 3
}
```

### Error Reference Table

| Status | Endpoint Pattern | Cause | Frontend Action |
|--------|-----------------|-------|-----------------|
| `400` | `POST /roles` | Duplicate role name | Inline form error |
| `400` | `PUT /roles/{id}` | Name conflict or role deleted | Inline error |
| `400` | `DELETE /roles/{id}` | System role | Disable button; show tooltip |
| `400` | `POST /roles/{id}/clone` | Duplicate target name | Inline form error |
| `400` | `POST /roles/{id}/copy-permissions-from` | Source/target not found | Toast error |
| `400` | `POST /permissions` | Duplicate `resource:action` | Inline error |
| `400` | `POST /roles/{id}/permissions/grant` | Role or permission not found | Toast error |
| `400` | `POST /roles/{id}/permissions/revoke` | Assignment does not exist | Toast error |
| `400` | `POST /users/{id}/roles/assign` | User or role not found | Toast error |
| `400` | `PUT /users/{id}/access-level` | User or role not found | Toast error |
| `400` | `POST /workflow-roles` | Duplicate name | Inline form error |
| `400` | `PUT /workflow-roles/{id}` | Role not found | Toast error |
| `400` | `PUT /workflow-roles/assignments/{userId}` | Primary/additional role invalid | Toast error |
| `401` | Any | Expired/missing JWT | Redirect to login |
| `403` | Any | Missing permission | Show "Access Denied" |
| `404` | `GET /roles/{id}` | Role not found | Navigate back to list |
| `404` | `GET /roles/{id}/effective-permissions-summary` | Role not found | Toast error |
| `404` | `GET /users/{id}/roles` | User not found | Toast error |
| `409` | `DELETE /workflow-roles/{id}` | Role in use by agents/stages | Show count-based blocking message |

### FluentValidation Error Shape

```json
{
  "errors": {
    "name": ["The Name field is required."],
    "resource": ["Resource must be lowercase letters, underscores, or colons."]
  },
  "status": 400
}
```

---

## 12. API Flow Diagrams

### 12.1 Clone Role Flow

```
Frontend (Clone Role Dialog)
        │
        │  POST /api/rbac/roles/{sourceId}/clone
        │  { newRoleName, newRoleDescription }
        ↓
   [JWT validated]  →  Permission check: "role:write"
        │
   [Source role fetched] → [Duplicate name check]
        │
   ┌────┴──────┐
   │  Pass?    │
   └────┬──────┘
   No → 400
   Yes → Clone role + copy all permissions
        ↓
   201 Created { id: newGuid }
        ↓
Frontend → navigate to /admin/roles/{newId}/permissions
```

---

### 12.2 Set Agent Workflow Roles Flow

```
Frontend (Agent Editor)
        │
        │  PUT /api/workflow-roles/assignments/{userId}
        │  { primaryWorkflowRoleId, additionalWorkflowRoleIds }
        ↓
   [JWT validated]  →  Permission check: "workflowrole:write"
        │
   [Primary role validated (must exist + active)]
        │
   [Additional roles validated (each must exist + active)]
        │
   [Fetch existing UserWorkflowRole records for userId]
        │
   [Soft-delete records not in target set]
        │
   [Create/restore records in target set]
        │
   [Mark primary assignment IsPrimary=true; others false]
        │
   204 No Content
        ↓
Frontend → show success toast
```

---

### 12.3 Delete Workflow Role Flow

```
Frontend (Workflow Role Management)
        │
        │  DELETE /api/workflow-roles/{id}
        ↓
   [JWT validated]  →  Permission check: "workflowrole:write"
        │
   [Role fetched → is system default?]
        │ Yes → 400
        │
   [Count assigned agents + referencing stages]
        │
   ┌────┴──────┐
   │  In use?  │
   └────┬──────┘
   Yes → 409 { assignedAgentCount, referencingStageCount }
   No  → Soft-delete role
        ↓
   204 No Content
        ↓
Frontend → remove role from list, show toast
```

---

## 13. Authentication & Authorization Integration

### Permission Strings Reference

| Resource | Actions | Controls |
|----------|---------|---------|
| `role` | `read`, `write` | Access Level CRUD, permission management |
| `permission` | `read`, `write` | Permission list and creation |
| `user` | `manage` | All user RBAC operations |
| `workflowrole` | `read`, `write` | Workflow Role CRUD and assignments |

### Client-Side Permission Hook

```typescript
const AuthContext = React.createContext<{ permissions: string[] }>({ permissions: [] });

function usePermission(key: string): boolean {
  const { permissions } = useContext(AuthContext);
  return permissions.includes(key);
}
```

### Conditional UI Visibility Rules

| UI Element | Required Permission |
|------------|-------------------|
| View Access Level list/detail | `role:read` |
| Create/Edit/Delete Access Level | `role:write` |
| Clone / Copy Permissions | `role:write` |
| View Effective Permissions Summary | `role:read` |
| View/Create/Delete Permissions | `permission:read` / `permission:write` |
| Grant/Revoke Permission on Role | `role:write` |
| View Users list | `user:manage` |
| Assign/Remove Access Level on User | `user:manage` |
| View Workflow Roles list | `workflowrole:read` |
| Create/Rename/Delete Workflow Role | `workflowrole:write` |
| Deactivate/Reactivate Workflow Role | `workflowrole:write` |
| Set Agent Workflow Role Assignments | `workflowrole:write` |
| Stage Eligibility Preview | `workflowrole:read` |

### Route Guards Example

```typescript
<Route
  path="/admin/roles"
  element={
    <ProtectedRoute requiredPermission="role:read">
      <RolesPage />
    </ProtectedRoute>
  }
/>
<Route
  path="/admin/workflow-roles"
  element={
    <ProtectedRoute requiredPermission="workflowrole:read">
      <WorkflowRolesPage />
    </ProtectedRoute>
  }
/>
```

### JWT Expiry Handling

```typescript
function isTokenExpired(token: string): boolean {
  const payload = JSON.parse(atob(token.split('.')[1]));
  return Date.now() / 1000 > (payload['exp'] as number);
}
```

### Important: JWT Permissions Are Stamped at Login

Because permissions are embedded in the JWT at login time, **changes to a user's Access Level roles or permissions do not take effect until they log out and log back in.** Workflow Role changes, however, take effect at the routing level without a re-login.

---

## 14. Testing Scenarios

### 14.1 Access Level (Role) Tests

| Test Case | Action | Expected Outcome |
|-----------|--------|-----------------|
| Create role — valid | `POST /roles` | `201` with new GUID |
| Create role — duplicate name | `POST /roles` with existing name | `400` |
| Clone role — valid | `POST /roles/{id}/clone` | `201` with new GUID, permissions copied |
| Clone role — duplicate target name | `POST /roles/{id}/clone` with taken name | `400` |
| Copy permissions from — valid | `POST /roles/{id}/copy-permissions-from` | `204` |
| Get effective permissions summary | `GET /roles/{id}/effective-permissions-summary` | `200` with module groups |
| Delete system role | `DELETE /roles/{systemRoleId}` | `400` |
| Update role — valid | `PUT /roles/{id}` | `204` |

### 14.2 Workflow Role Tests

| Test Case | Action | Expected Outcome |
|-----------|--------|-----------------|
| Create workflow role | `POST /workflow-roles` | `201` with new GUID |
| Create workflow role — duplicate name | `POST /workflow-roles` same name | `400` |
| List workflow roles — filtered by active | `GET /workflow-roles?isActive=true` | `200` active only |
| List workflow roles — search | `GET /workflow-roles?search=tier` | `200` name-matching only |
| Rename workflow role | `PUT /workflow-roles/{id}` | `204` |
| Deactivate workflow role | `POST /workflow-roles/{id}/deactivate` | `204`; hidden from pickers |
| Reactivate workflow role | `POST /workflow-roles/{id}/reactivate` | `204`; visible again |
| Delete unassigned workflow role | `DELETE /workflow-roles/{id}` | `204` |
| Delete workflow role in use | `DELETE /workflow-roles/{id}` | `409` with counts |
| Delete system-default workflow role | `DELETE /workflow-roles/{systemDefaultId}` | `400` |
| Set agent assignments — valid | `PUT /workflow-roles/assignments/{userId}` | `204` |
| Set agent assignments — primary + additional | Include same GUID in both | `204` (server deduplicates) |
| Set agent assignments — inactive primary | Pass inactive role as primary | `400` |
| Stage eligibility preview — valid | `POST /workflow-roles/stage-eligibility-preview` | `200` with stage names |
| Stage eligibility preview — invalid primary | Non-existent primary ID | `400` |

### 14.3 Permission String Tests

| Test Case | Expected Outcome |
|-----------|-----------------|
| Call `GET /rbac/roles` without `role:read` claim | `403 Forbidden` |
| Call `POST /rbac/roles` without `role:write` claim | `403 Forbidden` |
| Call `GET /workflow-roles` without `workflowrole:read` claim | `403 Forbidden` |
| Call any RBAC endpoint without JWT | `401 Unauthorized` |

---

## 15. Frontend Integration Checklist

### Access Levels

- [ ] Permission strings updated to short form: `role:read`, `role:write`, `user:manage`, `permission:read`, `permission:write`
- [ ] Clone Role UI added with name/description form; navigates to new role's permissions screen on success
- [ ] Copy Permissions From UI adds confirmation dialog before calling endpoint
- [ ] Effective Permissions Summary panel added to role detail screen
- [ ] `PUT /users/{id}/roles` calls migrated to `PUT /users/{id}/access-level` with single `accessLevelId`
- [ ] System roles show no Edit/Delete controls
- [ ] Role changes display "User must re-login for changes to take effect" notice

### Workflow Roles

- [ ] Workflow Roles management screen at `/admin/workflow-roles` guarded by `workflowrole:read`
- [ ] Active/inactive status badge shown on each Workflow Role in the list
- [ ] Inactive roles hidden from agent assignment pickers
- [ ] Delete shows confirmation dialog; 409 response displays agent + stage counts
- [ ] System-default Workflow Roles show no delete button
- [ ] Agent Editor includes Primary Workflow Role picker (required) and Additional Roles multi-select (optional)
- [ ] Stage Eligibility Preview panel updates on role selection change (debounced 300ms)
- [ ] Set Agent Assignments uses `PUT /workflow-roles/assignments/{userId}` (not individual add/remove calls)

### Error Handling

- [ ] `ApiErrorResponse.error` field extracted and shown
- [ ] `409 Conflict` from Workflow Role delete shows structured count message
- [ ] FluentValidation `ModelState` errors mapped to form fields
- [ ] Network errors show generic retry message

### Security

- [ ] UI elements gated on JWT `permission` claims
- [ ] Route guards prevent navigation to unauthorized pages
- [ ] Client-side permission checks are UX only — server is always source of truth

---

## 16. Common Frontend Workflows

### Workflow 1: Clone an Access Level

```typescript
async function handleCloneRole(sourceId: string, newName: string, newDescription?: string) {
  try {
    const { id } = await cloneRole(sourceId, { newRoleName: newName, newRoleDescription: newDescription });
    router.push(`/admin/roles/${id}/permissions`);
    toast.success('Role cloned successfully.');
  } catch (err) {
    setFormError('newRoleName', (err as Error).message);
  }
}
```

---

### Workflow 2: Copy Permissions From Another Role

```typescript
async function handleCopyPermissions(targetRoleId: string, sourceRoleId: string) {
  const confirmed = await showConfirmDialog(
    `This will overwrite the target role's permissions. Continue?`
  );
  if (!confirmed) return;
  try {
    await copyPermissionsFrom(targetRoleId, { sourceRoleId });
    await refreshRolePermissions(targetRoleId);
    toast.success('Permissions copied.');
  } catch (err) {
    toast.error((err as Error).message);
  }
}
```

---

### Workflow 3: Delete a Workflow Role

```typescript
async function handleDeleteWorkflowRole(id: string) {
  const confirmed = await showConfirmDialog('Delete this Workflow Role?');
  if (!confirmed) return;

  try {
    await deleteWorkflowRole(id);
    toast.success('Workflow Role deleted.');
    refreshList();
  } catch (err: any) {
    if (err.details) {
      const d = err.details as WorkflowRoleDeleteBlockedResponse;
      toast.error(
        `Cannot delete — assigned to ${d.assignedAgentCount} agent(s) ` +
        `and referenced by ${d.referencingStageCount} stage(s). Reassign them first.`
      );
    } else {
      toast.error((err as Error).message);
    }
  }
}
```

---

### Workflow 4: Save Agent Workflow Role Assignments

```typescript
async function handleSaveAgentRoles(
  userId: string,
  primaryWorkflowRoleId: string,
  additionalWorkflowRoleIds: string[]
) {
  try {
    await setAgentWorkflowRoles(userId, { primaryWorkflowRoleId, additionalWorkflowRoleIds });
    toast.success('Workflow Role assignments saved.');
  } catch (err) {
    toast.error((err as Error).message);
  }
}
```

---

### Workflow 5: Stage Eligibility Preview (Live)

```typescript
// In Agent Editor component
const [eligibleStages, setEligibleStages] = useState<string[]>([]);

const updatePreview = useMemo(() =>
  debounce(async (primaryId: string, additionalIds: string[]) => {
    if (!primaryId) { setEligibleStages([]); return; }
    try {
      const result = await previewStageEligibility({
        primaryWorkflowRoleId: primaryId,
        additionalWorkflowRoleIds: additionalIds,
      });
      setEligibleStages(result.eligibleStageNames);
    } catch {
      setEligibleStages([]);
    }
  }, 300),
[]);

useEffect(() => {
  updatePreview(selectedPrimary, selectedAdditional);
}, [selectedPrimary, selectedAdditional]);
```

---

*End of RBAC Frontend Integration Guide*

---

**Document Version:** 2.0.0 — Updated from source code analysis of the Adrenalin Unify Platform.  
**Controllers:** `RolesController`, `PermissionsController`, `UsersRbacController`, `WorkflowRolesController`  
**Module:** `Adrenalin.Modules.Auth`  
**Authorization:** `PermissionAuthorizationHandler`, `PermissionPolicyProvider`, `PermissionRequirement`  
**Breaking Changes from v1.0.0:**
- Permission strings changed from `rbac:role:*` prefix to short form (`role:read`, `role:write`, `user:manage`, etc.)
- `PUT /api/rbac/users/{id}/roles` replaced by `PUT /api/rbac/users/{id}/access-level` with single `accessLevelId`
- New `WorkflowRoles` resource added at `/api/workflow-roles`
- New Roles endpoints: `/clone`, `/copy-permissions-from`, `/effective-permissions-summary`
