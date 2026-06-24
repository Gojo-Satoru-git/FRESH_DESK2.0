import { Injectable } from '@angular/core';
import { Observable, delay, of, throwError } from 'rxjs';
import { AuthService, LoginResponse, User } from './auth.service';

interface MockUser extends User {
  password: string;
  permissions: string[];
  groups: string[];
  companyId: string | null;
  contactId: string | null;
}

const mockUsers: MockUser[] = [
  {
    id: 'mock-admin-001',
    email: 'admin@example.com',
    password: 'password',
    role: 'admin',
    firstName: 'Asha',
    lastName: 'Admin',
    fullName: 'Asha Admin',
    permissions: ['ticket:read', 'ticket:write', 'admin:read', 'admin:write'],
    groups: ['group-admin'],
    companyId: null,
    contactId: null,
  },
  {
    id: 'agent-001',
    email: 'agent@example.com',
    password: 'password',
    role: 'agent',
    firstName: 'Anika',
    lastName: 'Rao',
    fullName: 'Anika Rao',
    permissions: ['ticket:read', 'ticket:write'],
    groups: ['group-support'],
    companyId: 'company-001',
    contactId: null,
  },
  {
    id: 'lead-001',
    email: 'teamlead@example.com',
    password: 'password',
    role: 'team_lead',
    firstName: 'Kabir',
    lastName: 'Lead',
    fullName: 'Kabir Lead',
    permissions: ['ticket:read', 'ticket:write', 'team:assign'],
    groups: ['group-support'],
    companyId: 'company-001',
    contactId: null,
  },
  {
    id: 'customer-001',
    email: 'customer@example.com',
    password: 'password',
    role: 'customer',
    firstName: 'Rohan',
    lastName: 'Iyer',
    fullName: 'Rohan Iyer',
    permissions: ['ticket:read', 'ticket:create'],
    groups: ['group-customer'],
    companyId: 'company-001',
    contactId: 'contact-001',
  },
];

@Injectable()
export class MockAuthService extends AuthService {
  constructor() {
    super();
    this.initializeFromStoredToken();
  }

  override loadUserMetadata(): void {
    const user = this.currentUser();
    const mockUser = user ? mockUsers.find((item) => item.email === user.email) : null;

    this.permissions.set(mockUser?.permissions ?? []);
    this.groups.set(mockUser?.groups ?? []);
    this.companyId.set(mockUser?.companyId ?? null);
    this.contactId.set(mockUser?.contactId ?? null);
  }

  override login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    const email = credentials.email.trim().toLowerCase();
    const mockUser = mockUsers.find(
      (user) => user.email === email && user.password === credentials.password,
    );

    if (!mockUser) {
      return throwError(() => ({ error: { message: 'Invalid mock email or password.' } })).pipe(
        delay(250),
      );
    }

    return of({
      userId: {
        accessToken: this.createMockToken(mockUser),
        expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
      },
      message: 'Mock login successful.',
    }).pipe(delay(250));
  }

  override register(userData: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    phone: string;
    username?: string;
  }): Observable<{ userId: string }> {
    return of({ userId: `mock-${userData.email}` }).pipe(delay(250));
  }

  private createMockToken(user: MockUser): string {
    const header = this.toBase64Url({ alg: 'none', typ: 'JWT' });
    const payload = this.toBase64Url({
      sub: user.id,
      email: user.email,
      role: user.role,
      given_name: user.firstName,
      family_name: user.lastName,
      name: user.fullName,
      exp: Math.floor(Date.now() / 1000) + 60 * 60,
    });

    return `${header}.${payload}.mock-signature`;
  }

  private toBase64Url(value: unknown): string {
    const json = JSON.stringify(value);
    const base64 = btoa(unescape(encodeURIComponent(json)));
    return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
  }
}
