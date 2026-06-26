import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../../environments/environment';

// ─── PagedResult (re-declared here to avoid circular imports) ─────────────────
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// ─── Raw API DTOs (camelCase JSON from .NET) ──────────────────────────────────

export interface AgentWorkloadApiDto {
  agentId: string;
  agentName: string;
  openTickets: number;
  overdueTickets: number;
}

export interface GroupDashboardApiDto {
  groupId: string;
  groupName: string;
  totalTickets: number;
  unassignedCount: number;
  assignedCount: number;
  overdueCount: number;
  criticalCount: number;
  escalatedCount: number;
  agentWorkloads: AgentWorkloadApiDto[];
}

export interface LeadDashboardApiDto {
  groups: GroupDashboardApiDto[];
  totalUnassigned: number;
  totalOverdue: number;
  totalCritical: number;
}

// ─── View models used by components ──────────────────────────────────────────

export interface GroupDashboard {
  groupId: string;
  groupName: string;
  totalTickets: number;
  totalActive: number;
  unassigned: number;
  slasBreached: number;
  agentWorkloads: AgentWorkloadApiDto[];
}

export interface LeadDashboard {
  totalGroupsManaged: number;
  totalUnassigned: number;
  totalOverdue: number;
  totalCritical: number;
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

/** Flat member shape coming from /my-members (EnterpriseGroupMemberDto) */
export interface GroupMember {
  userId: string;
  name: string;       // Full name, e.g. "Alice Smith"
  email: string;
  isLead: boolean;
  roles: string[];
}

export interface GroupWithMembers {
  groupId: string;
  groupName: string;
  members: GroupMember[];
}

// ─── Raw /my-members shapes ───────────────────────────────────────────────────
interface RawGroupMemberDto {
  userId: string;
  name: string;
  email: string;
  isLead: boolean;
  roles: string[];
}
interface RawGroupWithMembersDto {
  group: { id: string; name: string };
  members: RawGroupMemberDto[];
}

// ─── Mapper ───────────────────────────────────────────────────────────────────
function mapLeadDashboard(dto: LeadDashboardApiDto): LeadDashboard {
  const groups: GroupDashboard[] = (dto.groups ?? []).map((g) => ({
    groupId: g.groupId,
    groupName: g.groupName,
    totalTickets: g.totalTickets,
    totalActive: g.assignedCount,
    unassigned: g.unassignedCount,
    slasBreached: g.overdueCount,
    agentWorkloads: g.agentWorkloads ?? [],
  }));
  return {
    totalGroupsManaged: groups.length,
    totalUnassigned: dto.totalUnassigned,
    totalOverdue: dto.totalOverdue,
    totalCritical: dto.totalCritical,
    groupDashboards: groups,
  };
}

function mapGroupWithMembers(raw: RawGroupWithMembersDto): GroupWithMembers {
  return {
    groupId: raw.group?.id ?? '',
    groupName: raw.group?.name ?? '',
    members: (raw.members ?? []).map((m) => ({
      userId: m.userId,
      name: m.name ?? m.email,
      email: m.email,
      isLead: m.isLead,
      roles: m.roles ?? [],
    })),
  };
}

// ─── Service ──────────────────────────────────────────────────────────────────
@Injectable({ providedIn: 'root' })
export class TeamLeadService {
  private http = inject(HttpClient);
  private base = `${environment.apiBaseUrl}/api/rbac/groups`;

  getLeadDashboard(): Observable<LeadDashboard> {
    return this.http
      .get<LeadDashboardApiDto>(`${this.base}/lead-dashboard`)
      .pipe(map(mapLeadDashboard));
  }

  getGroupQueue(
    groupId: string,
    queueType: string = 'all',
    page: number = 1,
    pageSize: number = 20
  ): Observable<GroupQueueResult> {
    const params = new HttpParams()
      .set('queueType', queueType)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<GroupQueueResult>(`${this.base}/${groupId}/queue`, { params });
  }

  getGroupWorkload(groupId: string): Observable<AgentWorkload[]> {
    return this.http.get<AgentWorkload[]>(`${this.base}/${groupId}/workload`);
  }

  getGroupMembers(groupId: string): Observable<GroupWithMembers> {
    return this.http
      .get<RawGroupWithMembersDto>(`${this.base}/${groupId}/my-members`)
      .pipe(map(mapGroupWithMembers));
  }
}