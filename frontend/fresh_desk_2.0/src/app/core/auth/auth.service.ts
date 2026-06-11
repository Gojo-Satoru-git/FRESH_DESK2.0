import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment.development';
import { of, delay } from 'rxjs';

export interface User {
  id: string;
  email: string;
  role: 'admin' | 'agent' | 'team_lead' | 'supervisor' | 'customer';
  firstName?: string;
  lastName?: string;
  fullName?: string;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  // Global Signal holding the logged-in user state
  currentUser = signal<User | null>(null);

  // The key we use to store the token in the browser
  private readonly TOKEN_KEY = 'jwt_token';

  constructor() {
    const token = this.getToken();
    if (token) {
      const user = this.getUserFromToken(token);
      if (user) {
        this.currentUser.set(user);
      } else {
        this.logout();
      }
    }
  }

  // --- LOGIN LOGIC ---
  login(credentials: { email: string; password: string }) {
    return this.http.post<{ userId: { accessToken: string; expiresAt: string }; message: string }>(
      `${environment.apiUrl}/api/auth/login`,
      credentials,
    );
  }

  // --- REGISTRATION LOGIC ---
  register(userData: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    phone: string;
    username?: string;
  }) {
    return this.http.post<{ userId: string }>(`${environment.apiUrl}/api/auth/register`, userData);
  }

  // --- ROUTING & STATE MANAGEMENT ---
  handleLoginSuccess(token: string, user: User) {
    localStorage.setItem(this.TOKEN_KEY, token); // Save token securely
    this.currentUser.set(user); // Update the global signal

    // The Traffic Cop Logic
    if (user.role === 'customer') {
      this.router.navigate(['/customer-portal']);
    } else {
      this.router.navigate(['/agent/dashboard']);
    }
  }

  // Decode JWT payload to get user info
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

      let role: 'admin' | 'agent' | 'team_lead' | 'supervisor' | 'customer' = 'customer';
      if (roleClaim) {
        const roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];
        const lowerRoles = roles.map((r) => r.toLowerCase());
        if (lowerRoles.includes('admin')) {
          role = 'admin';
        } else if (lowerRoles.includes('team_lead')) {
          role = 'team_lead';
        } else if (lowerRoles.some(r => r === 'agent' || r.endsWith('_agent'))) {
          role = 'agent';
        } else if (lowerRoles.includes('supervisor')) {
          role = 'supervisor';
        } else if (lowerRoles.includes('customer')) {
          role = 'customer';
        }
      }
      const firstName = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'] || payload['given_name'] || '';
      const lastName = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname'] || payload['family_name'] || '';
      const fullName = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload['name'] || '';
      return {
        id: payload.sub,
        email: payload.email,
        role: role,
        firstName: firstName,
        lastName: lastName,
        fullName: fullName || `${firstName} ${lastName}`.trim(),
      };
    } catch (e) {
      return null;
    }
  }

  // --- JWT INTERCEPTOR UTILITIES ---

  // The Interceptor will call this on every outgoing HTTP request
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  // Clear everything out and send them to the login screen
  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  // Helper check for Route Guards
  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}
