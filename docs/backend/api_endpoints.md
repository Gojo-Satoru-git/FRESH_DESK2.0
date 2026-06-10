# Adrenalin API Endpoints

## `POST` /api/admin/internal-users

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CreateInternalUserRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `CreateInternalUserRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `CreateInternalUserRequestDTO`

### Responses
- **200**: OK

---

## `POST` /api/auth/register

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `RegisterUserRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `RegisterUserRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `RegisterUserRequestDTO`

### Responses
- **200**: OK

---

## `POST` /api/auth/login

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `LoginRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `LoginRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `LoginRequestDTO`

### Responses
- **200**: OK

---

## `POST` /api/auth/refresh

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `RefreshTokenRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `RefreshTokenRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `RefreshTokenRequestDTO`

### Responses
- **200**: OK

---

## `POST` /api/auth/logout

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `LogoutRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `LogoutRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `LogoutRequestDTO`

### Responses
- **200**: OK

---

## `GET` /api/auth/verify-email

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `token` | query | No | `string` |

### Responses
- **200**: OK

---

## `GET` /api/auth/forgot-password

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `ForgotPasswordRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `ForgotPasswordRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `ForgotPasswordRequestDTO`

### Responses
- **200**: OK

---

## `POST` /api/auth/resend-verification

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `ResendVerificationRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `ResendVerificationRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `ResendVerificationRequestDTO`

### Responses
- **200**: OK

---

## `POST` /api/auth/verify-email-otp

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `VerifyEmailOtpRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `VerifyEmailOtpRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `VerifyEmailOtpRequestDTO`

### Responses
- **200**: OK

---

## `POST` /api/auth/reset-password

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `ResetPasswordRequestDTO`
- **Content-Type**: `text/json`
  - **Schema**: `ResetPasswordRequestDTO`
- **Content-Type**: `application/*+json`
  - **Schema**: `ResetPasswordRequestDTO`

### Responses
- **200**: OK

---

## `GET` /api/rbac/groups

### Responses
- **200**: OK
  - `application/json` Schema: Array of `GroupDto`

---

## `POST` /api/rbac/groups

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CreateGroupRequest`
- **Content-Type**: `text/json`
  - **Schema**: `CreateGroupRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `CreateGroupRequest`

### Responses
- **201**: Created
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/rbac/groups/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `GroupDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `PUT` /api/rbac/groups/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `UpdateGroupRequest`
- **Content-Type**: `text/json`
  - **Schema**: `UpdateGroupRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `UpdateGroupRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `DELETE` /api/rbac/groups/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/rbac/groups/{id}/members

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `GroupWithMembersDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/rbac/groups/{id}/members/add

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `AddMemberRequest`
- **Content-Type**: `text/json`
  - **Schema**: `AddMemberRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `AddMemberRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/rbac/groups/{id}/members/remove

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `UserIdRequest`
- **Content-Type**: `text/json`
  - **Schema**: `UserIdRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `UserIdRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `PATCH` /api/rbac/groups/{id}/members/{userId}/lead

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |
| `userId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `SetLeadRequest`
- **Content-Type**: `text/json`
  - **Schema**: `SetLeadRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `SetLeadRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/kb/articles

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `titleQuery` | query | No | `string` |
| `articleType` | query | No | `ArticleType` |
| `status` | query | No | `ArticleStatus` |
| `folderId` | query | No | `string` |
| `pageNumber` | query | No | `integer` |
| `pageSize` | query | No | `integer` |

### Responses
- **200**: OK
  - `application/json` Schema: `KbArticleSearchResultDto`

---

## `POST` /api/kb/articles

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CreateArticleRequest`
- **Content-Type**: `text/json`
  - **Schema**: `CreateArticleRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `CreateArticleRequest`

### Responses
- **201**: Created
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/kb/articles/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `KbArticleDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `PUT` /api/kb/articles/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `UpdateArticleRequest`
- **Content-Type**: `text/json`
  - **Schema**: `UpdateArticleRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `UpdateArticleRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `DELETE` /api/kb/articles/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/kb/articles/{id}/attachments

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `KbArticleWithAttachmentsDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/kb/articles/{id}/attachments

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `multipart/form-data`

### Responses
- **201**: Created
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/kb/articles/auto-resolve-candidates

### Responses
- **200**: OK
  - `application/json` Schema: Array of `KbArticleDto`

---

## `PUT` /api/kb/articles/{id}/move

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `MoveArticleRequest`
- **Content-Type**: `text/json`
  - **Schema**: `MoveArticleRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `MoveArticleRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/kb/articles/{id}/publish

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/kb/articles/{id}/archive

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/kb/articles/{id}/restore-to-draft

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/kb/articles/{id}/auto-resolve/enable

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `EnableAutoResolveRequest`
- **Content-Type**: `text/json`
  - **Schema**: `EnableAutoResolveRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `EnableAutoResolveRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/kb/articles/{id}/auto-resolve/disable

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content

---

## `POST` /api/kb/articles/{id}/guardrail-exclude

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `DELETE` /api/kb/articles/{id}/attachments/{attachmentId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |
| `attachmentId` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/kb/folders/tree

### Responses
- **200**: OK
  - `application/json` Schema: Array of `KbFolderTreeNodeDto`

---

## `GET` /api/kb/folders/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `KbFolderDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `DELETE` /api/kb/folders/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/kb/folders/{id}/children

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: Array of `KbFolderDto`

---

## `POST` /api/kb/folders

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CreateFolderRequest`
- **Content-Type**: `text/json`
  - **Schema**: `CreateFolderRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `CreateFolderRequest`

### Responses
- **201**: Created
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `PUT` /api/kb/folders/{id}/rename

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `RenameFolderRequest`
- **Content-Type**: `text/json`
  - **Schema**: `RenameFolderRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `RenameFolderRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `PUT` /api/kb/folders/{id}/reorder

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `ReorderFolderRequest`
- **Content-Type**: `text/json`
  - **Schema**: `ReorderFolderRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `ReorderFolderRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/rbac/permissions

### Responses
- **200**: OK
  - `application/json` Schema: Array of `PermissionDto`

---

## `POST` /api/rbac/permissions

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CreatePermissionRequest`
- **Content-Type**: `text/json`
  - **Schema**: `CreatePermissionRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `CreatePermissionRequest`

### Responses
- **201**: Created
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/rbac/permissions/by-role/{roleId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `roleId` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: Array of `PermissionDto`

---

## `DELETE` /api/rbac/permissions/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/kb/banners/active

### Responses
- **200**: OK
  - `application/json` Schema: Array of `ActivePortalBannerDto`

---

## `GET` /api/kb/banners

### Responses
- **200**: OK
  - `application/json` Schema: Array of `PortalBannerDto`

---

## `POST` /api/kb/banners

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CreateBannerRequest`
- **Content-Type**: `text/json`
  - **Schema**: `CreateBannerRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `CreateBannerRequest`

### Responses
- **201**: Created
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/kb/banners/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `PortalBannerDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `PUT` /api/kb/banners/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `UpdateBannerRequest`
- **Content-Type**: `text/json`
  - **Schema**: `UpdateBannerRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `UpdateBannerRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/kb/banners/{id}/activate

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/kb/banners/{id}/deactivate

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/rbac/roles

### Responses
- **200**: OK
  - `application/json` Schema: Array of `RoleDto`

---

## `POST` /api/rbac/roles

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CreateRoleRequest`
- **Content-Type**: `text/json`
  - **Schema**: `CreateRoleRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `CreateRoleRequest`

### Responses
- **201**: Created
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/rbac/roles/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `RoleDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `PUT` /api/rbac/roles/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `UpdateRoleRequest`
- **Content-Type**: `text/json`
  - **Schema**: `UpdateRoleRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `UpdateRoleRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `DELETE` /api/rbac/roles/{id}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/rbac/roles/{id}/permissions

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `RoleWithPermissionsDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `PUT` /api/rbac/roles/{id}/permissions

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `SetPermissionsRequest`
- **Content-Type**: `text/json`
  - **Schema**: `SetPermissionsRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `SetPermissionsRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/rbac/roles/{id}/permissions/grant

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `PermissionIdRequest`
- **Content-Type**: `text/json`
  - **Schema**: `PermissionIdRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `PermissionIdRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/rbac/roles/{id}/permissions/revoke

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `PermissionIdRequest`
- **Content-Type**: `text/json`
  - **Schema**: `PermissionIdRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `PermissionIdRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/tickets

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CreateTicketCommand`
- **Content-Type**: `text/json`
  - **Schema**: `CreateTicketCommand`
- **Content-Type**: `application/*+json`
  - **Schema**: `CreateTicketCommand`

### Responses
- **200**: OK

---

## `GET` /api/tickets

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `Term` | query | No | `string` |
| `Status` | query | No | `string` |
| `Priority` | query | No | `string` |
| `Type` | query | No | `string` |
| `AssigneeId` | query | No | `string` |
| `ReporterId` | query | No | `string` |
| `CreatedFrom` | query | No | `string` |
| `CreatedTo` | query | No | `string` |
| `Page` | query | No | `integer` |
| `PageSize` | query | No | `integer` |

### Responses
- **200**: OK

---

## `PUT` /api/tickets/{ticketId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `UpdateTicketRequest`
- **Content-Type**: `text/json`
  - **Schema**: `UpdateTicketRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `UpdateTicketRequest`

### Responses
- **200**: OK

---

## `DELETE` /api/tickets/{ticketId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Responses
- **200**: OK

---

## `GET` /api/tickets/{ticketId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Responses
- **200**: OK

---

## `POST` /api/tickets/{ticketId}/assign

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `AssignTicketRequest`
- **Content-Type**: `text/json`
  - **Schema**: `AssignTicketRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `AssignTicketRequest`

### Responses
- **200**: OK

---

## `POST` /api/tickets/{ticketId}/status

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `ChangeTicketStatusRequest`
- **Content-Type**: `text/json`
  - **Schema**: `ChangeTicketStatusRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `ChangeTicketStatusRequest`

### Responses
- **200**: OK

---

## `POST` /api/tickets/{ticketId}/comments

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `AddCommentRequest`
- **Content-Type**: `text/json`
  - **Schema**: `AddCommentRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `AddCommentRequest`

### Responses
- **200**: OK
  - `text/plain` Schema: `string`
  - `application/json` Schema: `string`
  - `text/json` Schema: `string`

---

## `GET` /api/tickets/{ticketId}/comments

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |
| `includeInternal` | query | No | `boolean` |

### Responses
- **200**: OK

---

## `GET` /api/tickets/my

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `page` | query | No | `integer` |
| `pageSize` | query | No | `integer` |

### Responses
- **200**: OK

---

## `GET` /api/tickets/assigned

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `page` | query | No | `integer` |
| `pageSize` | query | No | `integer` |

### Responses
- **200**: OK

---

## `GET` /api/tickets/dashboard

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `companyId` | query | No | `string` |

### Responses
- **200**: OK

---

## `GET` /api/tickets/{ticketId}/activities

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Responses
- **200**: OK

---

## `POST` /api/tickets/{ticketId}/watchers/{userId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |
| `userId` | path | Yes | `string` |
| `addedBy` | query | No | `string` |

### Responses
- **200**: OK

---

## `DELETE` /api/tickets/{ticketId}/watchers/{userId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |
| `userId` | path | Yes | `string` |

### Responses
- **200**: OK

---

## `POST` /api/tickets/{ticketId}/relations

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `LinkTicketRequest`
- **Content-Type**: `text/json`
  - **Schema**: `LinkTicketRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `LinkTicketRequest`

### Responses
- **200**: OK
  - `text/plain` Schema: `string`
  - `application/json` Schema: `string`
  - `text/json` Schema: `string`

---

## `POST` /api/tickets/{ticketId}/attachments

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `multipart/form-data`

### Responses
- **200**: OK
  - `text/plain` Schema: `string`
  - `application/json` Schema: `string`
  - `text/json` Schema: `string`

---

## `GET` /api/tickets/{ticketId}/attachments/{attachmentId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |
| `attachmentId` | path | Yes | `string` |

### Responses
- **200**: OK

---

## `DELETE` /api/tickets/{ticketId}/attachments/{attachmentId}

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |
| `attachmentId` | path | Yes | `string` |

### Responses
- **200**: OK

---

## `POST` /api/tickets/{ticketId}/merge

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `MergeTicketRequest`
- **Content-Type**: `text/json`
  - **Schema**: `MergeTicketRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `MergeTicketRequest`

### Responses
- **200**: OK
  - `text/plain` Schema: `string`
  - `application/json` Schema: `string`
  - `text/json` Schema: `string`

---

## `POST` /api/tickets/{ticketId}/close

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `CloseTicketRequest`
- **Content-Type**: `text/json`
  - **Schema**: `CloseTicketRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `CloseTicketRequest`

### Responses
- **200**: OK

---

## `POST` /api/tickets/{ticketId}/reopen

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `ReopenTicketRequest`
- **Content-Type**: `text/json`
  - **Schema**: `ReopenTicketRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `ReopenTicketRequest`

### Responses
- **200**: OK

---

## `POST` /api/tickets/{ticketId}/resolve

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `ResolveTicketRequest`
- **Content-Type**: `text/json`
  - **Schema**: `ResolveTicketRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `ResolveTicketRequest`

### Responses
- **200**: OK

---

## `GET` /api/tickets/{ticketId}/history

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Responses
- **200**: OK
  - `text/plain` Schema: `TicketHistoryDto`
  - `application/json` Schema: `TicketHistoryDto`
  - `text/json` Schema: `TicketHistoryDto`

---

## `POST` /api/tickets/{ticketId}/split

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `ticketId` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `SplitTicketRequest`
- **Content-Type**: `text/json`
  - **Schema**: `SplitTicketRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `SplitTicketRequest`

### Responses
- **200**: OK
  - `text/plain` Schema: `string`
  - `application/json` Schema: `string`
  - `text/json` Schema: `string`

---

## `GET` /api/rbac/users

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `email` | query | No | `string` |
| `isActive` | query | No | `boolean` |
| `pageNumber` | query | No | `integer` |
| `pageSize` | query | No | `integer` |

### Responses
- **200**: OK
  - `application/json` Schema: `UserSummaryDtoPagedResultDto`

---

## `GET` /api/rbac/users/{id}/roles

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `UserWithRolesDto`
- **404**: Not Found
  - `application/json` Schema: `ProblemDetails`

---

## `PUT` /api/rbac/users/{id}/roles

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `SetRolesRequest`
- **Content-Type**: `text/json`
  - **Schema**: `SetRolesRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `SetRolesRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `GET` /api/rbac/users/{id}/permissions

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: `array`

---

## `GET` /api/rbac/users/{id}/groups

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Responses
- **200**: OK
  - `application/json` Schema: Array of `GroupDto`

---

## `POST` /api/rbac/users/{id}/roles/assign

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `RoleIdRequest`
- **Content-Type**: `text/json`
  - **Schema**: `RoleIdRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `RoleIdRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

## `POST` /api/rbac/users/{id}/roles/remove

### Parameters
| Name | In | Required | Type |
| --- | --- | --- | --- |
| `id` | path | Yes | `string` |

### Request Body
- **Content-Type**: `application/json`
  - **Schema**: `RoleIdRequest`
- **Content-Type**: `text/json`
  - **Schema**: `RoleIdRequest`
- **Content-Type**: `application/*+json`
  - **Schema**: `RoleIdRequest`

### Responses
- **204**: No Content
- **400**: Bad Request
  - `application/json` Schema: `ProblemDetails`

---

# Schemas

## ActivePortalBannerDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `title` | `string` | Yes |
| `message` | `string` | Yes |

## AddCommentRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `authorId` | `string` | Yes |
| `contactId` | `string` | Yes |
| `body` | `string` | Yes |
| `isPrivate` | `boolean` | No |

## AddMemberRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `userId` | `string` | No |
| `isLead` | `boolean` | No |

## ArticleStatus

## ArticleType

## AssignTicketRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `agentId` | `string` | No |
| `assignedBy` | `string` | No |
| `notes` | `string` | Yes |

## AssignmentLogDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `fromAgentId` | `string` | Yes |
| `toAgentId` | `string` | No |
| `changedBy` | `string` | Yes |
| `notes` | `string` | Yes |
| `createdAt` | `string` | No |

## ChangeTicketStatusRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `newStatus` | `TicketStatus` | No |
| `changedBy` | `string` | No |
| `reason` | `string` | Yes |

## CloseTicketRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `closedBy` | `string` | No |
| `notes` | `string` | Yes |

## CreateArticleRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `title` | `string` | Yes |
| `content` | `string` | Yes |
| `articleType` | `ArticleType` | No |
| `folderId` | `string` | Yes |

## CreateBannerRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `title` | `string` | Yes |
| `message` | `string` | Yes |
| `activeFrom` | `string` | Yes |
| `activeTo` | `string` | Yes |

## CreateFolderRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `name` | `string` | Yes |
| `parentId` | `string` | Yes |
| `displayOrder` | `integer` | No |

## CreateGroupRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `name` | `string` | Yes |
| `regionCode` | `string` | Yes |
| `tierCode` | `string` | Yes |
| `unattendedAlertMinutes` | `integer` | No |

## CreateInternalUserRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `email` | `string` | Yes |
| `firstName` | `string` | Yes |
| `lastName` | `string` | Yes |
| `phone` | `string` | Yes |
| `roleId` | `string` | No |

## CreatePermissionRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `resource` | `string` | Yes |
| `action` | `string` | Yes |
| `description` | `string` | Yes |

## CreateRoleRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `name` | `string` | Yes |
| `description` | `string` | Yes |

## CreateTicketCommand

| Property | Type | Nullable |
| --- | --- | --- |
| `title` | `string` | Yes |
| `description` | `string` | Yes |
| `priority` | `string` | Yes |
| `type` | `string` | Yes |
| `actorId` | `string` | Yes |
| `isCustomer` | `boolean` | No |
| `senderEmail` | `string` | Yes |
| `assigneeId` | `string` | Yes |
| `moduleName` | `string` | Yes |

## EnableAutoResolveRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `keywords` | `Array of string` | Yes |
| `resolutionText` | `string` | Yes |
| `confidenceThreshold` | `number` | No |

## ForgotPasswordRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `email` | `string` | Yes |

## GroupDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `name` | `string` | Yes |
| `regionCode` | `string` | Yes |
| `tierCode` | `string` | Yes |
| `unattendedAlertMinutes` | `integer` | No |
| `isActive` | `boolean` | No |
| `createdAt` | `string` | No |
| `updatedAt` | `string` | Yes |

## GroupMemberDto

| Property | Type | Nullable |
| --- | --- | --- |
| `userId` | `string` | No |
| `email` | `string` | Yes |
| `firstName` | `string` | Yes |
| `lastName` | `string` | Yes |
| `isLead` | `boolean` | No |

## GroupWithMembersDto

| Property | Type | Nullable |
| --- | --- | --- |
| `group` | `GroupDto` | No |
| `members` | `Array of GroupMemberDto` | Yes |

## KbArticleDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `title` | `string` | Yes |
| `content` | `string` | Yes |
| `articleType` | `ArticleType` | No |
| `status` | `ArticleStatus` | No |
| `isPublished` | `boolean` | No |
| `authorId` | `string` | Yes |
| `folderId` | `string` | Yes |
| `autoResolve` | `boolean` | No |
| `confidenceThreshold` | `number` | No |
| `keywords` | `Array of string` | Yes |
| `resolutionText` | `string` | Yes |
| `guardrailExcluded` | `boolean` | No |
| `timesMatched` | `integer` | No |
| `timesReopened` | `integer` | No |
| `createdAt` | `string` | No |
| `updatedAt` | `string` | Yes |

## KbArticleSearchResultDto

| Property | Type | Nullable |
| --- | --- | --- |
| `items` | `Array of KbArticleSummaryDto` | Yes |
| `totalCount` | `integer` | No |
| `pageNumber` | `integer` | No |
| `pageSize` | `integer` | No |

## KbArticleSummaryDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `title` | `string` | Yes |
| `articleType` | `ArticleType` | No |
| `status` | `ArticleStatus` | No |
| `isPublished` | `boolean` | No |
| `autoResolve` | `boolean` | No |
| `guardrailExcluded` | `boolean` | No |
| `folderId` | `string` | Yes |
| `updatedAt` | `string` | Yes |

## KbArticleWithAttachmentsDto

| Property | Type | Nullable |
| --- | --- | --- |
| `article` | `KbArticleDto` | No |
| `attachments` | `Array of KbAttachmentDto` | Yes |

## KbAttachmentDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `articleId` | `string` | No |
| `fileUrl` | `string` | Yes |
| `fileName` | `string` | Yes |
| `fileSizeBytes` | `integer` | Yes |
| `mimeType` | `string` | Yes |
| `createdAt` | `string` | No |

## KbFolderDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `name` | `string` | Yes |
| `parentId` | `string` | Yes |
| `displayOrder` | `integer` | No |
| `createdAt` | `string` | No |
| `updatedAt` | `string` | Yes |

## KbFolderTreeNodeDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `name` | `string` | Yes |
| `parentId` | `string` | Yes |
| `displayOrder` | `integer` | No |
| `children` | `Array of KbFolderTreeNodeDto` | Yes |

## LinkTicketRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `childTicketId` | `string` | No |
| `relationType` | `TicketRelationType` | No |

## LoginRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `email` | `string` | Yes |
| `password` | `string` | Yes |

## LogoutRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `refreshToken` | `string` | Yes |

## MergeTicketRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `duplicateTicketId` | `string` | No |
| `mergedBy` | `string` | No |

## MoveArticleRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `targetFolderId` | `string` | Yes |

## PermissionDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `resource` | `string` | Yes |
| `action` | `string` | Yes |
| `description` | `string` | Yes |

## PermissionIdRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `permissionId` | `string` | No |

## PortalBannerDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `title` | `string` | Yes |
| `message` | `string` | Yes |
| `activeFrom` | `string` | Yes |
| `activeTo` | `string` | Yes |
| `isActive` | `boolean` | No |
| `createdAt` | `string` | No |
| `updatedAt` | `string` | Yes |

## ProblemDetails

| Property | Type | Nullable |
| --- | --- | --- |
| `type` | `string` | Yes |
| `title` | `string` | Yes |
| `status` | `integer` | Yes |
| `detail` | `string` | Yes |
| `instance` | `string` | Yes |

## RefreshTokenRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `refreshToken` | `string` | Yes |

## RegisterUserRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `email` | `string` | Yes |
| `password` | `string` | Yes |
| `firstName` | `string` | Yes |
| `lastName` | `string` | Yes |
| `username` | `string` | Yes |
| `phone` | `string` | Yes |

## RenameFolderRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `newName` | `string` | Yes |

## ReopenTicketRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `reopenedBy` | `string` | No |
| `reason` | `string` | Yes |

## ReorderFolderRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `newDisplayOrder` | `integer` | No |

## ResendVerificationRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `email` | `string` | Yes |

## ResetPasswordRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `token` | `string` | Yes |
| `newPassword` | `string` | Yes |

## ResolveTicketRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `resolvedBy` | `string` | No |
| `resolutionSummary` | `string` | Yes |

## RoleDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `name` | `string` | Yes |
| `description` | `string` | Yes |
| `isSystemRole` | `boolean` | No |
| `createdAt` | `string` | No |
| `updatedAt` | `string` | Yes |

## RoleIdRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `roleId` | `string` | No |

## RoleSummaryDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `name` | `string` | Yes |
| `isSystemRole` | `boolean` | No |

## RoleWithPermissionsDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `name` | `string` | Yes |
| `description` | `string` | Yes |
| `isSystemRole` | `boolean` | No |
| `permissions` | `Array of PermissionDto` | Yes |
| `createdAt` | `string` | No |
| `updatedAt` | `string` | Yes |

## SetLeadRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `isLead` | `boolean` | No |

## SetPermissionsRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `permissionIds` | `Array of string` | Yes |

## SetRolesRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `roleIds` | `Array of string` | Yes |

## SplitTicketRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `newSubject` | `string` | Yes |
| `newDescription` | `string` | Yes |
| `createdByUserId` | `string` | No |
| `commentIdsToMove` | `Array of string` | Yes |
| `attachmentIdsToMove` | `Array of string` | Yes |

## StatusHistoryDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `fromStatus` | `string` | Yes |
| `toStatus` | `string` | Yes |
| `changedBy` | `string` | Yes |
| `reason` | `string` | Yes |
| `createdAt` | `string` | No |

## TicketHistoryDto

| Property | Type | Nullable |
| --- | --- | --- |
| `statusHistory` | `Array of StatusHistoryDto` | Yes |
| `assignmentLogs` | `Array of AssignmentLogDto` | Yes |

## TicketPriority

## TicketRelationType

## TicketStatus

## TicketType

## UpdateArticleRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `newTitle` | `string` | Yes |
| `newContent` | `string` | Yes |

## UpdateBannerRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `title` | `string` | Yes |
| `message` | `string` | Yes |
| `activeFrom` | `string` | Yes |
| `activeTo` | `string` | Yes |

## UpdateGroupRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `name` | `string` | Yes |
| `regionCode` | `string` | Yes |
| `tierCode` | `string` | Yes |
| `unattendedAlertMinutes` | `integer` | No |

## UpdateRoleRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `name` | `string` | Yes |
| `description` | `string` | Yes |

## UpdateTicketRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `title` | `string` | Yes |
| `description` | `string` | Yes |
| `priority` | `TicketPriority` | No |
| `type` | `TicketType` | No |

## UserIdRequest

| Property | Type | Nullable |
| --- | --- | --- |
| `userId` | `string` | No |

## UserSummaryDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `email` | `string` | Yes |
| `firstName` | `string` | Yes |
| `lastName` | `string` | Yes |
| `isActive` | `boolean` | No |
| `phone` | `string` | Yes |

## UserSummaryDtoPagedResultDto

| Property | Type | Nullable |
| --- | --- | --- |
| `items` | `Array of UserSummaryDto` | Yes |
| `totalCount` | `integer` | No |
| `pageNumber` | `integer` | No |
| `pageSize` | `integer` | No |

## UserWithRolesDto

| Property | Type | Nullable |
| --- | --- | --- |
| `id` | `string` | No |
| `email` | `string` | Yes |
| `firstName` | `string` | Yes |
| `lastName` | `string` | Yes |
| `isActive` | `boolean` | No |
| `roles` | `Array of RoleSummaryDto` | Yes |
| `createdAt` | `string` | No |
| `lastLoginAt` | `string` | Yes |

## VerifyEmailOtpRequestDTO

| Property | Type | Nullable |
| --- | --- | --- |
| `otp` | `string` | Yes |

