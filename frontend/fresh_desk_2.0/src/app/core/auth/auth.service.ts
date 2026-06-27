import { Injectable, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';

export interface User {
  id: string;
  email: string;
  role: 'admin' | 'agent' | 'team_lead' | 'supervisor' | 'customer';
  firstName?: string;
  lastName?: string;
  fullName?: string;
}

export interface LoginResponse {
  userId: {
    accessToken: string;
    expiresAt: string;
  };
  message: string;
}

@Injectable()
export abstract class AuthService {
  protected router = inject(Router);
  protected readonly tokenKey = 'jwt_token';

  currentUser = signal<User | null>(null);
  permissions = signal<string[]>([]);
  groups = signal<string[]>([]);
  companyId = signal<string | null>(null);
  contactId = signal<string | null>(null);

  abstract loadUserMetadata(): Observable<any>;
  abstract login(credentials: { email: string; password: string }): Observable<LoginResponse>;
  abstract register(userData: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    phone: string;
    username?: string;
  }): Observable<{ userId: string }>;

  handleLoginSuccess(token: string, user: User): void {
    localStorage.setItem(this.tokenKey, token);
    this.currentUser.set(user);

    // We subscribe to ensure metadata is loaded before routing
    this.loadUserMetadata().subscribe({
      next: () => {
        // EVERYONE except customers goes to the unified workspace!
        if (user.role === 'customer') {
          this.router.navigate(['/customer-portal']);
        } else {
          this.router.navigate(['/workspace/dashboard']);
        }
      },
      error: (err) => {
        console.error('Failed to load metadata during login success', err);
        this.logout();
      },
    });
  }

  getUserFromToken(token: string): User | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;
      const base64Url = parts[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        window
          .atob(base64)
          .split('')
          .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join(''),
      );
      const payload = JSON.parse(jsonPayload);
      const roleClaim =
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'];

      let role: User['role'] = 'customer';
      if (roleClaim) {
        const roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];
        const lowerRoles = roles.map((r: string) => r.toLowerCase());
        if (lowerRoles.includes('admin')) {
          role = 'admin';
        } else if (lowerRoles.includes('team_lead')) {
          role = 'team_lead';
        } else if (lowerRoles.some((r: string) => r === 'agent' || r.endsWith('_agent'))) {
          role = 'agent';
        } else if (lowerRoles.includes('supervisor')) {
          role = 'supervisor';
        } else if (lowerRoles.includes('customer')) {
          role = 'customer';
        }
      }
      const firstName =
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] ||
        payload['given_name'] ||
        '';
      const lastName =
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname'] ||
        payload['family_name'] ||
        '';
      const fullName =
        payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        payload['name'] ||
        '';

      return {
        id: payload.sub,
        email: payload.email,
        role,
        firstName,
        lastName,
        fullName: fullName || `${firstName} ${lastName}`.trim(),
      };
    } catch {
      return null;
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.currentUser.set(null);
    this.permissions.set([]);
    this.groups.set([]);
    this.companyId.set(null);
    this.contactId.set(null);
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }
  // --- NEW PBAC HELPER METHODS ---
  hasPermission(permission: string): boolean {
    return this.permissions().includes(permission);
  }

  hasAnyPermission(permissions: string[]): boolean {
    if (!permissions || permissions.length === 0) return true;
    const userPerms = this.permissions();
    return permissions.some((p) => userPerms.includes(p));
  }

  hasAllPermissions(permissions: string[]): boolean {
    if (!permissions || permissions.length === 0) return true;
    const userPerms = this.permissions();
    return permissions.every((p) => userPerms.includes(p));
  }

  initializeSession(): Promise<void> {
    return new Promise((resolve) => {
      const token = this.getToken();
      if (!token) {
        resolve(); // No token, let the app boot (they are logged out)
        return;
      }

      const user = this.getUserFromToken(token);
      if (user) {
        this.currentUser.set(user);

        // We need to wait for loadUserMetadata to finish.
        // We will modify loadUserMetadata to accept an optional callback or return an Observable.
        this.loadUserMetadata().subscribe({
          next: () => resolve(),
          error: () => {
            this.logout();
            resolve();
          },
        });
      } else {
        this.logout();
        resolve();
      }
    });
  }
}
