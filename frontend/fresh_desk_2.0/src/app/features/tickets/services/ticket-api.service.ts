import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { SearchTicketsParams, TicketService } from './ticket.service';
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

@Injectable()
export class ApiTicketService extends TicketService {
  private apiUrl = `${environment.apiBaseUrl}/api/tickets`;

  constructor(private http: HttpClient) {
    super();
  }

  // ── Queries ──────────────────────────────────────────────────────────────

  override getAgents(): Observable<any> {
    return this.http.get<any>(`${environment.apiBaseUrl}/api/rbac/users?pageSize=100`);
  }

  /** GET /api/rbac/users/{id}/roles — returns UserWithRolesDto */
  override getUserWithRoles(userId: string): Observable<any> {
    return this.http.get<any>(`${environment.apiBaseUrl}/api/rbac/users/${userId}/roles`);
  }

  /** GET /api/rbac/groups/my — returns the groups the calling user belongs to (no admin required) */
  override getUserGroups(userId: string): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiBaseUrl}/api/rbac/groups/my`);
  }

  /** GET /api/rbac/groups/{id}/my-members — returns members of a group the caller belongs to (no admin required) */
  override getGroupMembers(groupId: string): Observable<any> {
    return this.http.get<any>(`${environment.apiBaseUrl}/api/rbac/groups/${groupId}/my-members`);
  }

  /** GET /api/tickets/dashboard — actorId injected from JWT server-side */
  override getDashboard(companyId?: string): Observable<TicketDashboard> {
    let params = new HttpParams();
    if (companyId) params = params.set('companyId', companyId);
    return this.http.get<TicketDashboard>(`${this.apiUrl}/dashboard`, { params });
  }

  /** GET /api/tickets/my — returns tickets created by or reported by current user */
  override getMyTickets(
    page: number = 1,
    pageSize: number = 20,
  ): Observable<PagedResult<TicketListItem>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<TicketListItem>>(`${this.apiUrl}/my`, { params });
  }

  /** GET /api/tickets/assigned — tickets assigned to the current agent */
  override getAssignedTickets(
    page: number = 1,
    pageSize: number = 20,
  ): Observable<PagedResult<TicketListItem>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<PagedResult<TicketListItem>>(`${this.apiUrl}/assigned`, { params });
  }

  /** GET /api/tickets — searchable all-tickets list (requires ticket:read policy) */
  override searchTickets(opts: SearchTicketsParams = {}): Observable<PagedResult<TicketListItem>> {
    let params = new HttpParams()
      .set('page', (opts.page ?? 1).toString())
      .set('pageSize', (opts.pageSize ?? 20).toString());
    if (opts.search) params = params.set('Search', opts.search);
    if (opts.status) params = params.set('Status', opts.status);
    if (opts.assignedAgentId) params = params.set('AssignedAgentId', opts.assignedAgentId);
    if (opts.groupId) params = params.set('GroupId', opts.groupId);
    if (opts.priority) params = params.set('Priority', opts.priority);
    if (opts.unassigned !== undefined) params = params.set('Unassigned', opts.unassigned.toString());
    if (opts.breached !== undefined) params = params.set('Breached', opts.breached.toString());
    return this.http.get<PagedResult<TicketListItem>>(this.apiUrl, { params });
  }

  /** GET /api/tickets/{id} — returns full ticket with embedded comments & activities */
  override getTicketById(id: string): Observable<TicketDetails> {
    return this.http.get<TicketDetails>(`${this.apiUrl}/${id}`);
  }

  /** GET /api/tickets/{id}/comments */
  override getComments(id: string, includeInternal: boolean = false): Observable<TicketComment[]> {
    const params = new HttpParams().set('includeInternal', includeInternal.toString());
    return this.http.get<TicketComment[]>(`${this.apiUrl}/${id}/comments`, { params });
  }

  /** GET /api/tickets/{id}/activities */
  override getActivities(id: string): Observable<TicketActivity[]> {
    return this.http.get<TicketActivity[]>(`${this.apiUrl}/${id}/activities`);
  }

  // ── Commands ─────────────────────────────────────────────────────────────

  /**
   * POST /api/tickets
   * ActorId & IsCustomer are injected server-side from JWT — do NOT send them.
   */
  override createTicket(
    request: CreateTicketRequest,
  ): Observable<{ ticketId: string; message: string }> {
    return this.http.post<{ ticketId: string; message: string }>(this.apiUrl, request);
  }

  /** PUT /api/tickets/{id} — actorId injected from JWT */
  override updateTicket(
    id: string,
    request: UpdateTicketRequest,
  ): Observable<{ ticketId: string; message: string }> {
    return this.http.put<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}`, request);
  }

  /** DELETE /api/tickets/{id} */
  override deleteTicket(id: string): Observable<{ ticketId: string; message: string }> {
    return this.http.delete<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}`);
  }

  /**
   * POST /api/tickets/{id}/assign
   * AssignedBy is injected from JWT server-side — only agentId & optional notes needed.
   */
  override assignTicket(
    id: string,
    agentId?: string,
    notes?: string,
    groupId?: string,
  ): Observable<{ ticketId: string; agentId: string; message: string }> {
    return this.http.post<{ ticketId: string; agentId: string; message: string }>(
      `${this.apiUrl}/${id}/assign`,
      { agentId: agentId ?? null, groupId: groupId ?? null, notes: notes ?? null },
    );
  }

  /**
   * POST /api/tickets/{id}/status
   * ChangedBy is injected from JWT server-side.
   * newStatus must match TicketStatus enum: New|Open|Assigned|InProgress|Pending|Resolved|Closed|Reopened
   */
  override changeStatus(
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
  override resolveTicket(
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
  override closeTicket(
    id: string,
    notes?: string,
  ): Observable<{ ticketId: string; message: string }> {
    return this.http.post<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}/close`, {
      notes,
    });
  }

  /**
   * POST /api/tickets/{id}/reopen
   * ReopenedBy injected from JWT server-side.
   */
  override reopenTicket(
    id: string,
    reason?: string,
  ): Observable<{ ticketId: string; message: string }> {
    return this.http.post<{ ticketId: string; message: string }>(`${this.apiUrl}/${id}/reopen`, {
      reason,
    });
  }

  /**
   * POST /api/tickets/{id}/comments
   * AuthorId/ContactId resolved from JWT server-side — only body & visibility needed.
   * visibility: 'Public' | 'Internal'
   */
  override addComment(id: string, request: AddCommentRequest): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/${id}/comments`, request);
  }

  override getActiveBanners(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiBaseUrl}/api/kb/banners/active`);
  }
}
