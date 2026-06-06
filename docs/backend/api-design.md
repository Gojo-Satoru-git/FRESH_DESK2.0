# 🌐 Ticketing REST API Design

This document details the REST API specifications for the Ticketing feature module in Adrenalin. All endpoints are hosted under the `TicketsController` in `Adrenalin.unify.API`.

---

## 📌 Base Route: `/api/tickets`

| Method | Endpoint | Description |
| :--- | :--- | :--- |
| **POST** | `/api/tickets` | Create a new support ticket |
| **GET** | `/api/tickets` | Query and filter tickets (paginated) |
| **GET** | `/api/tickets/{ticketId}` | Get ticket details by ID |
| **POST** | `/api/tickets/{ticketId}/assign` | Assign an agent and optional notes to a ticket |
| **POST** | `/api/tickets/{ticketId}/status` | Change the status of a ticket |
| **POST** | `/api/tickets/{ticketId}/comments` | Add a comment (public or internal) |
| **POST** | `/api/tickets/{ticketId}/watchers/{userId}` | Add a user as a watcher |
| **DELETE** | `/api/tickets/{ticketId}/watchers/{userId}` | Remove a watcher from a ticket |
| **POST** | `/api/tickets/{ticketId}/relations` | Relate or link two tickets |
| **POST** | `/api/tickets/{ticketId}/attachments` | Upload an attachment (multipart/form-data) |
| **GET** | `/api/tickets/{ticketId}/attachments/{attachmentId}` | Fetch and download a ticket attachment |
| **DELETE** | `/api/tickets/{ticketId}/attachments/{attachmentId}` | Delete a ticket attachment |
| **POST** | `/api/tickets/{ticketId}/merge` | Merge a duplicate ticket into this master ticket |
| **POST** | `/api/tickets/{ticketId}/close` | Close a resolved ticket |
| **POST** | `/api/tickets/{ticketId}/reopen` | Reopen a resolved/closed ticket |
| **POST** | `/api/tickets/{ticketId}/resolve` | Resolve an open ticket |
| **GET** | `/api/tickets/{ticketId}/history` | Retrieve the status change log history |
| **POST** | `/api/tickets/{ticketId}/split` | Split a ticket into a new ticket, moving comments/attachments |

---

## 🛠️ Endpoint Specifications

### 1. Create Ticket
- **URL**: `/api/tickets`
- **Method**: `POST`
- **Payload**:
```json
{
  "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "moduleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "subject": "System crash on database connection loss",
  "description": "The backend system throws an unhandled exception when database is offline...",
  "contactId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```
- **Validation Rules** (`CreateTicketCommandValidator`):
  - `companyId`: Must not be empty.
  - `moduleId`: Must not be empty.
  - `subject`: Required, max length 500 characters.
  - `description`: Required, max length 10000 characters.
- **Success Response** (`200 OK`):
```json
{
  "message": "Ticket created successfully.",
  "ticketId": "8f86fce8-b5fe-4e50-bd6b-80df24be9719"
}
```

---

### 2. Get Tickets (Paginated Query)
- **URL**: `/api/tickets`
- **Method**: `GET`
- **Query Parameters**:
  - `ticketNumber` (string, optional)
  - `status` (string, optional)
  - `assignedAgentId` (Guid, optional)
  - `companyId` (Guid, optional)
  - `page` (int, default = 1)
  - `pageSize` (int, default = 20)
- **Success Response** (`200 OK`):
```json
{
  "items": [
    {
      "id": "8f86fce8-b5fe-4e50-bd6b-80df24be9719",
      "ticketNumber": "TK-1002",
      "subject": "System crash on database connection loss",
      "status": "New",
      "assignedAgentId": null,
      "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "createdAt": "2026-06-06T13:15:14Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 20,
  "totalCount": 1,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

---

### 3. Get Ticket by ID
- **URL**: `/api/tickets/{ticketId}`
- **Method**: `GET`
- **Success Response** (`200 OK`):
```json
{
  "id": "8f86fce8-b5fe-4e50-bd6b-80df24be9719",
  "ticketNumber": "TK-1002",
  "subject": "System crash on database connection loss",
  "description": "The backend system throws an unhandled exception...",
  "status": "New",
  "assignedAgentId": null,
  "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdAt": "2026-06-06T13:15:14Z",
  "comments": [],
  "statusHistory": [
    {
      "fromStatus": null,
      "toStatus": "New",
      "changedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "changedAt": "2026-06-06T13:15:14Z",
      "reason": "Ticket Created"
    }
  ],
  "assignmentLogs": [],
  "watchers": [],
  "relations": [],
  "attachments": []
}
```

---

### 4. Assign Ticket
- **URL**: `/api/tickets/{ticketId}/assign`
- **Method**: `POST`
- **Payload**:
```json
{
  "agentId": "9a75dce8-b5fe-4e50-bd6b-80df24be9720",
  "assignedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "notes": "Assigning to db expert team"
}
```
- **Validation Rules** (`AssignTicketCommandValidator`):
  - `agentId`: Must not be empty.
  - `assignedBy`: Must not be empty.
- **Success Response** (`200 OK`):
```json
{
  "message": "Ticket assigned successfully.",
  "ticketId": "8f86fce8-b5fe-4e50-bd6b-80df24be9719",
  "agentId": "9a75dce8-b5fe-4e50-bd6b-80df24be9720"
}
```

---

### 5. Change Ticket Status
- **URL**: `/api/tickets/{ticketId}/status`
- **Method**: `POST`
- **Payload**:
```json
{
  "newStatus": "InProgress",
  "changedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reason": "Developer started analyzing logs"
}
```
- **Validation Rules** (`ChangeTicketStatusCommandValidator`):
  - `newStatus`: Must be a valid `TicketStatus` value.
  - `changedBy`: Must not be empty.
- **Success Response** (`200 OK`):
```json
{
  "message": "Ticket status updated successfully.",
  "ticketId": "8f86fce8-b5fe-4e50-bd6b-80df24be9719",
  "newStatus": "InProgress"
}
```

---

### 6. Add Comment
- **URL**: `/api/tickets/{ticketId}/comments`
- **Method**: `POST`
- **Payload**:
```json
{
  "authorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "contactId": null,
  "body": "This is an internal note analyzing the logs.",
  "visibility": "Internal"
}
```
- **Domain Constraints**:
  - Either `authorId` (Agent) or `contactId` (Customer) must be provided, but **not both**.
  - Customer comments (`contactId` present) **cannot** be marked with `Internal` visibility.
  - Comment body must not be empty and has a maximum length of 10,000 characters.
- **Success Response** (`200 OK`):
```json
"c8d6fce8-b5fe-4e50-bd6b-80df24be9721"
```

---

### 7. Manage Watchers (Add / Remove)
- **Add Watcher**:
  - **URL**: `/api/tickets/{ticketId}/watchers/{userId}?addedBy={addedById}`
  - **Method**: `POST`
  - **Success Response**: `204 No Content`
- **Remove Watcher**:
  - **URL**: `/api/tickets/{ticketId}/watchers/{userId}`
  - **Method**: `DELETE`
  - **Success Response**: `204 No Content`

---

### 8. Link Tickets (Create Relation)
- **URL**: `/api/tickets/{ticketId}/relations`
- **Method**: `POST`
- **Payload**:
```json
{
  "childTicketId": "9b86fce8-b5fe-4e50-bd6b-80df24be9799",
  "relationType": "DependsOn"
}
```
- **Relation Types**: `Related` (1), `Duplicate` (2), `ParentChild` (3), `BlockedBy` (4), `DependsOn` (5), `MergedInto` (6), `SplitFrom` (7).
- **Success Response** (`200 OK`): Returns the newly created relation ID.

---

### 9. Attachments Management (Upload / Download / Delete)
- **Upload Attachment**:
  - **URL**: `/api/tickets/{ticketId}/attachments`
  - **Method**: `POST`
  - **Content-Type**: `multipart/form-data`
  - **Form Data**:
    - `File`: Binary file stream (Max 50MB)
    - `CommentId`: Guid (Optional, to link the attachment to a specific comment)
    - `UploadedBy`: Guid (Required)
  - **Success Response** (`200 OK`): Returns the attachment ID.
- **Download Attachment**:
  - **URL**: `/api/tickets/{ticketId}/attachments/{attachmentId}`
  - **Method**: `GET`
  - **Success Response**: Binary file stream with the correct `Content-Type` and `Content-Disposition`.
- **Delete Attachment**:
  - **URL**: `/api/tickets/{ticketId}/attachments/{attachmentId}`
  - **Method**: `DELETE`
  - **Success Response**: `204 No Content`

---

### 10. Merge Ticket
- **URL**: `/api/tickets/{ticketId}/merge`
- **Method**: `POST`
- **Payload**:
```json
{
  "duplicateTicketId": "9b86fce8-b5fe-4e50-bd6b-80df24be9799",
  "mergedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```
- **Behavior**: Merges the duplicate ticket into this master ticket. Closes the duplicate ticket with status history note `Merged into TK-{MasterNumber}`.
- **Success Response** (`200 OK`): Returns the merge relation ID.

---

### 11. Close Ticket
- **URL**: `/api/tickets/{ticketId}/close`
- **Method**: `POST`
- **Payload**:
```json
{
  "closedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "notes": "Verified resolution with customer."
}
```
- **Domain Constraints**: Only tickets in the `Resolved` status can be closed.
- **Success Response** (`200 OK`):
```json
{
  "message": "Ticket closed successfully.",
  "ticketId": "8f86fce8-b5fe-4e50-bd6b-80df24be9719"
}
```

---

### 12. Reopen Ticket
- **URL**: `/api/tickets/{ticketId}/reopen`
- **Method**: `POST`
- **Payload**:
```json
{
  "reopenedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reason": "Customer reported that the bug is still happening."
}
```
- **Domain Constraints**: Only tickets in `Resolved` or `Closed` status can be reopened.
- **Success Response** (`200 OK`):
```json
{
  "message": "Ticket reopened successfully.",
  "ticketId": "8f86fce8-b5fe-4e50-bd6b-80df24be9719"
}
```

---

### 13. Resolve Ticket
- **URL**: `/api/tickets/{ticketId}/resolve`
- **Method**: `POST`
- **Payload**:
```json
{
  "resolvedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "resolutionSummary": "Updated configuration settings and restarted service."
}
```
- **Domain Constraints**:
  - Customer call must be recorded before resolving (`CustomerCallTaken` must be `true`).
  - Root Cause Analysis (`Rca` property) is required (must be set using database updates or provide RCA methods).
  - Cannot resolve a closed ticket.
- **Success Response** (`200 OK`):
```json
{
  "message": "Ticket resolved successfully.",
  "ticketId": "8f86fce8-b5fe-4e50-bd6b-80df24be9719"
}
```

---

### 14. Get Ticket History
- **URL**: `/api/tickets/{ticketId}/history`
- **Method**: `GET`
- **Success Response** (`200 OK`):
```json
{
  "ticketId": "8f86fce8-b5fe-4e50-bd6b-80df24be9719",
  "history": [
    {
      "fromStatus": null,
      "toStatus": "New",
      "changedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "changedAt": "2026-06-06T13:15:14Z",
      "reason": "Ticket Created"
    },
    {
      "fromStatus": "New",
      "toStatus": "Open",
      "changedBy": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "changedAt": "2026-06-06T13:20:00Z",
      "reason": "System automatic update"
    }
  ]
}
```

---

### 15. Split Ticket
- **URL**: `/api/tickets/{ticketId}/split`
- **Method**: `POST`
- **Payload**:
```json
{
  "newSubject": "Separate database issue discovered",
  "newDescription": "While solving the system crash, we noticed table lockouts...",
  "createdByUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "commentIdsToMove": [
    "c8d6fce8-b5fe-4e50-bd6b-80df24be9721"
  ],
  "attachmentIdsToMove": []
}
```
- **Behavior**: Creates a new ticket using the provided subject and description. Moves the specified comments and/or attachments from the parent ticket to the new ticket.
- **Success Response** (`200 OK`):
```json
{
  "message": "Ticket split successfully.",
  "newTicketId": "2c96fce8-b5fe-4e50-bd6b-80df24be9888"
}
```

---

## ⚠️ Common Exception Handling & Error Formats

The backend uses a global exception handler (`GlobalExceptionHandler.cs`) that captures domain and validation errors and maps them to clean HTTP standard responses:

### 1. Validation Failures (`400 Bad Request`)
Returned when FluentValidation catches request payload violations.
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Subject": [
      "'Subject' must not be empty."
    ],
    "Description": [
      "'Description' must not be empty."
    ]
  }
}
```

### 2. Domain Rule Violations (`400 Bad Request` or `422 Unprocessable Entity`)
Returned when business invariants inside Domain entities (like `Ticket.cs`) are breached.
```json
{
  "title": "Domain Exception",
  "status": 400,
  "detail": "Customer call must be recorded before resolution."
}
```

### 3. Resource Not Found (`404 Not Found`)
Returned when query details resolve to a null record.
```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Ticket '8f86fce8-b5fe-4e50-bd6b-80df24be9719' not found."
}
```
