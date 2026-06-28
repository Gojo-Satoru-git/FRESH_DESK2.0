import { Observable } from 'rxjs';
import {
  AddCommentRequest,
  CreateTicketRequest,
  PagedResult,
  TicketActivity,
  TicketComment,
  TicketDashboard,
  TicketDetails,
  TicketListItem,
  UpdateTicketRequest,
} from '../models/ticket.model';

export interface SearchTicketsParams {
  groupId?: string;
  assignedAgentId?: string;
  status?: string;
  priority?: string;
  unassigned?: boolean;
  breached?: boolean;
  search?: string;
  page?: number;
  pageSize?: number;
}

export abstract class TicketService {
  abstract getAgents(): Observable<any>;
  abstract getUserWithRoles(userId: string): Observable<any>;
  abstract getUserGroups(userId: string): Observable<any[]>;
  abstract getGroupMembers(groupId: string): Observable<any>;
  abstract getDashboard(companyId?: string): Observable<TicketDashboard>;
  abstract getMyTickets(page?: number, pageSize?: number): Observable<PagedResult<TicketListItem>>;
  abstract getAssignedTickets(
    page?: number,
    pageSize?: number,
  ): Observable<PagedResult<TicketListItem>>;
  abstract searchTickets(opts?: SearchTicketsParams): Observable<PagedResult<TicketListItem>>;
  abstract getTicketById(id: string): Observable<TicketDetails>;
  abstract getComments(id: string, includeInternal?: boolean): Observable<TicketComment[]>;
  abstract getActivities(id: string): Observable<TicketActivity[]>;
  abstract createTicket(
    request: CreateTicketRequest,
  ): Observable<{ ticketId: string; message: string }>;
  abstract updateTicket(
    id: string,
    request: UpdateTicketRequest,
  ): Observable<{ ticketId: string; message: string }>;
  abstract deleteTicket(id: string): Observable<{ ticketId: string; message: string }>;
  abstract assignTicket(
    id: string,
    agentId?: string,
    notes?: string,
    groupId?: string,
  ): Observable<{ ticketId: string; agentId: string; message: string }>;
  abstract changeStatus(
    id: string,
    newStatus: string,
    reason?: string,
  ): Observable<{ ticketId: string; newStatus: string; message: string }>;
  abstract resolveTicket(
    id: string,
    resolutionSummary: string,
  ): Observable<{ ticketId: string; message: string }>;
  abstract closeTicket(
    id: string,
    notes?: string,
  ): Observable<{ ticketId: string; message: string }>;
  abstract reopenTicket(
    id: string,
    reason?: string,
  ): Observable<{ ticketId: string; message: string }>;
  abstract addComment(id: string, request: AddCommentRequest): Observable<string>;
  abstract getActiveBanners(): Observable<any[]>;
}
