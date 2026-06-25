import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PagedResult } from '../../tickets/models/ticket.model';
import { environment } from '../../../../environments/environment';

export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  isEmailVerified: boolean;
  createdAt: string;
}

export interface CreateInternalUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  roleName: string;
}

export interface Role {
  id: string;
  name: string;
  description: string;
  isSystemRole: boolean;
  createdAt: string;
}

export interface Permission {
  id: string;
  resource: string;
  action: string;
  description: string;
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
}

export interface Group {
  id: string;
  name: string;
  regionCode?: string;
  tierCode?: string;
  unattendedAlertMinutes: number;
  assignmentStrategy: number;
  fallbackGroupId?: string;
}

export interface CreateGroupRequest {
  name: string;
  regionCode?: string;
  tierCode?: string;
  unattendedAlertMinutes?: number;
  assignmentStrategy?: number;
  fallbackGroupId?: string;
}

export interface Company {
  id: string;
  name: string;
  geoRegion: string;
  supportTier: string;
  industry: string;
  notes?: string;
  status: string;
  createdAt: string;
}

export interface CreateCompanyRequest {
  name: string;
  geoRegion: string;
  supportTier: string;
  industry?: string;
  notes?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private rbacApiUrl = `${environment.apiBaseUrl}/api/rbac/users`;
  private adminApiUrl = `${environment.apiBaseUrl}/api/admin`;
  private rolesApiUrl = `${environment.apiBaseUrl}/api/rbac/roles`;
  private permissionsApiUrl = `${environment.apiBaseUrl}/api/rbac/permissions`;
  private groupsApiUrl = `${environment.apiBaseUrl}/api/rbac/groups`;
  private companiesApiUrl = `${environment.apiBaseUrl}/api/companies`;

  // --- Users ---
  getUsers(page: number = 1, pageSize: number = 20, email?: string, isActive?: boolean): Observable<PagedResult<UserSummary>> {
    let params = new HttpParams()
      .set('pageNumber', page.toString())
      .set('pageSize', pageSize.toString());

    if (email) params = params.set('email', email);
    if (isActive !== undefined) params = params.set('isActive', isActive.toString());

    return this.http.get<PagedResult<UserSummary>>(this.rbacApiUrl, { params });
  }

  createInternalUser(request: CreateInternalUserRequest): Observable<{ userId: string }> {
    return this.http.post<{ userId: string }>(`${this.adminApiUrl}/internal-users`, request);
  }

  // --- Roles ---
  getRoles(): Observable<Role[]> {
    return this.http.get<Role[]>(this.rolesApiUrl);
  }

  createRole(request: CreateRoleRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.rolesApiUrl, request);
  }

  getRolePermissions(roleId: string): Observable<{ roleId: string, permissions: Permission[] }> {
    return this.http.get<{ roleId: string, permissions: Permission[] }>(`${this.rolesApiUrl}/${roleId}/permissions`);
  }

  setRolePermissions(roleId: string, permissionIds: string[]): Observable<void> {
    return this.http.put<void>(`${this.rolesApiUrl}/${roleId}/permissions`, { permissionIds });
  }

  // --- Permissions ---
  getPermissions(): Observable<Permission[]> {
    return this.http.get<Permission[]>(this.permissionsApiUrl);
  }

  // --- User Roles ---
  assignRoleToUser(userId: string, roleId: string): Observable<void> {
    return this.http.post<void>(`${this.rbacApiUrl}/${userId}/roles/assign`, { roleId });
  }

  removeRoleFromUser(userId: string, roleId: string): Observable<void> {
    return this.http.post<void>(`${this.rbacApiUrl}/${userId}/roles/remove`, { roleId });
  }

  // --- Groups ---
  getGroups(): Observable<Group[]> {
    return this.http.get<Group[]>(this.groupsApiUrl);
  }

  createGroup(request: CreateGroupRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.groupsApiUrl, request);
  }

  deleteGroup(groupId: string): Observable<void> {
    return this.http.delete<void>(`${this.groupsApiUrl}/${groupId}`);
  }

  addGroupMember(groupId: string, userId: string, isLead: boolean = false): Observable<void> {
    return this.http.post<void>(`${this.groupsApiUrl}/${groupId}/members/add`, { userId, isLead });
  }

  removeGroupMember(groupId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.groupsApiUrl}/${groupId}/members/remove`, { userId });
  }

  getGroupMembers(groupId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.groupsApiUrl}/${groupId}/members`);
  }

  // --- Companies ---
  getCompanies(page: number = 1, pageSize: number = 20): Observable<PagedResult<Company>> {
    let params = new HttpParams()
      .set('pageNumber', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<PagedResult<Company>>(this.companiesApiUrl, { params });
  }

  createCompany(request: CreateCompanyRequest): Observable<{ companyId: string }> {
    return this.http.post<{ companyId: string }>(this.companiesApiUrl, request);
  }
}
