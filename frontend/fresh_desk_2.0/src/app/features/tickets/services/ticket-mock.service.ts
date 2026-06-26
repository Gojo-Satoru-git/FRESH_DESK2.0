import { Injectable } from '@angular/core';
import { Observable, delay, of } from 'rxjs';
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
import { mockAgents, mockTickets, toTicketListItem } from './mock-ticket.data';
import { SearchTicketsParams, TicketService } from './ticket.service';

@Injectable()
export class MockTicketService extends TicketService {
  private readonly latencyMs = 250;
  private tickets = [...mockTickets];

  override getAgents(): Observable<any> {
    return this.respond({
      items: mockAgents,
      totalCount: mockAgents.length,
      page: 1,
      pageSize: 100,
      totalPages: 1,
    });
  }

  override getUserWithRoles(userId: string): Observable<any> {
    const user = mockAgents.find((agent) => agent.id === userId);
    return this.respond({
      ...(user ?? { id: userId, firstName: 'Mock', lastName: 'User' }),
      roles: ['agent'],
    });
  }

  override getUserGroups(userId: string): Observable<any[]> {
    return this.respond([
      {
        id: 'group-support',
        name: 'Support',
        members: mockAgents.map((agent) => ({ userId: agent.id, ...agent })),
      },
    ]);
  }

  override getGroupMembers(groupId: string): Observable<any> {
    return this.respond({
      id: groupId,
      members: mockAgents.map((agent) => ({ userId: agent.id, ...agent })),
    });
  }

  override getDashboard(companyId?: string): Observable<TicketDashboard> {
    const tickets = companyId
      ? this.tickets.filter((ticket) => ticket.companyId === companyId)
      : this.tickets;
    const active = tickets.filter((ticket) => !['Resolved', 'Closed'].includes(ticket.status));
    const inProgress = tickets.filter((ticket) => ticket.status === 'InProgress').length;
    const pending = tickets.filter((ticket) => ticket.status === 'Pending').length;
    const resolvedClosed = tickets.filter((ticket) =>
      ['Resolved', 'Closed'].includes(ticket.status),
    ).length;

    return this.respond({
      totalTickets: tickets.length,
      totalActive: active.length,
      inProgress,
      pendingReply: pending,
      resolvedClosed,
      counts: {
        total: tickets.length,
        unassigned: tickets.filter((ticket) => !ticket.assignedAgentId).length,
        open: tickets.filter((ticket) => ticket.status === 'Open').length,
        pending,
      },
      performance: {
        receivedToday: tickets.filter((ticket) => ticket.createdAt.startsWith('2026-06-24')).length,
        resolvedToday: 1,
        resolutionRate: tickets.length ? Math.round((resolvedClosed / tickets.length) * 100) : null,
      },
      trends: [
        { timeLabel: '09:00', todayCount: 2, yesterdayCount: 1 },
        { timeLabel: '12:00', todayCount: 3, yesterdayCount: 2 },
        { timeLabel: '15:00', todayCount: 1, yesterdayCount: 4 },
      ],
      todos: active.slice(0, 3).map((ticket) => ({
        id: ticket.id,
        title: ticket.title,
        due: ticket.priority === 'High' ? 'Today' : 'Tomorrow',
      })),
      groupMetrics: [
        { groupId: 'group-support', groupName: 'Support', ticketCount: active.length },
      ],
      agentWorkloads: mockAgents.map((agent) => ({
        agentId: agent.id,
        agentName: `${agent.firstName} ${agent.lastName}`,
        openTickets: tickets.filter((ticket) => ticket.assignedAgentId === agent.id).length,
        overdueTickets: agent.id === 'agent-001' ? 1 : 0,
      })),
      slaMetrics: {
        breachedCount: 1,
        atRiskCount: 2,
      },
    });
  }

  override getMyTickets(
    page: number = 1,
    pageSize: number = 20,
  ): Observable<PagedResult<TicketListItem>> {
    return this.respond(this.toPagedResult(this.tickets.map(toTicketListItem), page, pageSize));
  }

  override getAssignedTickets(
    page: number = 1,
    pageSize: number = 20,
  ): Observable<PagedResult<TicketListItem>> {
    const assigned = this.tickets
      .filter((ticket) => Boolean(ticket.assignedAgentId))
      .map(toTicketListItem);
    return this.respond(this.toPagedResult(assigned, page, pageSize));
  }

  override searchTickets(opts: SearchTicketsParams = {}): Observable<PagedResult<TicketListItem>> {
    const filtered = this.tickets.filter((ticket) => {
      const matchesTicketNumber =
        !opts.ticketNumber ||
        ticket.ticketNumber?.toLowerCase().includes(opts.ticketNumber.toLowerCase());
      const matchesStatus = !opts.status || ticket.status === opts.status;
      const matchesAgent = !opts.assignedAgentId || ticket.assignedAgentId === opts.assignedAgentId;
      const matchesCompany = !opts.companyId || ticket.companyId === opts.companyId;

      return matchesTicketNumber && matchesStatus && matchesAgent && matchesCompany;
    });

    return this.respond(
      this.toPagedResult(filtered.map(toTicketListItem), opts.page ?? 1, opts.pageSize ?? 20),
    );
  }

  override getTicketById(id: string): Observable<TicketDetails> {
    const ticket = this.requireTicket(id);
    return this.respond({ ...ticket });
  }

  override getComments(id: string, includeInternal: boolean = false): Observable<TicketComment[]> {
    const ticket = this.requireTicket(id);
    const comments = includeInternal
      ? ticket.comments
      : ticket.comments.filter((comment) => comment.visibility !== 'Internal');
    return this.respond(comments);
  }

  override getActivities(id: string): Observable<TicketActivity[]> {
    return this.respond(this.requireTicket(id).activities ?? []);
  }

  override createTicket(
    request: CreateTicketRequest,
  ): Observable<{ ticketId: string; message: string }> {
    const ticketId = `ticket-${String(this.tickets.length + 1).padStart(3, '0')}`;
    const ticketNumber = `FD-${1000 + this.tickets.length + 1}`;
    this.tickets = [
      {
        id: ticketId,
        ticketNumber,
        title: request.title,
        description: request.description,
        status: request.assigneeId ? 'Assigned' : 'Open',
        priority: request.priority,
        category: request.type,
        assignedAgentId: request.assigneeId ?? undefined,
        companyId: 'company-mock',
        moduleName: request.moduleName ?? undefined,
        createdAt: new Date().toISOString(),
        tags: request.tags ?? [],
        type: request.type,
        comments: [],
        statusHistory: [],
        assignmentLogs: [],
        activities: [],
        attachments: [],
      },
      ...this.tickets,
    ];

    return this.respond({ ticketId, message: 'Mock ticket created successfully.' });
  }

  override updateTicket(
    id: string,
    request: UpdateTicketRequest,
  ): Observable<{ ticketId: string; message: string }> {
    this.tickets = this.tickets.map((ticket) =>
      ticket.id === id
        ? {
            ...ticket,
            title: request.title,
            description: request.description,
            priority: request.priority,
            type: request.type,
            tags: request.tags,
            updatedAt: new Date().toISOString(),
          }
        : ticket,
    );

    return this.respond({ ticketId: id, message: 'Mock ticket updated successfully.' });
  }

  override deleteTicket(id: string): Observable<{ ticketId: string; message: string }> {
    this.tickets = this.tickets.filter((ticket) => ticket.id !== id);
    return this.respond({ ticketId: id, message: 'Mock ticket deleted successfully.' });
  }

  override assignTicket(
    id: string,
    agentId: string,
    notes?: string,
  ): Observable<{ ticketId: string; agentId: string; message: string }> {
    this.tickets = this.tickets.map((ticket) =>
      ticket.id === id
        ? {
            ...ticket,
            assignedAgentId: agentId,
            assignedAgentName: this.getAgentName(agentId),
            status: ticket.status === 'Open' ? 'Assigned' : ticket.status,
            updatedAt: new Date().toISOString(),
            assignmentLogs: [
              ...ticket.assignmentLogs,
              {
                id: `assignment-${Date.now()}`,
                agentId,
                assignedBy: 'mock-user',
                assignedAt: new Date().toISOString(),
                notes,
              },
            ],
          }
        : ticket,
    );

    return this.respond({ ticketId: id, agentId, message: 'Mock ticket assigned successfully.' });
  }

  override changeStatus(
    id: string,
    newStatus: string,
    reason?: string,
  ): Observable<{ ticketId: string; newStatus: string; message: string }> {
    this.tickets = this.tickets.map((ticket) =>
      ticket.id === id
        ? {
            ...ticket,
            status: newStatus,
            updatedAt: new Date().toISOString(),
            statusHistory: [
              ...ticket.statusHistory,
              {
                id: `history-${Date.now()}`,
                oldStatus: ticket.status,
                newStatus,
                changedBy: 'mock-user',
                changedAt: new Date().toISOString(),
                reason,
              },
            ],
          }
        : ticket,
    );

    return this.respond({ ticketId: id, newStatus, message: 'Mock status changed successfully.' });
  }

  override resolveTicket(
    id: string,
    resolutionSummary: string,
  ): Observable<{ ticketId: string; message: string }> {
    this.addInternalComment(id, resolutionSummary);
    this.setStatus(id, 'Resolved');
    return this.respond({ ticketId: id, message: 'Mock ticket resolved successfully.' });
  }

  override closeTicket(
    id: string,
    notes?: string,
  ): Observable<{ ticketId: string; message: string }> {
    if (notes) this.addInternalComment(id, notes);
    this.setStatus(id, 'Closed');
    return this.respond({ ticketId: id, message: 'Mock ticket closed successfully.' });
  }

  override reopenTicket(
    id: string,
    reason?: string,
  ): Observable<{ ticketId: string; message: string }> {
    if (reason) this.addInternalComment(id, reason);
    this.setStatus(id, 'Reopened');
    return this.respond({ ticketId: id, message: 'Mock ticket reopened successfully.' });
  }

  override addComment(id: string, request: AddCommentRequest): Observable<string> {
    this.tickets = this.tickets.map((ticket) =>
      ticket.id === id
        ? {
            ...ticket,
            comments: [
              ...ticket.comments,
              {
                id: `comment-${Date.now()}`,
                authorId: 'mock-user',
                body: request.body,
                visibility: request.isPrivate ? 'Internal' : 'Public',
                createdAt: new Date().toISOString(),
                attachments: [],
                authorName: 'Mock User',
              },
            ],
          }
        : ticket,
    );

    return this.respond('Mock comment added successfully.');
  }

  override getActiveBanners(): Observable<any[]> {
    return this.respond([
      {
        id: 'banner-001',
        title: 'Mock service window',
        message: 'Frontend mock data is enabled for local development.',
        severity: 'info',
      },
    ]);
  }

  private toPagedResult<T>(items: T[], page: number, pageSize: number): PagedResult<T> {
    const totalCount = items.length;
    const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
    const start = (page - 1) * pageSize;

    return {
      items: items.slice(start, start + pageSize),
      totalCount,
      page,
      pageSize,
      totalPages,
    };
  }

  private requireTicket(id: string): TicketDetails {
    const ticket = this.tickets.find((item) => item.id === id);
    if (!ticket) {
      throw new Error(`Mock ticket not found: ${id}`);
    }

    return ticket;
  }

  private setStatus(id: string, status: string): void {
    this.tickets = this.tickets.map((ticket) =>
      ticket.id === id
        ? {
            ...ticket,
            status,
            updatedAt: new Date().toISOString(),
          }
        : ticket,
    );
  }

  private addInternalComment(id: string, body: string): void {
    this.tickets = this.tickets.map((ticket) =>
      ticket.id === id
        ? {
            ...ticket,
            comments: [
              ...ticket.comments,
              {
                id: `comment-${Date.now()}`,
                authorId: 'mock-user',
                body,
                visibility: 'Internal',
                createdAt: new Date().toISOString(),
                attachments: [],
                authorName: 'Mock User',
              },
            ],
          }
        : ticket,
    );
  }

  private getAgentName(agentId: string): string {
    const agent = mockAgents.find((item) => item.id === agentId);
    return agent ? `${agent.firstName} ${agent.lastName}` : 'Mock Agent';
  }

  private respond<T>(value: T): Observable<T> {
    return of(value).pipe(delay(this.latencyMs));
  }
}
