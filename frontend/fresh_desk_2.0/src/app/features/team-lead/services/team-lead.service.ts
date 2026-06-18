import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../tickets/models/ticket.model';

export interface GroupDashboard {
  groupId: string;
  groupName: string;
  totalTickets: number;
  totalActive: number;
  inProgress: number;
  pendingReply: number;
  resolvedClosed: number;
  unassigned: number;
  slasBreached: number;
}

export interface LeadDashboard {
  totalGroupsManaged: number;
  groupDashboards: GroupDashboard[];
}

export interface TicketListItem {
  id: string;
  ticketNumber: string;
  title: string;
  status: string;
  priority: string;
  companyId: string;
  createdAt: string;
  updatedAt: string;
  assignedAgentId?: string;
  assignedAgentName?: string;
}

export interface GroupQueueResult {
  groupId: string;
  groupName: string;
  queueType: string;
  tickets: PagedResult<TicketListItem>;
}

export interface AgentWorkload {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  activeTicketsCount: number;
  inProgressCount: number;
  pendingReplyCount: number;
  averageResolutionTimeHours?: number;
  csatScore?: number;
  capacityPercentage: number;
}

export interface GroupMember {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  isLead: boolean;
  joinedAt: string;
}

export interface GroupWithMembers {
  id: string;
  name: string;
  members: GroupMember[];
}

@Injectable({
  providedIn: 'root'
})
export class TeamLeadService {
  private http = inject(HttpClient);
  private groupsApiUrl = '/api/rbac/groups';

  getLeadDashboard(): Observable<LeadDashboard> {
    return this.http.get<LeadDashboard>(`${this.groupsApiUrl}/lead-dashboard`);
  }

  getGroupQueue(groupId: string, queueType: string = 'all', page: number = 1, pageSize: number = 20): Observable<GroupQueueResult> {
    let params = new HttpParams()
      .set('queueType', queueType)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<GroupQueueResult>(`${this.groupsApiUrl}/${groupId}/queue`, { params });
  }

  getGroupWorkload(groupId: string): Observable<AgentWorkload[]> {
    return this.http.get<AgentWorkload[]>(`${this.groupsApiUrl}/${groupId}/workload`);
  }

  getGroupMembers(groupId: string): Observable<GroupWithMembers> {
    return this.http.get<GroupWithMembers>(`${this.groupsApiUrl}/${groupId}/my-members`);
  }
}
