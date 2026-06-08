# RBAC Frontend Integration Guide

**Project:** Adrenalin Unify Platform  
**Module:** Role-Based Access Control (RBAC)  
**Base URL:** `/api/rbac`  
**Audience:** Frontend Developers · QA Engineers · API Consumers  
**Document Version:** 1.0.0

---

## Table of Contents

1. [Module Overview](#1-module-overview)
2. [API Endpoints — Roles](#2-api-endpoints--roles)
3. [API Endpoints — Permissions](#3-api-endpoints--permissions)
4. [API Endpoints — Role Permissions](#4-api-endpoints--role-permissions)
5. [API Endpoints — User Roles](#5-api-endpoints--user-roles)
6. [Entity Documentation](#6-entity-documentation)
7. [TypeScript Models](#7-typescript-models)
8. [Frontend API Service Examples](#8-frontend-api-service-examples)
9. [Search, Filtering and Pagination](#9-search-filtering-and-pagination)
10. [Error Handling Guide](#10-error-handling-guide)
11. [API Flow Diagrams](#11-api-flow-diagrams)
12. [Authentication & Authorization Integration](#12-authentication--authorization-integration)
13. [Testing Scenarios](#13-testing-scenarios)
14. [Frontend Integration Checklist](#14-frontend-integration-checklist)
15. [Common Frontend Workflows](#15-common-frontend-workflows)

---

## 1. Module Overview

### Purpose

The RBAC module for the Adrenalin Unify Platform provides a complete Role-Based Access Control system. It manages the assignment of Roles to Users and the assignment of Permissions to Roles. Every API call to a protected resource is validated against a permission string embedded inside the user's JWT at login time.

### Features

- Create, update, and soft-delete **Roles**
- Create and soft-delete **Permissions** (defined as `resource:action` pairs)
- Grant or revoke individual permissions from a role, or replace the full permission set atomically
- Assign or remove a role from a user, or replace the full role set atomically
- Query users with pagination and email/active-status filtering
- Retrieve a user's effective permissions (the full resolved list across all roles)
- All destructive operations are **soft-deletes** — records are never physically removed

### User Flow

```
User logs in
     ↓
Server resolves user → roles → permissions
     ↓
JWT is issued with:
  - sub  (userId)
  - email
  - role[] claims
  - permission[] claims  ← e.g. "rbac:role:read", "ticket:create"
     ↓
Frontend stores JWT
     ↓
Every API request carries:  Authorization: Bearer <JWT>
     ↓
Server validates JWT and checks "permission" claims
     ↓
Access granted or 403 returned
```

### Authentication Model

The platform uses **JWT Bearer authentication** (HMAC-SHA256 signed). Tokens are issued at login and contain:

| Claim | Type | Description |
|-------|------|-------------|
| `sub` | string (GUID) | User's unique identifier |
| `email` | string | User's email address |
| `jti` | string (GUID) | Unique token identifier |
| `http://schemas.microsoft.com/ws/2008/06/identity/claims/role` | string[] | One entry per assigned role name |
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
└─────────────┘                                    │ UpdatedAt        │
                                                   └──────────────────┘
                                                            │ 1
                                                            │
                                                           <│
                                                   ┌──────────────────┐
                                                   │  RolePermission  │
                                                   │──────────────────│
                                                   │ RoleId (FK)      │
                                                   │ PermissionId (FK)│
                                                   │ IsDeleted        │
                                                   └──────────────────┘
                                                            │>
                                                            │ 1
                                                   ┌──────────────────┐
                                                   │   Permission     │
                                                   │──────────────────│
                                                   │ Id (PK)          │
                                                   │ Resource         │
                                                   │ Action           │
                                                   │ Description      │
                                                   │ IsDeleted        │
                                                   └──────────────────┘
```

---

## 2. API Endpoints — Roles

### 2.1 Get All Roles

**HTTP Method:** `GET`

**Route:** `/api/rbac/roles`

**Description:** Returns a flat list of all non-deleted roles.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:role:read`

**Path Parameters:** None

**Query Parameters:** None

**Request Body:** None

**Validation Rules:** None

**Success Response:**

- **Status:** `200 OK`
- **Body:** Array of `RoleDto`

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Support Agent",
    "description": "Can view and respond to tickets",
    "isSystemRole": false,
    "createdAt": "2026-01-15T09:00:00Z",
    "updatedAt": null
  },
  {
    "id": "8ab12c34-1234-5678-abcd-ef0123456789",
    "name": "Admin",
    "description": "Full system access",
    "isSystemRole": true,
    "createdAt": "2026-01-01T00:00:00Z",
    "updatedAt": "2026-03-01T10:00:00Z"
  }
]
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Internal failure returning the list |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | JWT does not contain `rbac:role:read` permission |

**Frontend Usage Notes:**
- Use this endpoint to populate role dropdowns, role management tables, and user-assignment UI.
- Cache the result briefly (e.g. 30 seconds) if the UI renders role lists frequently.
- System roles (`isSystemRole: true`) should be displayed as read-only and deletion controls must be hidden for them.

---

### 2.2 Get Role by ID

**HTTP Method:** `GET`

**Route:** `/api/rbac/roles/{id}`

**Description:** Returns a single role by its GUID.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:role:read`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The role's unique identifier |

**Query Parameters:** None

**Request Body:** None

**Validation Rules:** `id` must be a valid GUID (enforced by route constraint `{id:guid}`).

**Success Response:**

- **Status:** `200 OK`
- **Body:** Single `RoleDto`

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
| `403 Forbidden` | Missing `rbac:role:read` permission |
| `404 Not Found` | No role found with the given ID |

**Frontend Usage Notes:**
- Use for detail view / edit form pre-population.
- If `404` is received, show a "Role not found" message and redirect to the roles list.

---

### 2.3 Get Role With Permissions

**HTTP Method:** `GET`

**Route:** `/api/rbac/roles/{id}/permissions`

**Description:** Returns a role together with its full list of currently active (non-deleted) permissions.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:role:read`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The role's unique identifier |

**Query Parameters:** None

**Request Body:** None

**Success Response:**

- **Status:** `200 OK`
- **Body:** `RoleWithPermissionsDto`

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
    },
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f01234567890",
      "resource": "ticket",
      "action": "create",
      "description": "Create new tickets"
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
| `403 Forbidden` | Missing `rbac:role:read` permission |
| `404 Not Found` | Role not found |

**Frontend Usage Notes:**
- Use this endpoint on the role-edit screen to show which permissions are currently active.
- The `permissions` array contains only non-deleted (active) role-permission assignments.
- The permission key (used in JWT claims) for each item is `resource + ":" + action`, e.g. `"ticket:read"`.

---

### 2.4 Create Role

**HTTP Method:** `POST`

**Route:** `/api/rbac/roles`

**Description:** Creates a new role. The actor's user ID is extracted from the JWT `sub` claim.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:role:create`

**Path Parameters:** None

**Query Parameters:** None

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

**Validation Rules:**
- `name`: Required, `NotEmpty`, `MaximumLength(80)`. Uniqueness is checked server-side — if a role with the same name already exists, the request fails with `400`.
- `description`: Optional. If provided, `MaximumLength(500)`.
- Both values are trimmed of leading/trailing whitespace by the domain entity.

**Success Response:**

- **Status:** `201 Created`
- **Headers:** `Location: /api/rbac/roles/{newId}`
- **Body:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Validation failure or duplicate role name |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:role:create` permission |

**Error body example (duplicate name):**
```json
{
  "error": "A role named 'Billing Manager' already exists."
}
```

**Frontend Usage Notes:**
- After a `201`, use the returned `id` to redirect the user to the role detail or permissions-assignment screen.
- Display a toast/notification confirming creation.
- Pre-validate `name` length client-side (max 80 chars) before submission.
- Show a meaningful error if the API returns a duplicate-name `400`.

---

### 2.5 Update Role

**HTTP Method:** `PUT`

**Route:** `/api/rbac/roles/{id}`

**Description:** Updates an existing role's name and/or description.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:role:update`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | Role to update |

**Request Body:** `UpdateRoleRequest`

```json
{
  "name": "Senior Billing Manager",
  "description": "Updated description for senior billing role"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `name` | string | **Yes** | Non-empty, max 80 characters, unique among other roles |
| `description` | string | No | Max 500 characters |

**Validation Rules:**
- `name`: Required, `NotEmpty`, `MaximumLength(80)`. A duplicate-name check is performed — if another role (different ID) already uses the name, request fails with `400`.
- `description`: Optional, `MaximumLength(500)` when provided.
- Cannot update a soft-deleted role (server returns `400`).

**Success Response:**

- **Status:** `204 No Content`
- **Body:** Empty

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Validation failure, role not found, duplicate name, or role is deleted |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:role:update` permission |

**Frontend Usage Notes:**
- Optimistic UI: you may update the local state immediately and rollback on error.
- After a `204`, refresh the roles list or the specific role's detail view.
- If the role is a system role (visible from `GET /roles/{id}`), disable the edit controls to prevent confusing errors.

---

### 2.6 Delete Role

**HTTP Method:** `DELETE`

**Route:** `/api/rbac/roles/{id}`

**Description:** Soft-deletes a role and cascades soft-deletion to all associated `UserRole` records. System roles (`isSystemRole: true`) cannot be deleted.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:role:delete`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | Role to delete |

**Request Body:** None

**Success Response:**

- **Status:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role not found, already deleted, or is a system role |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:role:delete` permission |

**Error body example (system role):**
```json
{
  "error": "System roles cannot be deleted."
}
```

**Frontend Usage Notes:**
- Always display a confirmation dialog before calling delete.
- If `isSystemRole: true`, do not show the delete button at all.
- After a successful `204`, remove the role from the local list and show a success notification.
- All users who previously held this role will lose it automatically (cascade soft-delete).

---

## 3. API Endpoints — Permissions

### 3.1 Get All Permissions

**HTTP Method:** `GET`

**Route:** `/api/rbac/permissions`

**Description:** Returns a flat list of all non-deleted permissions.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:role:read`

**Request Body:** None

**Success Response:**

- **Status:** `200 OK`

```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
    "resource": "ticket",
    "action": "read",
    "description": "View tickets"
  },
  {
    "id": "b2c3d4e5-f6a7-8901-bcde-f01234567890",
    "resource": "ticket",
    "action": "create",
    "description": "Create new tickets"
  },
  {
    "id": "c3d4e5f6-a7b8-9012-cdef-012345678901",
    "resource": "rbac",
    "action": "role:read",
    "description": "Read roles and permissions"
  }
]
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Internal failure |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:role:read` permission |

**Frontend Usage Notes:**
- Use this to populate the permission-picker in a role's edit screen.
- Each permission can be identified by its composite key: `resource + ":" + action`.
- Store the full list in component state or a lightweight cache to avoid repeated fetches during role management flows.

---

### 3.2 Get Permissions by Role

**HTTP Method:** `GET`

**Route:** `/api/rbac/permissions/by-role/{roleId}`

**Description:** Returns all active permissions assigned to a specific role. This queries the `RolePermission` join table and returns only non-deleted assignments.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:role:read`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `roleId` | GUID | Yes | The role to query permissions for |

**Success Response:**

- **Status:** `200 OK`

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
| `403 Forbidden` | Missing `rbac:role:read` permission |

**Frontend Usage Notes:**
- Use alongside `GET /api/rbac/permissions` to display a "currently assigned" vs "available" two-panel permission picker.

---

### 3.3 Create Permission

**HTTP Method:** `POST`

**Route:** `/api/rbac/permissions`

**Description:** Creates a new permission. The `resource` and `action` values are automatically converted to lowercase by the domain entity.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:permission:manage`

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
| `description` | string | No | No length constraint defined |

**Validation Rules:**
- `resource`: Required, `MaximumLength(60)`, must match `^[a-z_:]+$` (lowercase letters, underscores, colons only).
- `action`: Required, `MaximumLength(60)`, must match `^[a-z_]+$` (lowercase letters and underscores only).
- The combination `resource:action` must be unique. Server returns `400` if it already exists.
- Both values are trimmed and converted to lowercase by the domain model.

**Success Response:**

- **Status:** `201 Created`
- **Body:**

```json
{
  "id": "d4e5f6a7-b8c9-0123-def0-123456789012"
}
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Validation failure or duplicate `resource:action` |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:permission:manage` permission |

**Error body example (duplicate):**
```json
{
  "error": "Permission 'report:export' already exists."
}
```

**Frontend Usage Notes:**
- Enforce the regex patterns client-side with real-time validation hints.
- Show the composite key preview (`resource:action`) in the form as the user types.
- After creation, refresh the full permission list.

---

### 3.4 Delete Permission

**HTTP Method:** `DELETE`

**Route:** `/api/rbac/permissions/{id}`

**Description:** Soft-deletes a permission and cascades soft-deletion to all `RolePermission` records that reference it.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:permission:manage`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | Permission to delete |

**Request Body:** None

**Success Response:**

- **Status:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Permission not found or already deleted |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:permission:manage` permission |

**Frontend Usage Notes:**
- Warn the user that deleting a permission removes it from all roles that currently hold it.
- After a `204`, refresh both the permissions list and any open role-with-permissions views.

---

## 4. API Endpoints — Role Permissions

### 4.1 Grant Permission to Role

**HTTP Method:** `POST`

**Route:** `/api/rbac/roles/{id}/permissions/grant`

**Description:** Assigns a single permission to a role. This operation is **idempotent** — granting an already-assigned permission returns `204` without error or duplication.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:permission:manage`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The role to assign the permission to |

**Request Body:** `PermissionIdRequest`

```json
{
  "permissionId": "a1b2c3d4-e5f6-7890-abcd-ef0123456789"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `permissionId` | GUID | **Yes** | Must not be `Guid.Empty` |

**Validation Rules:**
- `roleId` (path): Must be a valid non-empty GUID.
- `permissionId` (body): Must be a valid non-empty GUID.
- Both the role and the permission must exist (non-deleted); otherwise `400` is returned.

**Success Response:**

- **Status:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role or permission not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:permission:manage` permission |

**Frontend Usage Notes:**
- Safe to call when the user clicks a checkbox in a permission-picker; repeated clicks cause no harm.
- After `204`, update the local permissions list for the role.

---

### 4.2 Revoke Permission from Role

**HTTP Method:** `POST`

**Route:** `/api/rbac/roles/{id}/permissions/revoke`

**Description:** Removes a single permission from a role (soft-delete of the `RolePermission` record).

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:permission:manage`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The role to remove the permission from |

**Request Body:** `PermissionIdRequest`

```json
{
  "permissionId": "a1b2c3d4-e5f6-7890-abcd-ef0123456789"
}
```

**Validation Rules:**
- Both IDs must be non-empty GUIDs.
- The permission must currently be assigned to the role; otherwise `400` is returned with `"Permission is not assigned to this role."`.

**Success Response:**

- **Status:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Assignment not found or internal error |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:permission:manage` permission |

**Frontend Usage Notes:**
- Check that the permission is currently assigned before offering the revoke action to avoid confusing `400` errors.

---

### 4.3 Set Role Permissions (Atomic Replace)

**HTTP Method:** `PUT`

**Route:** `/api/rbac/roles/{id}/permissions`

**Description:** Atomically replaces the entire permission set of a role. All existing `RolePermission` records for this role are soft-deleted, then new ones are created for every ID in `permissionIds`. Sending an empty array effectively removes all permissions.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:permission:manage`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | The role to update |

**Request Body:** `SetPermissionsRequest`

```json
{
  "permissionIds": [
    "a1b2c3d4-e5f6-7890-abcd-ef0123456789",
    "b2c3d4e5-f6a7-8901-bcde-f01234567890"
  ]
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `permissionIds` | GUID[] | **Yes** | Must not be null; all GUIDs must be non-empty and must exist as active permissions |

**Validation Rules:**
- `permissionIds`: `NotNull()`, all values must not be `Guid.Empty`.
- Each permission ID is individually verified to exist before the atomic replace proceeds. First invalid ID causes a `400` and aborts the operation.

**Success Response:**

- **Status:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Role not found or any permission ID does not exist |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:permission:manage` permission |

**Frontend Usage Notes:**
- Use this for a "save all" button that submits the entire checked-permission state at once, rather than making individual grant/revoke calls.
- Because the operation is atomic (server-side), the UI should lock the form during the request and refresh after.

---

### 4.4 Get Role Permissions (via Roles endpoint)

See [Section 2.3 — Get Role With Permissions](#23-get-role-with-permissions) for the `GET /api/rbac/roles/{id}/permissions` endpoint.

See [Section 3.2 — Get Permissions by Role](#32-get-permissions-by-role) for the `GET /api/rbac/permissions/by-role/{roleId}` endpoint.

Both return `PermissionDto[]` for the given role.

---

## 5. API Endpoints — User Roles

### 5.1 Get All Users (with Filtering & Pagination)

**HTTP Method:** `GET`

**Route:** `/api/rbac/users`

**Description:** Returns a paginated list of users. Supports optional filtering by email substring and active status.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:user:read`

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `email` | string | No | null | Substring filter on email address |
| `isActive` | boolean | No | null | Filter by active status |
| `pageNumber` | integer | No | 1 | Page number (1-based) |
| `pageSize` | integer | No | 20 | Items per page |

**Request Body:** None

**Success Response:**

- **Status:** `200 OK`
- **Body:** `PagedResultDto<UserSummaryDto>`

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
| `403 Forbidden` | Missing `rbac:user:read` permission |

---

### 5.2 Get User With Roles

**HTTP Method:** `GET`

**Route:** `/api/rbac/users/{id}/roles`

**Description:** Returns a user and all their currently active (non-deleted) role assignments.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:user:read`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | User's unique identifier |

**Success Response:**

- **Status:** `200 OK`
- **Body:** `UserWithRolesDto`

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
| `403 Forbidden` | Missing `rbac:user:read` permission |
| `404 Not Found` | User not found |

---

### 5.3 Get User Effective Permissions

**HTTP Method:** `GET`

**Route:** `/api/rbac/users/{id}/permissions`

**Description:** Returns the full resolved list of `resource:action` permission strings for a user, derived from all their currently active roles.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:user:read`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | User's unique identifier |

**Success Response:**

- **Status:** `200 OK`
- **Body:** `string[]`

```json
[
  "ticket:read",
  "ticket:create",
  "rbac:role:read",
  "rbac:user:read"
]
```

**Error Responses:**

| Status | Cause |
|--------|-------|
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:user:read` permission |
| `404 Not Found` | User not found |

**Frontend Usage Notes:**
- This is the same list that the server stamps into the JWT at login. Useful for admin UIs that display a user's current access.
- Do **not** use this endpoint to gate UI elements in the current user's own session — use the JWT claims already available client-side instead.

---

### 5.4 Assign Role to User

**HTTP Method:** `POST`

**Route:** `/api/rbac/users/{id}/roles/assign`

**Description:** Assigns a role to a user. **Idempotent** — assigning an already-active role returns `204` without duplication. If the assignment previously existed but was soft-deleted, it is restored.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:user:assign`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | Target user |

**Request Body:** `RoleIdRequest`

```json
{
  "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `roleId` | GUID | **Yes** | Must not be `Guid.Empty` |

**Validation Rules:**
- Both `userId` (path) and `roleId` (body) must be non-empty GUIDs.
- Both the user and role must exist and not be deleted.

**Success Response:**

- **Status:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | User or role not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:user:assign` permission |

---

### 5.5 Remove Role from User

**HTTP Method:** `POST`

**Route:** `/api/rbac/users/{id}/roles/remove`

**Description:** Removes (soft-deletes) a role assignment from a user.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:user:assign`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | Target user |

**Request Body:** `RoleIdRequest`

```json
{
  "roleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Validation Rules:**
- Both IDs must be non-empty GUIDs.
- The role must currently be actively assigned to the user; otherwise returns `"Role is not assigned to this user."`.

**Success Response:**

- **Status:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | Assignment not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:user:assign` permission |

---

### 5.6 Set User Roles (Atomic Replace)

**HTTP Method:** `PUT`

**Route:** `/api/rbac/users/{id}/roles`

**Description:** Atomically replaces all role assignments for a user. Existing assignments are soft-deleted, then new ones are created for each ID in `roleIds`. Sending an empty array removes all roles.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:user:assign`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | Target user |

**Request Body:** `SetRolesRequest`

```json
{
  "roleIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "8ab12c34-1234-5678-abcd-ef0123456789"
  ]
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `roleIds` | GUID[] | **Yes** | Must not be null; all GUIDs non-empty and existing |

**Success Response:**

- **Status:** `204 No Content`

**Error Responses:**

| Status | Cause |
|--------|-------|
| `400 Bad Request` | User not found or any role ID not found |
| `401 Unauthorized` | Missing or invalid JWT |
| `403 Forbidden` | Missing `rbac:user:assign` permission |

---

### 5.7 Get User Groups

**HTTP Method:** `GET`

**Route:** `/api/rbac/users/{id}/groups`

**Description:** Returns all groups the user currently belongs to.

**Authentication:** Bearer JWT required.

**Authorization Requirement:** `rbac:user:read`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | GUID | Yes | User's unique identifier |

**Success Response:**

- **Status:** `200 OK`
- **Body:** `GroupDto[]`

```json
[
  {
    "id": "f1e2d3c4-b5a6-7890-cdef-012345678901",
    "name": "APAC Support Team",
    "regionCode": "APAC",
    "tierCode": "T1",
    "unattendedAlertMinutes": 30,
    "isActive": true,
    "createdAt": "2026-01-05T00:00:00Z",
    "updatedAt": null
  }
]
```

---

## 6. Entity Documentation

### 6.1 Role

Inherits from `SoftDeleteEntity` (which provides `Id`, `IsDeleted`, `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`).

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | GUID | Yes | Unique identifier (auto-generated) |
| `Name` | string | Yes | Role name; max 80 chars; unique; trimmed |
| `Description` | string? | No | Optional description; max 500 chars |
| `IsSystemRole` | bool | Yes | If `true`, the role cannot be deleted |
| `IsDeleted` | bool | Yes | Soft-delete flag; defaults to `false` |
| `CreatedAt` | DateTimeOffset | Yes | UTC timestamp of creation |
| `UpdatedAt` | DateTimeOffset? | No | UTC timestamp of last update |
| `CreatedBy` | GUID | Yes | ID of the user who created the role |
| `UpdatedBy` | GUID? | No | ID of the user who last updated the role |
| `UserRoles` | UserRole[] | — | Navigation: active user-role assignments |
| `RolePermissions` | RolePermission[] | — | Navigation: permission assignments |

---

### 6.2 Permission

Inherits from `SoftDeleteEntity`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | GUID | Yes | Unique identifier (auto-generated) |
| `Resource` | string | Yes | Resource segment; max 60 chars; lowercase; pattern `^[a-z_:]+$` |
| `Action` | string | Yes | Action segment; max 60 chars; lowercase; pattern `^[a-z_]+$` |
| `Description` | string? | No | Optional human-readable description |
| `IsDeleted` | bool | Yes | Soft-delete flag |
| `CreatedAt` | DateTimeOffset | Yes | UTC creation timestamp |
| `UpdatedAt` | DateTimeOffset? | No | UTC last-update timestamp |
| `CreatedBy` | GUID | Yes | Actor who created this permission |
| `UpdatedBy` | GUID? | No | Actor who last modified this permission |
| `RolePermissions` | RolePermission[] | — | Navigation: roles this permission is assigned to |

**Computed key:** `Resource + ":" + Action` — this string is what appears in JWT `permission` claims.

---

### 6.3 RolePermission

Join entity between `Role` and `Permission`. Inherits from `SoftDeleteEntity`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | GUID | Yes | Unique identifier |
| `RoleId` | GUID | Yes | FK → Role.Id |
| `PermissionId` | GUID | Yes | FK → Permission.Id |
| `IsDeleted` | bool | Yes | Soft-delete flag |
| `CreatedAt` | DateTimeOffset | Yes | UTC assignment timestamp |
| `UpdatedAt` | DateTimeOffset? | No | UTC last-change timestamp |
| `CreatedBy` | GUID | Yes | Actor who created the assignment |
| `UpdatedBy` | GUID? | No | Actor who last modified the assignment |

---

### 6.4 UserRole

Join entity between `User` and `Role`. Inherits from `SoftDeleteEntity`.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `Id` | GUID | Yes | Unique identifier |
| `UserId` | GUID | Yes | FK → User.Id |
| `RoleId` | GUID | Yes | FK → Role.Id |
| `AssignedAt` | DateTimeOffset | Yes | UTC timestamp when role was assigned |
| `AssignedBy` | GUID? | No | Actor who performed the assignment |
| `IsDeleted` | bool | Yes | Soft-delete flag |
| `CreatedAt` | DateTimeOffset | Yes | UTC creation timestamp |
| `UpdatedAt` | DateTimeOffset? | No | UTC last-change timestamp |
| `CreatedBy` | GUID | Yes | Actor who created the record |
| `UpdatedBy` | GUID? | No | Actor who last modified the record |

**Note on restore behaviour:** If a `UserRole` previously existed and was soft-deleted, assigning the same role again restores the existing record (updating `AssignedAt`, `AssignedBy`, `IsDeleted = false`) rather than creating a duplicate.

---

## 7. TypeScript Models

These interfaces are derived directly from the C# DTOs in `RbacDtos.cs` and the request records in the controllers.

```typescript
// ─── Response DTOs ────────────────────────────────────────────────────────────

export interface RoleDto {
  id: string;            // GUID
  name: string;
  description: string | null;
  isSystemRole: boolean;
  createdAt: string;     // ISO 8601 DateTimeOffset
  updatedAt: string | null;
}

export interface RoleSummaryDto {
  id: string;            // GUID
  name: string;
  isSystemRole: boolean;
}

export interface PermissionDto {
  id: string;            // GUID
  resource: string;
  action: string;
  description: string | null;
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

export interface UserSummaryDto {
  id: string;            // GUID
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

export interface GroupMemberDto {
  userId: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  isLead: boolean;
}

export interface GroupWithMembersDto {
  group: GroupDto;
  members: GroupMemberDto[];
}

export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

// ─── Request Bodies ───────────────────────────────────────────────────────────

export interface CreateRoleRequest {
  name: string;
  description?: string | null;
}

export interface UpdateRoleRequest {
  name: string;
  description?: string | null;
}

export interface CreatePermissionRequest {
  resource: string;
  action: string;
  description?: string | null;
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

export interface SetRolesRequest {
  roleIds: string[];     // GUID[]
}

// ─── Query Params ─────────────────────────────────────────────────────────────

export interface GetUsersParams {
  email?: string;
  isActive?: boolean;
  pageNumber?: number;  // default: 1
  pageSize?: number;    // default: 20
}

// ─── API Error ────────────────────────────────────────────────────────────────

export interface ApiErrorResponse {
  error: string;
}

// ─── Utility ──────────────────────────────────────────────────────────────────

/** Derives the JWT permission key from a PermissionDto */
export function permissionKey(p: PermissionDto): string {
  return `${p.resource}:${p.action}`;
}
```

---

## 8. Frontend API Service Examples

```typescript
// rbacService.ts
// Production-ready TypeScript service using the Fetch API.
// Replace BASE_URL and getToken() with your application's equivalents.

const BASE_URL = '/api/rbac';

function getToken(): string {
  return localStorage.getItem('access_token') ?? '';
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown
): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    method,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${getToken()}`,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (response.status === 204) {
    return undefined as unknown as T;
  }

  const data = await response.json();

  if (!response.ok) {
    throw new Error((data as ApiErrorResponse).error ?? `HTTP ${response.status}`);
  }

  return data as T;
}

// ─── Roles ────────────────────────────────────────────────────────────────────

export async function getRoles(): Promise<RoleDto[]> {
  return request<RoleDto[]>('GET', '/roles');
}

export async function getRoleById(id: string): Promise<RoleDto> {
  return request<RoleDto>('GET', `/roles/${id}`);
}

export async function getRoleWithPermissions(id: string): Promise<RoleWithPermissionsDto> {
  return request<RoleWithPermissionsDto>('GET', `/roles/${id}/permissions`);
}

export async function createRole(body: CreateRoleRequest): Promise<{ id: string }> {
  return request<{ id: string }>('POST', '/roles', body);
}

export async function updateRole(id: string, body: UpdateRoleRequest): Promise<void> {
  return request<void>('PUT', `/roles/${id}`, body);
}

export async function deleteRole(id: string): Promise<void> {
  return request<void>('DELETE', `/roles/${id}`);
}

// ─── Permissions ──────────────────────────────────────────────────────────────

export async function getPermissions(): Promise<PermissionDto[]> {
  return request<PermissionDto[]>('GET', '/permissions');
}

export async function getPermissionsByRole(roleId: string): Promise<PermissionDto[]> {
  return request<PermissionDto[]>('GET', `/permissions/by-role/${roleId}`);
}

export async function createPermission(body: CreatePermissionRequest): Promise<{ id: string }> {
  return request<{ id: string }>('POST', '/permissions', body);
}

export async function deletePermission(id: string): Promise<void> {
  return request<void>('DELETE', `/permissions/${id}`);
}

// ─── Role ↔ Permission ────────────────────────────────────────────────────────

export async function assignPermission(
  roleId: string,
  permissionId: string
): Promise<void> {
  return request<void>('POST', `/roles/${roleId}/permissions/grant`, { permissionId });
}

export async function removePermission(
  roleId: string,
  permissionId: string
): Promise<void> {
  return request<void>('POST', `/roles/${roleId}/permissions/revoke`, { permissionId });
}

export async function setRolePermissions(
  roleId: string,
  permissionIds: string[]
): Promise<void> {
  return request<void>('PUT', `/roles/${roleId}/permissions`, { permissionIds });
}

// ─── Users ────────────────────────────────────────────────────────────────────

export async function getUsers(params: GetUsersParams = {}): Promise<PagedResultDto<UserSummaryDto>> {
  const qs = new URLSearchParams();
  if (params.email !== undefined)    qs.set('email', params.email);
  if (params.isActive !== undefined) qs.set('isActive', String(params.isActive));
  if (params.pageNumber !== undefined) qs.set('pageNumber', String(params.pageNumber));
  if (params.pageSize !== undefined)   qs.set('pageSize', String(params.pageSize));
  const query = qs.toString() ? `?${qs}` : '';
  return request<PagedResultDto<UserSummaryDto>>('GET', `/users${query}`);
}

export async function getUserWithRoles(userId: string): Promise<UserWithRolesDto> {
  return request<UserWithRolesDto>('GET', `/users/${userId}/roles`);
}

export async function getUserEffectivePermissions(userId: string): Promise<string[]> {
  return request<string[]>('GET', `/users/${userId}/permissions`);
}

// ─── User ↔ Role ──────────────────────────────────────────────────────────────

export async function assignRoleToUser(userId: string, roleId: string): Promise<void> {
  return request<void>('POST', `/users/${userId}/roles/assign`, { roleId });
}

export async function removeRoleFromUser(userId: string, roleId: string): Promise<void> {
  return request<void>('POST', `/users/${userId}/roles/remove`, { roleId });
}

export async function setUserRoles(userId: string, roleIds: string[]): Promise<void> {
  return request<void>('PUT', `/users/${userId}/roles`, { roleIds });
}
```

---

## 9. Search, Filtering and Pagination

The RBAC module exposes filtering and pagination exclusively on the **Get All Users** endpoint (`GET /api/rbac/users`).

### Supported Query Parameters

| Parameter | Type | Default | Behaviour |
|-----------|------|---------|-----------|
| `email` | string | `null` | Case-insensitive substring filter on email address. Example: `email=alice` matches `alice@company.com`. |
| `isActive` | boolean | `null` | When `true`, returns only active users. When `false`, returns only inactive users. When omitted, returns all users. |
| `pageNumber` | integer | `1` | 1-based page index. |
| `pageSize` | integer | `20` | Number of items per page. |

### Pagination Response Envelope

```json
{
  "items": [ ... ],
  "totalCount": 142,
  "pageNumber": 2,
  "pageSize": 20
}
```

**Total pages** can be calculated as: `Math.ceil(totalCount / pageSize)`.

### Search Examples

```
GET /api/rbac/users?email=support&pageNumber=1&pageSize=10
→ Returns page 1 of users whose email contains "support"

GET /api/rbac/users?isActive=true&pageNumber=2&pageSize=25
→ Returns page 2 (items 26–50) of active users

GET /api/rbac/users?email=jones&isActive=false
→ Returns inactive users whose email contains "jones" (page 1, 20 per page)
```

### Sorting

The current API does **not** expose a sort parameter — the server returns results in repository-defined order. Do not pass `sort` or `orderBy` parameters; they will be silently ignored.

### Notes for Role and Permission Endpoints

`GET /api/rbac/roles` and `GET /api/rbac/permissions` return **complete, unfiltered lists** with no pagination or search parameters. Client-side filtering and sorting must be implemented in the frontend for those lists.

```typescript
// Client-side filtering example for roles
const filteredRoles = allRoles.filter(r =>
  r.name.toLowerCase().includes(searchTerm.toLowerCase())
);

// Client-side sort by name
const sorted = [...filteredRoles].sort((a, b) => a.name.localeCompare(b.name));
```

---

## 10. Error Handling Guide

### Standard Error Response Shape

All `4xx` error responses from the RBAC module return a JSON body with the following shape:

```json
{
  "error": "Human-readable error message"
}
```

### Error Reference Table

| Status Code | Endpoint Pattern | Cause | Frontend Action |
|-------------|-----------------|-------|-----------------|
| `400 Bad Request` | POST `/roles` | Duplicate role name | Show inline form error: "A role with this name already exists." |
| `400 Bad Request` | PUT `/roles/{id}` | New name conflicts with another role | Show inline error on name field |
| `400 Bad Request` | PUT `/roles/{id}` | Role is soft-deleted | Show toast: "This role has been deleted and cannot be edited." |
| `400 Bad Request` | DELETE `/roles/{id}` | `IsSystemRole = true` | Disable delete button; show tooltip "System roles cannot be deleted." |
| `400 Bad Request` | POST `/permissions` | Duplicate `resource:action` | Show inline error: "This permission already exists." |
| `400 Bad Request` | POST `/roles/{id}/permissions/grant` | Role or permission not found | Show toast: "The role or permission could not be found." |
| `400 Bad Request` | POST `/roles/{id}/permissions/revoke` | Assignment does not exist | Show toast: "This permission is not assigned to the role." |
| `400 Bad Request` | PUT `/roles/{id}/permissions` | Any permission ID not found | Show toast listing the unknown ID |
| `400 Bad Request` | POST `/users/{id}/roles/assign` | User or role not found | Show toast: "User or role not found." |
| `400 Bad Request` | POST `/users/{id}/roles/remove` | Assignment not active | Show toast: "This role is not currently assigned to the user." |
| `400 Bad Request` | PUT `/users/{id}/roles` | Any role ID not found | Show toast: "One or more roles were not found." |
| `401 Unauthorized` | Any | Missing, expired, or malformed JWT | Redirect to login; clear stored tokens |
| `403 Forbidden` | Any | JWT valid but lacks required permission | Show "Access Denied" message; do not redirect |
| `404 Not Found` | GET `/roles/{id}` | Role does not exist | Show "Role not found"; redirect to list |
| `404 Not Found` | GET `/users/{id}/roles` | User does not exist | Show "User not found" |
| `404 Not Found` | GET `/users/{id}/permissions` | User does not exist | Show "User not found" |

### FluentValidation Error Shape

If a request body fails server-side FluentValidation before reaching the handler, ASP.NET Core returns a `400` with a `ModelState`-style body:

```json
{
  "errors": {
    "name": ["The Name field is required.", "Name must not exceed 80 characters."],
    "resource": ["Resource must be lowercase letters, underscores, or colons."]
  },
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-abc123..."
}
```

**Handling validation errors in the frontend:**

```typescript
async function handleApiError(response: Response, fieldSetter?: (field: string, msg: string) => void) {
  const data = await response.json();

  // FluentValidation / ModelState errors
  if (data.errors) {
    Object.entries(data.errors as Record<string, string[]>).forEach(([field, messages]) => {
      fieldSetter?.(field, messages[0]);
    });
    return;
  }

  // Business rule error from Result<T>
  if (data.error) {
    throw new Error(data.error);
  }
}
```

---

## 11. API Flow Diagrams

### 11.1 Create Role Flow

```
Frontend (Role Form)
        │
        │  POST /api/rbac/roles
        │  { name, description }
        ↓
   [JWT validated]
        │
        │  Permission check: "rbac:role:create"
        ↓
   [Duplicate name check]
        │
   ┌────┴────┐
   │ exists? │
   └────┬────┘
   Yes  │  No
        │   ↓
   400  │  Role.Create(name, description, actorId)
        │   ↓
        │  SaveChangesAsync()
        │   ↓
        │  201 Created { id: newGuid }
        ↓
Frontend → navigate to /roles/{id}/permissions
        │
        │ Refresh role list
```

---

### 11.2 Update Role Flow

```
Frontend (Edit Role Form pre-populated via GET /roles/{id})
        │
        │  PUT /api/rbac/roles/{id}
        │  { name, description }
        ↓
   [JWT validated]
        │
        │  Permission check: "rbac:role:update"
        ↓
   [Role fetched by id]
        │
   ┌────┴──────────┐
   │  not found /  │
   │  deleted?     │
   └────┬──────────┘
   Yes  │  No
   400  │   ↓
        │  [Name uniqueness check — exclude self]
        │   ↓
        │  role.Update(name, description, actorId)
        │   ↓
        │  SaveChangesAsync()
        │   ↓
        │  204 No Content
        ↓
Frontend → show success toast, refresh detail view
```

---

### 11.3 Assign Permission to Role Flow

```
Frontend (Permission Picker)
        │
        │  POST /api/rbac/roles/{roleId}/permissions/grant
        │  { permissionId }
        ↓
   [JWT validated]
        │
        │  Permission check: "rbac:permission:manage"
        ↓
   [Role exists?] [Permission exists?]
        │
   ┌────┴──────────────────┐
   │  Either not found?    │
   └────┬──────────────────┘
   Yes  │  No
   400  │   ↓
        │  [Already assigned? (idempotent check)]
        │   ↓
        │  Already assigned → 204 (no-op)
        │   ↓
        │  RolePermission.Assign(roleId, permissionId, actorId)
        │   ↓
        │  SaveChangesAsync()
        │   ↓
        │  204 No Content
        ↓
Frontend → update local permission list for role
```

---

### 11.4 Assign Role to User Flow

```
Frontend (User Role Management)
        │
        │  POST /api/rbac/users/{userId}/roles/assign
        │  { roleId }
        ↓
   [JWT validated]
        │
        │  Permission check: "rbac:user:assign"
        ↓
   [User exists?] [Role exists?]
        │
   ┌────┴──────────────────┐
   │  Either not found?    │
   └────┬──────────────────┘
   Yes  │  No
   400  │   ↓
        │  [Active assignment already exists? → 204 idempotent]
        │   ↓
        │  [Soft-deleted assignment exists? → Restore()]
        │  else UserRole.Assign(userId, roleId, actorId)
        │   ↓
        │  SaveChangesAsync()
        │   ↓
        │  204 No Content
        ↓
Frontend → refresh user's role list
        │
        ⚠️  Note: User's JWT does NOT auto-update.
            They must log out and log back in to
            receive a JWT with the new role/permissions.
```

---

### 11.5 Permission Check Flow (Client-Side)

```
User navigates to a protected page
        │
        │  Read JWT from storage
        │  Decode payload (base64)
        │  Extract permission[] claims
        ↓
   [Has "required:permission" in claims?]
        │
   ┌────┴──────────────┐
   │  Yes      │  No   │
   └────┬──────┴───┬───┘
        │           │
   Render page   Redirect to /unauthorized
        │         or hide the UI element
        ↓
   Make API call
        │
   [Server validates JWT again]
        │
   [403 received?] → show error, do not redirect
```

---

## 12. Authentication & Authorization Integration

### JWT Storage

Store the JWT in `httpOnly` cookies (preferred for security) or in-memory state. Avoid `localStorage` for tokens due to XSS risk.

```typescript
// Minimal JWT decode utility (no signature verification — for client-side use only)
function decodeJwtPayload(token: string): Record<string, unknown> {
  const base64Payload = token.split('.')[1];
  const json = atob(base64Payload.replace(/-/g, '+').replace(/_/g, '/'));
  return JSON.parse(json);
}
```

### Claims Extraction

After login, decode the JWT to extract the user's permissions:

```typescript
interface JwtPayload {
  sub: string;
  email: string;
  // ASP.NET Core role claim key
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string | string[];
  permission: string | string[];
  exp: number;
  jti: string;
}

function extractPermissions(token: string): string[] {
  const payload = decodeJwtPayload(token) as JwtPayload;
  const raw = payload['permission'];
  if (!raw) return [];
  return Array.isArray(raw) ? raw : [raw];
}

function extractRoles(token: string): string[] {
  const payload = decodeJwtPayload(token) as JwtPayload;
  const raw = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
  if (!raw) return [];
  return Array.isArray(raw) ? raw : [raw];
}
```

### Permission Check Hook (React example)

```typescript
// usePermission.ts
import { useMemo } from 'react';

function usePermission(required: string): boolean {
  const token = getToken(); // your token accessor
  return useMemo(() => {
    if (!token) return false;
    const permissions = extractPermissions(token);
    return permissions.includes(required);
  }, [token, required]);
}

// Usage:
function RoleManagementPage() {
  const canCreate = usePermission('rbac:role:create');
  const canDelete = usePermission('rbac:role:delete');

  return (
    <div>
      {canCreate && <button onClick={openCreateModal}>Create Role</button>}
      {canDelete && <button onClick={handleDelete}>Delete</button>}
    </div>
  );
}
```

### Route Guards

```typescript
// ProtectedRoute.tsx (React Router v6)
import { Navigate } from 'react-router-dom';

interface Props {
  requiredPermission: string;
  children: React.ReactNode;
}

function ProtectedRoute({ requiredPermission, children }: Props) {
  const hasAccess = usePermission(requiredPermission);
  if (!hasAccess) {
    return <Navigate to="/unauthorized" replace />;
  }
  return <>{children}</>;
}

// Router setup:
<Route
  path="/admin/roles"
  element={
    <ProtectedRoute requiredPermission="rbac:role:read">
      <RolesPage />
    </ProtectedRoute>
  }
/>
```

### Conditional Rendering / UI Visibility Rules

| UI Element | Required Permission |
|------------|-------------------|
| View Roles list/detail | `rbac:role:read` |
| Create Role button | `rbac:role:create` |
| Edit Role button | `rbac:role:update` |
| Delete Role button | `rbac:role:delete` (and `isSystemRole === false`) |
| View Permissions | `rbac:role:read` |
| Create Permission button | `rbac:permission:manage` |
| Delete Permission button | `rbac:permission:manage` |
| Grant/Revoke Permission on Role | `rbac:permission:manage` |
| View Users list | `rbac:user:read` |
| Assign/Remove Role on User | `rbac:user:assign` |

### JWT Expiry Handling

```typescript
function isTokenExpired(token: string): boolean {
  const payload = decodeJwtPayload(token);
  const exp = payload['exp'] as number;
  return Date.now() / 1000 > exp;
}

// In your request interceptor:
if (isTokenExpired(getToken())) {
  redirectToLogin();
  throw new Error('Session expired. Please log in again.');
}
```

### Important: JWT Permissions Are Stamped at Login

Because permissions are embedded in the JWT at login time, **changes to a user's roles or permissions do not take effect until they log out and log back in.** The frontend should:

1. Inform administrators of this behaviour via UI copy.
2. Optionally prompt affected users to re-authenticate after significant role changes.

---

## 13. Testing Scenarios

### 13.1 Role Management Tests

| Test Case | Action | Expected Outcome |
|-----------|--------|-----------------|
| Create role with valid data | POST `/roles` with `{ name: "Tester", description: "QA" }` | `201` with new GUID in body |
| Create role with duplicate name | POST `/roles` with name matching existing role | `400` with duplicate-name error |
| Create role — name exceeds 80 chars | POST `/roles` with 81-char name | `400` validation error |
| Create role — description exceeds 500 chars | POST `/roles` with 501-char description | `400` validation error |
| Get all roles — authenticated | GET `/roles` with valid JWT | `200` with array |
| Get all roles — unauthenticated | GET `/roles` without token | `401` |
| Get all roles — insufficient permission | GET `/roles` with JWT lacking `rbac:role:read` | `403` |
| Get role by ID — exists | GET `/roles/{validId}` | `200` with `RoleDto` |
| Get role by ID — not found | GET `/roles/{nonExistentId}` | `404` |
| Update role — valid | PUT `/roles/{id}` with new name | `204` |
| Update role — system role name check | PUT `/roles/{systemRoleId}` | `204` (update allowed; only delete is blocked) |
| Delete role — non-system | DELETE `/roles/{id}` | `204`, cascades user-role soft-deletes |
| Delete role — system role | DELETE `/roles/{systemRoleId}` | `400` "System roles cannot be deleted." |
| Delete role — already deleted | DELETE `/roles/{deletedId}` | `400` "Role is already deleted." |

### 13.2 Permission Management Tests

| Test Case | Action | Expected Outcome |
|-----------|--------|-----------------|
| Create permission — valid | POST `/permissions` `{ resource: "report", action: "export" }` | `201` |
| Create permission — uppercase resource | POST `/permissions` `{ resource: "Report" }` | `400` — must match `^[a-z_:]+$` |
| Create permission — action with numbers | POST `/permissions` `{ action: "export2" }` | `400` — must match `^[a-z_]+$` |
| Create permission — duplicate | POST `/permissions` with existing `resource:action` | `400` |
| Delete permission — cascades to roles | DELETE `/permissions/{id}` | `204`, revokes from all roles |
| Get all permissions — valid | GET `/permissions` | `200` array |
| Get permissions by role — valid | GET `/permissions/by-role/{roleId}` | `200` array of assigned permissions |

### 13.3 Role Permission Assignment Tests

| Test Case | Action | Expected Outcome |
|-----------|--------|-----------------|
| Grant permission — success | POST `/roles/{id}/permissions/grant` | `204` |
| Grant permission — idempotent | Grant same permission twice | Both return `204`, no duplicate record |
| Grant permission — bad role ID | POST with non-existent `roleId` | `400` |
| Grant permission — bad permission ID | POST with non-existent `permissionId` | `400` |
| Revoke permission — success | POST `/roles/{id}/permissions/revoke` | `204` |
| Revoke permission — not assigned | Revoke a permission not on the role | `400` |
| Set permissions — atomic replace | PUT `/roles/{id}/permissions` with new list | `204`, old ones gone, new ones present |
| Set permissions — empty array | PUT with `permissionIds: []` | `204`, all permissions removed |
| Set permissions — invalid ID in list | PUT with one invalid GUID in array | `400`, operation aborted |

### 13.4 User Role Assignment Tests

| Test Case | Action | Expected Outcome |
|-----------|--------|-----------------|
| Assign role — success | POST `/users/{id}/roles/assign` | `204` |
| Assign role — idempotent | Assign same role twice | Both return `204` |
| Assign role — restore soft-deleted | Assign a previously removed role | `204`, `AssignedAt` refreshed |
| Assign role — user not found | POST with non-existent `userId` | `400` |
| Assign role — role not found | POST with non-existent `roleId` | `400` |
| Remove role — success | POST `/users/{id}/roles/remove` | `204` |
| Remove role — not assigned | Remove a role not held by user | `400` |
| Set user roles — atomic | PUT `/users/{id}/roles` with new list | `204` |
| Get user with roles — includes all active | GET `/users/{id}/roles` | `200` with non-deleted roles only |
| Get effective permissions | GET `/users/{id}/permissions` | `200` with flat `resource:action` strings |

### 13.5 Authorization Tests

| Test Case | Expected Outcome |
|-----------|-----------------|
| Call any RBAC endpoint without JWT | `401 Unauthorized` |
| Call endpoint with expired JWT | `401 Unauthorized` |
| Call create-role endpoint without `rbac:role:create` claim | `403 Forbidden` |
| Call manage-permission endpoint without `rbac:permission:manage` | `403 Forbidden` |
| Call assign-user endpoint without `rbac:user:assign` | `403 Forbidden` |
| User with `rbac:role:read` calls GET endpoints only | `200` on GET, `403` on POST/PUT/DELETE |

---

## 14. Frontend Integration Checklist

### Authentication

- [ ] JWT is stored securely (httpOnly cookie or in-memory; not `localStorage`)
- [ ] Token expiry is checked before every API request
- [ ] Expired token triggers redirect to login and clears all stored tokens
- [ ] `Authorization: Bearer <token>` header is sent on every RBAC request
- [ ] `401` responses redirect the user to the login page
- [ ] `403` responses show an "Access Denied" message without redirecting

### Role Management

- [ ] Roles list fetched on mount; loading and error states handled
- [ ] System roles (`isSystemRole: true`) show no Edit/Delete controls
- [ ] Role name client-side validation: required, max 80 characters
- [ ] Role description client-side validation: max 500 characters (optional)
- [ ] Duplicate name `400` error surfaces as an inline form error
- [ ] Delete action shows a confirmation dialog before calling API
- [ ] Successful create navigates to the role's permissions screen using the returned `id`
- [ ] Successful delete removes the role from local state immediately

### Permission Management

- [ ] All permissions listed in the permission management screen
- [ ] `resource` field validated: required, max 60 chars, regex `^[a-z_:]+$`
- [ ] `action` field validated: required, max 60 chars, regex `^[a-z_]+$`
- [ ] Composite key preview (`resource:action`) shown in real time as user types
- [ ] Duplicate permission `400` surfaced as inline error
- [ ] Delete shows warning that the permission will be removed from all roles

### User Assignment

- [ ] User list is paginated; page number and size are passed as query params
- [ ] Email search filter is debounced before triggering API call
- [ ] Active/inactive filter is reflected in the URL for shareability
- [ ] Role assignment is idempotent — UI does not disable already-assigned roles
- [ ] Users are warned that role changes take effect only after re-login
- [ ] Effective permissions displayed on user detail screen via `GET /users/{id}/permissions`

### API Integration

- [ ] All requests use a centralized service layer (see Section 8)
- [ ] Request errors are caught and surfaced with user-friendly messages
- [ ] `204 No Content` responses are handled (do not attempt to parse body)
- [ ] `404` responses show a clear "not found" message and navigation back
- [ ] FluentValidation `ModelState` error format is parsed and field errors mapped

### Error Handling

- [ ] `ApiErrorResponse.error` field is extracted and shown to the user
- [ ] `ModelState` validation errors are parsed and applied to form fields
- [ ] Network errors (fetch failure) are caught and show a generic retry message
- [ ] Errors do not leak internal server details to the user

### Security

- [ ] UI elements are hidden or disabled based on JWT `permission` claims
- [ ] Route guards prevent navigation to unauthorized pages
- [ ] Client-side permission checks are treated as UX only — server is always the source of truth
- [ ] No sensitive data (other than the JWT itself) is stored in `localStorage`

### Testing

- [ ] All 400 error scenarios tested with mocked API responses
- [ ] Idempotent operations tested with double-submit scenarios
- [ ] Expired JWT scenario tested (simulate with short-lived test token)
- [ ] Permission-missing scenario tested for every protected route
- [ ] Pagination navigation tested: first page, last page, beyond bounds

---

## 15. Common Frontend Workflows

### Workflow 1: Create a Role

**Step-by-step:**

1. Navigate to `/admin/roles` — requires `rbac:role:read` to see the page.
2. Click "Create Role" button (visible only if user has `rbac:role:create`).
3. Fill in `name` (required, max 80 chars) and optionally `description` (max 500 chars).
4. Client-side validates fields; shows inline errors if invalid.
5. Submit → `POST /api/rbac/roles` with `{ name, description }`.
6. On `201`: extract `id` from response, navigate to `/admin/roles/{id}/permissions`.
7. On `400`: display `error` field as inline form message.

```typescript
async function handleCreateRole(name: string, description: string) {
  try {
    const { id } = await createRole({ name, description });
    router.push(`/admin/roles/${id}/permissions`);
    toast.success('Role created successfully.');
  } catch (err) {
    setFormError('name', (err as Error).message);
  }
}
```

---

### Workflow 2: Create a Permission

**Step-by-step:**

1. Navigate to `/admin/permissions` — requires `rbac:role:read`.
2. Click "New Permission" button (visible only if `rbac:permission:manage`).
3. Enter `resource` and `action`. Show live preview: `resource:action`.
4. Client-side validates: both fields required, regex patterns enforced.
5. Submit → `POST /api/rbac/permissions`.
6. On `201`: close modal, refresh permissions list.
7. On `400`: show error message inline.

```typescript
async function handleCreatePermission(resource: string, action: string, description?: string) {
  if (!/^[a-z_:]+$/.test(resource)) {
    setError('resource', 'Must be lowercase letters, underscores, or colons.');
    return;
  }
  if (!/^[a-z_]+$/.test(action)) {
    setError('action', 'Must be lowercase letters and underscores only.');
    return;
  }
  try {
    await createPermission({ resource, action, description });
    await refreshPermissions();
    closeModal();
    toast.success(`Permission "${resource}:${action}" created.`);
  } catch (err) {
    toast.error((err as Error).message);
  }
}
```

---

### Workflow 3: Assign Permission to Role

**Step-by-step:**

1. Navigate to `/admin/roles/{id}` — requires `rbac:role:read`.
2. Fetch role with permissions: `GET /api/rbac/roles/{id}/permissions`.
3. Fetch all permissions: `GET /api/rbac/permissions`.
4. Display two-panel picker: "Assigned" (left) and "Available" (right).
5. User checks a permission to assign → `POST /api/rbac/roles/{id}/permissions/grant`.
6. User unchecks a permission to revoke → `POST /api/rbac/roles/{id}/permissions/revoke`.
7. Alternatively, "Save All" button → `PUT /api/rbac/roles/{id}/permissions` with full current selection.

```typescript
// Toggling a single permission
async function togglePermission(roleId: string, permId: string, isAssigned: boolean) {
  try {
    if (isAssigned) {
      await removePermission(roleId, permId);
    } else {
      await assignPermission(roleId, permId);
    }
    await refreshRolePermissions(roleId);
  } catch (err) {
    toast.error((err as Error).message);
  }
}

// Saving entire selection atomically
async function saveAllPermissions(roleId: string, selectedIds: string[]) {
  try {
    await setRolePermissions(roleId, selectedIds);
    toast.success('Permissions updated.');
  } catch (err) {
    toast.error((err as Error).message);
  }
}
```

---

### Workflow 4: Assign Role to User

**Step-by-step:**

1. Navigate to `/admin/users` — requires `rbac:user:read`.
2. Search users by email using `GET /api/rbac/users?email=...`.
3. Click user to open detail: `GET /api/rbac/users/{id}/roles`.
4. View current roles. Click "Assign Role" (requires `rbac:user:assign`).
5. Select a role from the roles list.
6. Submit → `POST /api/rbac/users/{id}/roles/assign` with `{ roleId }`.
7. Refresh user roles display.
8. **Remind admin**: the user must re-login for the new role to be reflected in their session.

```typescript
async function handleAssignRole(userId: string, roleId: string) {
  try {
    await assignRoleToUser(userId, roleId);
    await refreshUserRoles(userId);
    toast.success('Role assigned. User must log out and log back in for changes to take effect.');
  } catch (err) {
    toast.error((err as Error).message);
  }
}
```

---

### Workflow 5: Client-Side Permission Validation

**Step-by-step:**

1. At login, decode the JWT and extract all `permission` claim values.
2. Store the permissions array in your auth state/context.
3. Before rendering action buttons or navigating to protected routes, check the permission array.
4. When an API call returns `403` (e.g. token was refreshed with different claims), show "Access Denied" and do not retry.

```typescript
// Permission context
const AuthContext = React.createContext<{ permissions: string[] }>({ permissions: [] });

// Hook
function usePermission(key: string): boolean {
  const { permissions } = useContext(AuthContext);
  return permissions.includes(key);
}

// Conditional button rendering
function RoleActionsMenu({ role }: { role: RoleDto }) {
  const canEdit   = usePermission('rbac:role:update');
  const canDelete = usePermission('rbac:role:delete') && !role.isSystemRole;

  return (
    <menu>
      {canEdit   && <button onClick={() => openEdit(role)}>Edit</button>}
      {canDelete && <button onClick={() => confirmDelete(role)}>Delete</button>}
    </menu>
  );
}

// API call guard
async function safeDeleteRole(id: string) {
  if (!usePermission('rbac:role:delete')) {
    toast.error('You do not have permission to delete roles.');
    return;
  }
  try {
    await deleteRole(id);
    toast.success('Role deleted.');
  } catch (err) {
    if ((err as Error).message.includes('System roles')) {
      toast.error('System roles cannot be deleted.');
    } else {
      toast.error('Failed to delete role: ' + (err as Error).message);
    }
  }
}
```

---

*End of RBAC Frontend Integration Guide*

---

**Document generated from source code analysis of the Adrenalin Unify Platform.**  
**Controllers:** `RolesController`, `PermissionsController`, `UsersRbacController`  
**Module:** `Adrenalin.Modules.Auth`  
**Authorization:** `PermissionAuthorizationHandler`, `PermissionPolicyProvider`, `PermissionRequirement`
