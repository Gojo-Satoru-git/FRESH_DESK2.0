# Company Module API Documentation

This document describes the endpoints provided by the **Company** module in Adrenalin. It includes endpoints for managing companies, their domains, and individual contacts within those companies.

## Frontend Integration Guidelines

When integrating with these endpoints from the frontend (e.g., using Axios, Fetch, or RTK Query):

1. **Authorization**: All endpoints require a valid JWT token. Include it in the header: `Authorization: Bearer <your_token>`.
2. **Actor IDs**: You do not need to send `CreatedBy` or `ModifiedBy` fields. The backend automatically extracts the current user's ID from the JWT token's `sub` or `NameIdentifier` claim.
3. **Pagination & Filtering**: For `GET /api/companies`, `GET /api/companies/search`, and `GET /api/companies/{id}/contacts`, pass query string parameters (e.g., `?page=1&pageSize=20&searchTerm=Acme`).
4. **Soft Deletes**: Deleting a company, domain, or contact performs a soft delete. The data is hidden but retained in the database for compliance.
5. **Errors**: Failed requests return standard Adrenalin `ProblemDetails` or JSON `{ "error": "Reason" }` payloads with 400 (Validation/Domain error) or 404 (Not Found).

---

## 🏢 Companies Controller (`/api/companies`)

### Core Company Management

#### `GET /api/companies`

- **Description**: Lists companies with pagination, sorting, and filtering.
- **Query Params**: `GeoRegion`, `SupportTier`, `IsActive`, `SortBy`, `SortDescending`, `Page`, `PageSize`.
- **Response**: `PagedResult<CompanyListItemDto>`

#### `GET /api/companies/search`

- **Description**: Searches for companies across name, industry, and CSP ID.
- **Query Params**: `Term`, `GeoRegion`, `SupportTier`, `IsActive`, `Page`, `PageSize`.
- **Response**: `PagedResult<CompanyListItemDto>`

#### `GET /api/companies/{id}`

- **Description**: Retrieves full details for a specific company by its GUID.
- **Response**: `CompanyDetailDto`

#### `GET /api/companies/{id}/summary`

- **Description**: Retrieves a lightweight summary for a specific company.
- **Response**: `CompanySummaryDto`

#### `GET /api/companies/{id}/health`

- **Description**: Retrieves health information (such as Health Score).
- **Response**: `CompanyHealthDto`

#### `GET /api/companies/{id}/ownership`

- **Description**: Retrieves ownership details (Customer Account Manager, Delivery Manager).
- **Response**: `CompanyOwnershipDto`

#### `POST /api/companies`

- **Description**: Creates a new company in the system.
- **Body**:
  ```json
  {
    "name": "string",
    "geoRegion": "string",
    "supportTier": "string"
  }
  ```
- **Response**: `{ "message": "Company created successfully.", "companyId": "guid" }`

#### `PUT /api/companies/{id}`

- **Description**: Updates core details for an existing company.
- **Body**:
  ```json
  {
    "name": "string",
    "geoRegion": "string",
    "supportTier": "string",
    "industry": "string (optional)",
    "notes": "string (optional)"
  }
  ```

#### `DELETE /api/companies/{id}`

- **Description**: Soft-deletes a company.

#### `POST /api/companies/{id}/restore`

- **Description**: Restores a previously soft-deleted company.

#### `POST /api/companies/{id}/activate`

- **Description**: Activates a company. (E.g. resume services/billing).

#### `POST /api/companies/{id}/deactivate`

- **Description**: Deactivates a company. (E.g. put services/billing on hold).

---

### Company Assignment & Metrics

#### `POST /api/companies/{id}/cam`

- **Description**: Assigns a Customer Account Manager (CAM) to a company.
- **Body**:
  ```json
  {
    "camUserId": "guid"
  }
  ```
  _(Note: The GUID must exist in the users table, as referential integrity is strictly enforced)._

#### `POST /api/companies/{id}/delivery-manager`

- **Description**: Assigns a Delivery Manager to a company.
- **Body**:
  ```json
  {
    "deliveryManagerId": "guid"
  }
  ```

#### `PUT /api/companies/{id}/health-score`

- **Description**: Updates the quantitative health score of the company.
- **Body**:
  ```json
  {
    "score": 85
  }
  ```

#### `PUT /api/companies/{id}/tier`

- **Description**: Updates the support tier mapping of the company.
- **Body**:
  ```json
  {
    "supportTier": "M1"
  }
  ```

#### `PUT /api/companies/{id}/contact-limit`

- **Description**: Sets the maximum number of contacts that can be authorized for this company.
- **Body**:
  ```json
  {
    "maxContacts": 50
  }
  ```

---

### Company Domains

_Domains represent email extensions (e.g. `acme.com`) associated with the company, used for auto-routing user signups to the correct company._

#### `GET /api/companies/{id}/domains`

- **Description**: Gets all domains configured for the company.
- **Response**: `IReadOnlyList<CompanyDomainDto>`

#### `POST /api/companies/{id}/domains`

- **Description**: Adds a new domain to the company.
- **Body**:
  ```json
  {
    "domain": "acme.com",
    "isPrimary": true
  }
  ```

#### `DELETE /api/companies/{id}/domains/{domainId}`

- **Description**: Removes a domain from a company.

#### `POST /api/companies/{id}/domains/{domainId}/primary`

- **Description**: Sets a specific domain as the primary domain for the company.

#### `POST /api/companies/{id}/domains/{domainId}/verify`

- **Description**: Marks a domain as manually verified.

---

### Company Contacts

_Contacts represent individuals who belong to the company._

#### `GET /api/companies/{id}/contacts`

- **Description**: Retrieves a paginated list of contacts belonging to the company.
- **Query Params**: `searchTerm`, `isAuthorized`, `page`, `pageSize`
- **Response**: `PagedResult<ContactDto>`

#### `POST /api/companies/{id}/contacts`

- **Description**: Creates a new contact under this company.
- **Body**:
  ```json
  {
    "name": "John Doe",
    "email": "johndoe@acme.com",
    "phone": "+1234567890 (optional)",
    "isAuthorized": true
  }
  ```

---

## 🧑‍💼 Contacts Controller (`/api/contacts`)

_This controller handles operations directly targeting an existing contact by their unique Contact ID, independently of the company route._

#### `PUT /api/contacts/{id}`

- **Description**: Updates an existing contact's profile details.
- **Body**:
  ```json
  {
    "name": "John Smith",
    "email": "jsmith@acme.com",
    "phone": "+0987654321 (optional)"
  }
  ```

#### `DELETE /api/contacts/{id}`

- **Description**: Soft-deletes a contact.

#### `POST /api/contacts/{id}/authorize`

- **Description**: Authorizes a contact for portal access. Ensures they are permitted to log in and create tickets on behalf of the company.

#### `POST /api/contacts/{id}/deactivate`

- **Description**: Revokes authorization for a contact. They remain attached to the company for historical ticket data but can no longer log into the portal.
