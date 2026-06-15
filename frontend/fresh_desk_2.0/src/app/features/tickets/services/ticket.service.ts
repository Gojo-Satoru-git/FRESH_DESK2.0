import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment.development';
import {
  TicketListItem,
  TicketDashboard,
  TicketDetails,
  PagedResult,
  CreateTicketRequest,
  UpdateTicketRequest,
  AddCommentRequest,
  TicketComment,
  TicketActivity,
} from '../models/ticket.model';

export interface SearchTicketsParams {
  ticketNumber?: string;
  status?: string;
  assignedAgentId?: string;
  companyId?: string;
  page?: number;
  pageSize?: number;
}

@Injectable({
  providedIn: 'root',
})
export class TicketService {
  private apiUrl = `${environment.apiUrl}/api/tickets`;

  constructor(private http: HttpClient) {}

  // ── Queries ──────────────────────────────────────────────────────────────

  getAgents(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/rbac/users?pageSize=100`);
  }

  /** GET /api/rbac/users/{id}/roles — returns UserWithRolesDto */
  getUserWithRoles(userId: string): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/rbac/users/${userId}/roles`);
  }

  /** GET /api/rbac/groups/my — returns the groups the calling user belongs to (no admin required) */
  getUserGroups(userId: string): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/rbac/groups/my`);
  }

  /** GET /api/rbac/groups/{id}/my-members — returns members of a group the caller belongs to (no admin required) */
  getGroupMembers(groupId: string): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/rbac/groups/${groupId}/my-members`);
  }

  /** GET /api/tickets/dashboard — actorId injected from JWT server-side */
  getDashboard(companyId?: string): Observable<TicketDashboard> {
    let params = new HttpParams();
    if (companyId) params = params.set('companyId', companyId);
    return this.http.get<TicketDashboard>(`${this.apiUrl}/dashboard`, { params });
  }

  /** GET /api/tickets/my — returns tickets created by or reported by current user */
  getMyTickets(page: number = 1, pageSize: number = 20): Observable<PagedResult<TicketListItem>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<TicketListItem>>(`${this.apiUrl}/my`, { params });
  }

  /** GET /api/tickets/assigned — tickets assigned to the current agent */
  getAssignedTickets(
    page: number = 1,
    pageSize: number = 20,
  ): Observable<PagedResult<TicketListItem>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<TicketListItem>>(`${this.apiUrl}/assigned`, { params });
  }

  /** GET /api/tickets — searchable all-tickets list (requires ticket:read policy) */
  searchTickets(opts: SearchTicketsParams = {}): Observable<PagedResult<TicketListItem>> {
    let params = new HttpParams()
      .set('page', (opts.page ?? 1).toString())
      .set('pageSize', (opts.pageSize ?? 20).toString());
    if (opts.ticketNumber) params = params.set('ticketNumber', opts.ticketNumber);
    if (opts.status) params = params.set('status', opts.status);
    if (opts.assignedAgentId) params = params.set('assignedAgentId', opts.assignedAgentId);
    if (opts.companyId) params = params.set('companyId', opts.companyId);
    return this.http.get<PagedResult<TicketListItem>>(this.apiUrl, { params });
  }

  /** GET /api/tickets/{id} — returns full ticket with embedded comments & activities */
  getTicketById(id: string): Observable<TicketDetails> {
    return this.http.get<TicketDetails>(`${this.apiUrl}/${id}`);
  }

  /** GET /api/tickets/{id}/comments */
  getComments(id: string, includeInternal: boolean = false): Observable<TicketComment[]> {
    const params = new HttpParams().set('includeInternal', includeInternal.toString());
    return this.http.get<TicketComment[]>(`${this.apiUrl}/${id}/comments`, { params });
  }

  /** GET /api/tickets/{id}/activities */
  getActivities(id: string): Observable<TicketActivity[]> {
    return this.http.get<TicketActivity[]>(`${this.apiUrl}/${id}/activities`);
  }

  // ── Commands ─────────────────────────────────────────────────────────────

  /**
   * POST /api/tickets
   * ActorId & IsCustomer are injected server-side from JWT — do NOT send them.
   */
  createTicket(request: CreateTicketRequest): Observable<{ ticketId: string; message: string }> {
    return this.http.post<{ ticketId: string; message: string }>(this.apiUrl, request);
  }

  /** PUT /api/tickets/{id} — actorId injected from JWT */
  updateTicket(
    id: string,
    request: UpdateTicketRequest,
  ): Observable<{ ticketId: string; message: string }> {
    return this.http.put<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}`, request);
  }

  /** DELETE /api/tickets/{id} */
  deleteTicket(id: string): Observable<{ ticketId: string; message: string }> {
    return this.http.delete<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}`);
  }

  /**
   * POST /api/tickets/{id}/assign
   * AssignedBy is injected from JWT server-side — only agentId & optional notes needed.
   */
  assignTicket(
    id: string,
    agentId: string,
    notes?: string,
  ): Observable<{ ticketId: string; agentId: string; message: string }> {
    return this.http.post<{ ticketId: string; agentId: string; message: string }>(
      `${this.apiUrl}/${id}/assign`,
      { agentId, notes },
    );
  }

  /**
   * POST /api/tickets/{id}/status
   * ChangedBy is injected from JWT server-side.
   * newStatus must match TicketStatus enum: New|Open|Assigned|InProgress|Pending|Resolved|Closed|Reopened
   */
  changeStatus(
    id: string,
    newStatus: string,
    reason?: string,
  ): Observable<{ ticketId: string; newStatus: string; message: string }> {
    return this.http.post<{ ticketId: string; newStatus: string; message: string }>(
      `${this.apiUrl}/${id}/status`,
      { newStatus, reason },
    );
  }

 
  /**
   * POST /api/tickets/{id}/resolve
   * ResolvedBy injected from JWT server-side.
   */
  resolveTicket(
    id: string,
    resolutionSummary: string,
  ): Observable<{ ticketId: string; message: string }> {
    return this.http.post<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}/resolve`, {
      resolutionSummary,
    });
  }

  /**
   * POST /api/tickets/{id}/close
   * ClosedBy injected from JWT server-side.
   */
  closeTicket(id: string, notes?: string): Observable<{ ticketId: string; message: string }> {
    return this.http.post<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}/close`, {
      notes,
    });
  }

  /**
   * POST /api/tickets/{id}/reopen
   * ReopenedBy injected from JWT server-side.
   */
  reopenTicket(id: string, reason?: string): Observable<{ ticketId: string; message: string }> {
    return this.http.post<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}/reopen`, {
      reason,
    });
  }

  /**
   * POST /api/tickets/{id}/comments
   * AuthorId/ContactId resolved from JWT server-side — only body & visibility needed.
   * visibility: 'Public' | 'Internal'
   */
  addComment(id: string, request: AddCommentRequest): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/${id}/comments`, request);
  }

  getActiveBanners(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/kb/banners/active`);
  }
}
