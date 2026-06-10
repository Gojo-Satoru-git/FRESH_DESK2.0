// ─── List / Paging ────────────────────────────────────────────────────────────
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ─── Ticket List Item (from GET /api/tickets, /my, /assigned) ─────────────────
export interface TicketListItem {
  id: string;
  ticketNumber: string;
  title: string;
  status: string;
  priority: string;
  descriptionPreview: string;
  assignedAgentId?: string;
  companyId: string;
  createdAt: string;
  updatedAt?: string;
}

// ─── Ticket Dashboard (from GET /api/tickets/dashboard) ───────────────────────
export interface TicketDashboard {
  totalActive: number;
  inProgress: number;
  pendingReply: number;
  resolvedClosed: number;
}

// ─── Comment Attachment (nested in CommentDto) ────────────────────────────────
export interface CommentAttachment {
  id: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
}

// ─── Comment (from CommentDto) ────────────────────────────────────────────────
export interface TicketComment {
  id: string;
  authorId?: string;
  contactId?: string;
  body: string;
  visibility: string;
  createdAt: string;
  attachments: CommentAttachment[];
  authorName?: string;
  contactName?: string;
}

// ─── Activity (from TicketActivityDto) ────────────────────────────────────────
export interface TicketActivity {
  id: string;
  activityType: string;
  oldValue?: string;
  newValue?: string;
  performedBy?: string;
  performedAt: string;
  performedByName?: string;
}

// ─── Status History (from StatusHistoryDto) ───────────────────────────────────
export interface StatusHistory {
  id: string;
  oldStatus: string;
  newStatus: string;
  changedBy?: string;
  changedAt: string;
  reason?: string;
}

// ─── Assignment Log (from AssignmentLogDto) ───────────────────────────────────
export interface AssignmentLog {
  id: string;
  agentId?: string;
  assignedBy?: string;
  assignedAt: string;
  notes?: string;
}

// ─── Ticket Detail (from GetTicketByIdResponse) ───────────────────────────────
export interface TicketDetails {
  id: string;
  ticketNumber?: string;
  title: string;
  description: string;
  status: string;
  priority: string;
  category: string;
  assignedAgentId?: string;
  reporterId?: string;
  companyId: string;
  department?: string;
  region?: string;
  moduleName?: string;
  createdAt: string;
  updatedAt?: string;
  resolvedAt?: string;
  closedAt?: string;
  tags: string[];
  comments: TicketComment[];
  statusHistory: StatusHistory[];
  assignmentLogs: AssignmentLog[];
  activities: TicketActivity[];
  reporterName?: string;
  assignedAgentName?: string;
}

// ─── Create / Update Requests ─────────────────────────────────────────────────

/** Matches CreateTicketCommand — ActorId/IsCustomer injected server-side from JWT */
export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: string;   // 'Critical' | 'High' | 'Medium' | 'Low'
  type: string;       // 'Bug' | 'FeatureRequest' | 'Support' | 'ChangeRequest'
  tags?: string[];
  assigneeId?: string | null;
  moduleName?: string | null;
}

/** Matches UpdateTicketRequest (inline in TicketsController) */
export interface UpdateTicketRequest {
  title: string;
  description: string;
  priority: string;
  type: string;
  tags: string[];
}

/** Matches AddCommentRequest — AuthorId/ContactId resolved from JWT server-side */
export interface AddCommentRequest {
  body: string;
  visibility: 'Public' | 'Internal';
}
