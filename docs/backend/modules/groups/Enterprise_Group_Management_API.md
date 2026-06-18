# Enterprise Group Management & Ticket Routing

This document details the API endpoints, input/output schemas, RBAC requirements, and frontend integration guidelines for the newly upgraded Enterprise Group Management and Ticket Routing features.

## 1. Routing Rules API (`/api/routing-rules`)
This module allows administrators to define explicit routing rules for automatic ticket assignment based on multiple criteria. Rules are evaluated in a priority cascade.

### 1.1 Get All Routing Rules
- **Endpoint**: `GET /api/routing-rules`
- **Query Params**: `companyId` (optional) - Filter rules by a specific company.
- **RBAC Policy**: `ticket:manage`
- **Response** `200 OK`:
  ```json
  [
    {
      "id": "guid",
      "companyId": "guid",
      "companyName": "string",
      "groupId": "guid",
      "groupName": "string",
      "moduleId": "guid (optional)",
      "moduleName": "string (optional)",
      "regionCode": "string (optional)",
      "tierCode": "string (optional)",
      "priority": "int (optional)",
      "ticketType": "int (optional)",
      "keywords": "string (optional)",
      "rulePriority": 100,
      "isDefault": false,
      "createdAt": "datetime",
      "updatedAt": "datetime (optional)"
    }
  ]
  ```

### 1.2 Get Routing Rule by ID
- **Endpoint**: `GET /api/routing-rules/{id}`
- **RBAC Policy**: `ticket:manage`
- **Response** `200 OK`: `RoutingRuleDto` (Same as above)
- **Response** `404 Not Found`: `{ "error": "Routing rule {id} not found." }`

### 1.3 Create Routing Rule
- **Endpoint**: `POST /api/routing-rules`
- **RBAC Policy**: `ticket:manage`
- **Input Payload**:
  ```json
  {
    "companyId": "guid",
    "groupId": "guid",
    "moduleId": "guid (optional)",
    "regionCode": "string (optional)",
    "tierCode": "string (optional)",
    "priority": "int (optional)",
    "ticketType": "int (optional)",
    "keywords": "string (optional)",
    "rulePriority": 100,
    "isDefault": false
  }
  ```
- **Response** `201 Created`: Returns the rule ID.

### 1.4 Update Routing Rule
- **Endpoint**: `PUT /api/routing-rules/{id}`
- **RBAC Policy**: `ticket:manage`
- **Input Payload**: Same as Create (excluding `companyId`).
- **Response** `200 OK`: `{ "message": "Routing rule updated successfully." }`

### 1.5 Delete Routing Rule
- **Endpoint**: `DELETE /api/routing-rules/{id}`
- **RBAC Policy**: `ticket:manage`
- **Response** `200 OK`: `{ "message": "Routing rule deleted successfully." }`

### 1.6 Simulate Routing Rule
- **Endpoint**: `POST /api/routing-rules/simulate`
- **RBAC Policy**: `ticket:manage`
- **Description**: Simulates the 4-tier routing cascade without creating a ticket. Traces which rules evaluated to true/false.
- **Input Payload**:
  ```json
  {
    "companyId": "guid",
    "moduleId": "guid (optional)",
    "priority": "int (optional)",
    "type": "int (optional)",
    "title": "string (optional)",
    "description": "string (optional)"
  }
  ```
- **Response** `200 OK`:
  ```json
  {
    "resolvedGroupId": "guid (optional)",
    "resolvedAgentId": "guid (optional)",
    "finalStrategy": "CompanyExplicit | CategoryMatch | RegionMatch | Fallback | None",
    "finalReason": "string",
    "traces": [
      {
        "evaluationStep": "string",
        "matched": true,
        "reason": "string",
        "ruleId": "guid (optional)"
      }
    ]
  }
  ```

### 1.7 Get Routing Rule History
- **Endpoint**: `GET /api/routing-rules/{id}/history`
- **RBAC Policy**: `ticket:manage`
- **Description**: Returns the audit trail for a specific routing rule, tracking creations, updates, and deletions over time.
- **Response** `200 OK`:
  ```json
  [
    {
      "id": "guid",
      "ruleId": "guid",
      "action": "Created",
      "oldValues": null,
      "newValues": "{\"GroupId\": \"guid\", \"Priority\": 1}",
      "changedBy": "guid",
      "changedByName": "John Doe",
      "changedAt": "datetime",
      "ipAddress": "192.168.1.1"
    }
  ]
  ```

---

## 2. Group Dashboards & Queues API (`/api/groups`)

### 2.1 Get Group Dashboard
- **Endpoint**: `GET /api/groups/{id}/dashboard`
- **RBAC Policy**: Authenticated user (`Authorize`). User must be a member of the group, a team lead, or have admin privileges.
- **Response** `200 OK`:
  ```json
  {
    "groupId": "guid",
    "groupName": "string",
    "totalUnassigned": 5,
    "totalOverdue": 2,
    "totalCritical": 1,
    "agentWorkloads": {
      "agentId_1": 4,
      "agentId_2": 2
    }
  }
  ```

### 2.2 Get Group Queue
- **Endpoint**: `GET /api/groups/{id}/queue`
- **Query Params**:
  - `queueType` (string) - `all`, `unassigned`, `assigned`, `overdue`, `critical` (Default: `all`)
  - `page` (int) - Default: 1
  - `pageSize` (int) - Default: 20
- **RBAC Policy**: Authenticated user (`Authorize`). User must be a member of the group, a team lead, or have admin privileges.
- **Response** `200 OK`:
  ```json
  {
    "items": [
      {
        "id": "guid",
        "title": "string",
        "priority": 1,
        "status": 1,
        "assigneeId": "guid",
        "assigneeName": "string",
        "dueDate": "datetime"
      }
    ],
    "totalCount": 50,
    "page": 1,
    "pageSize": 20
  }
  ```

### 2.3 Get Lead Dashboard
- **Endpoint**: `GET /api/groups/lead-dashboard`
- **RBAC Policy**: Authenticated user (`Authorize`). User must be a Team Lead.
- **Response** `200 OK`:
  ```json
  {
    "leadId": "guid",
    "managedGroups": [
      {
        "groupId": "guid",
        "groupName": "string",
        "unassignedTickets": 3,
        "criticalTickets": 1
      }
    ]
  }
  ```

### 2.4 Get Companies Assigned to Group
- **Endpoint**: `GET /api/groups/{id}/companies`
- **RBAC Policy**: `user:manage`
- **Response** `200 OK`: Array of `CompanyGroupDto`.

### 2.5 Get Group Routing Preview
- **Endpoint**: `GET /api/groups/{id}/routing-preview`
- **RBAC Policy**: `user:manage`
- **Description**: Shows all companies routing to this group (by explicit rule or default fallback).
- **Response** `200 OK`:
  ```json
  {
    "groupId": "guid",
    "groupName": "string",
    "defaultForCompanies": [
      {
        "companyId": "guid",
        "companyName": "string"
      }
    ],
    "explicitRulesTargetingGroup": [
      {
        "id": "guid",
        "companyId": "guid",
        "companyName": "string",
        "rulePriority": 100
      }
    ]
  }
  ```

---

## 3. Company ↔ Group Mappings (`/api/companies`)

### 3.1 Get Company Groups
- **Endpoint**: `GET /api/companies/{id}/groups`
- **RBAC Policy**: `company:read`
- **Response** `200 OK`:
  ```json
  [
    {
      "groupId": "guid",
      "groupName": "string",
      "isDefault": true,
      "priority": 0
    }
  ]
  ```

### 3.2 Assign Group to Company
- **Endpoint**: `POST /api/companies/{id}/groups`
- **RBAC Policy**: `company:update`
- **Input Payload**:
  ```json
  {
    "groupId": "guid",
    "isDefault": false,
    "priority": 0
  }
  ```
- **Response** `200 OK`: `{ "message": "Company assigned to group successfully." }`

### 3.3 Remove Group from Company
- **Endpoint**: `DELETE /api/companies/{id}/groups/{groupId}`
- **RBAC Policy**: `company:update`
- **Response** `200 OK`: `{ "message": "Company removed from group successfully." }`

### 3.4 Set Default Group for Company
- **Endpoint**: `POST /api/companies/{id}/groups/{groupId}/default`
- **RBAC Policy**: `company:update`
- **Response** `200 OK`: `{ "message": "Group set as default successfully." }`

### 3.5 Get Company Ticket Metrics
- **Endpoint**: `GET /api/companies/{id}/ticket-metrics`
- **RBAC Policy**: `company:read`
- **Description**: Returns aggregated metrics for a specific company's tickets.
- **Response** `200 OK`:
  ```json
  {
    "companyId": "guid",
    "totalTickets": 100,
    "unassignedTickets": 5,
    "openTickets": 20,
    "criticalTickets": 2,
    "slaBreachedTickets": 3,
    "resolvedTickets": 80
  }
  ```

### 3.6 Get Company Routing Preview
- **Endpoint**: `GET /api/companies/{id}/routing-preview`
- **RBAC Policy**: `company:read`
- **Description**: Previews the routing cascade for a company, showing explicit rules and fallback defaults.
- **Response** `200 OK`:
  ```json
  {
    "companyId": "guid",
    "companyName": "string",
    "defaultGroupId": "guid",
    "defaultGroupName": "string",
    "explicitRules": [
      {
        "id": "guid",
        "groupId": "guid",
        "groupName": "string",
        "rulePriority": 100
      }
    ]
  }
  ```

---

## 4. Enterprise Group Members & Leaders API (`/api/groups`)

### 4.1 Get Enterprise Group Members
- **Endpoint**: `GET /api/groups/{id}/members`
- **RBAC Policy**: `user:manage`
- **Response** `200 OK`:
  ```json
  [
    {
      "userId": "guid",
      "firstName": "string",
      "lastName": "string",
      "email": "string",
      "isLead": false,
      "leadRole": null,
      "joinedAt": "datetime",
      "roles": ["Agent", "IT Support"]
    }
  ]
  ```

### 4.2 Get Group Leaders (Multi-Lead Support)
- **Endpoint**: `GET /api/groups/{id}/leaders`
- **RBAC Policy**: `user:manage`
- **Response** `200 OK`:
  ```json
  [
    {
      "userId": "guid",
      "firstName": "string",
      "lastName": "string",
      "email": "string",
      "leadRole": "Primary", // Primary or Secondary
      "assignedAt": "datetime"
    }
  ]
  ```

### 4.3 Add Member to Group
- **Endpoint**: `POST /api/groups/{id}/members/add`
- **RBAC Policy**: `user:manage`
- **Input Payload**:
  ```json
  {
    "userId": "guid",
    "isLead": false,
    "leadRole": null // 1 for Primary, 2 for Secondary
  }
  ```
- **Response** `204 NoContent`

### 4.4 Set Group Lead Status
- **Endpoint**: `PATCH /api/groups/{id}/members/{userId}/lead`
- **RBAC Policy**: `user:manage`
- **Input Payload**:
  ```json
  {
    "isLead": true,
    "leadRole": 1 // 1 for Primary, 2 for Secondary
  }
  ```
- **Response** `204 NoContent`

### 4.5 Get Group Workload Distribution
- **Endpoint**: `GET /api/groups/{id}/workload`
- **RBAC Policy**: `user:manage`
- **Response** `200 OK`:
  ```json
  {
    "groupId": "guid",
    "totalActiveTickets": 10,
    "agentWorkloads": [
      {
        "agentId": "guid",
        "agentName": "string",
        "activeTickets": 4,
        "isAtCapacity": false
      }
    ]
  }
  ```

---

## 5. Ticket Self-Assignment API (`/api/tickets`)

### 5.1 Claim Ticket
- **Endpoint**: `POST /api/tickets/{id}/claim`
- **RBAC Policy**: `ticket:assign`
- **Description**: Allows an agent to self-assign a ticket from their group's queue. The agent must be a member of the group the ticket belongs to.
- **Response** `200 OK`:
  ```json
  {
    "message": "Ticket claimed successfully.",
    "ticketId": "guid"
  }
  ```

### 5.2 Bulk Assign Tickets
- **Endpoint**: `POST /api/tickets/bulk-assign`
- **RBAC Policy**: `ticket:assign`
- **Description**: Allows bulk assignment of multiple tickets to an agent, a group, or both. Includes partial success handling.
- **Request Body**:
  ```json
  {
    "ticketIds": ["guid1", "guid2"],
    "agentId": "guid", // Optional
    "groupId": "guid"  // Optional
  }
  ```
- **Response** `200 OK`:
  ```json
  {
    "successfulCount": 2,
    "failedCount": 0,
    "errors": []
  }
  ```

---

## 6. Frontend Implementation Guidelines

### 4.1 Routing Rules Configuration UI
**Target Audience**: ITSM Administrators / Service Managers
- **Implementation**: Create a dedicated settings page (`/admin/routing-rules`) where admins can define rules.
- **UI Elements**: 
  - A sortable datagrid/table to display existing rules, ordered by `RulePriority`.
  - A complex form/modal for rule creation:
    - Autocomplete dropdowns for `Company`, `Group`, `Module`.
    - Selectors for `RegionCode`, `TierCode`, `Priority`, and `TicketType`.
    - A tag-input field for `Keywords`.
  - Ensure users can drag-and-drop rows to reorder rules (this will update `RulePriority` across multiple records).

### 4.2 Group Lead Dashboard
**Target Audience**: Team Leads
- **Implementation**: A multi-group monitoring dashboard (`/dashboards/lead`).
- **UI Elements**: 
  - A summary KPI bar showing total unassigned and critical tickets across all managed groups.
  - Cards or tabs for each group from `managedGroups`.
  - Within each group view, display the agent workload distribution (bar chart or list) derived from `agentWorkloads` in the `GetDashboard` endpoint.

### 4.3 Agent Queue View
**Target Audience**: Support Agents
- **Implementation**: Enhance the standard ticket list (`/tickets`).
- **UI Elements**:
  - Filter tabs mapping to the `queueType` parameter (`All`, `Unassigned`, `Assigned to Me`, `Overdue`, `Critical`).
  - Polling or WebSockets to refresh the queue counts frequently.
  - When clicking "Unassigned", the agent should see the shared pool for their group and be able to self-assign tickets.

### 5.4 Company Profiles
**Target Audience**: Account Managers / ITSM Admins
- **Implementation**: Under the Company profile page (`/companies/{id}`).
- **UI Elements**:
  - A "Support Groups" tab calling `GET /api/companies/{id}/groups`.
  - A button to "Assign Support Group" opening a modal with a group search dropdown.
  - A star/radio toggle to designate one group as the `isDefault` fallback.

### 5.5 Enterprise Group Member Management
**Target Audience**: ITSM Admins / Team Leads
- **Implementation**: Under the Group settings page (`/groups/{id}/members`).
- **UI Elements**:
  - A rich datatable displaying members calling `GET /api/groups/{id}/members`.
  - Columns for Name, Email, Roles, and Lead Status.
  - An action menu to assign a member as a Lead calling `PATCH /api/groups/{id}/members/{userId}/lead`, showing a dropdown to select Primary or Secondary lead role.
  - A "Group Leaders" widget calling `GET /api/groups/{id}/leaders` to summarize the current command structure.
