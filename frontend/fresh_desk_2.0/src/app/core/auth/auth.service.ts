import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { of, delay } from 'rxjs';

export interface User {
  id: string;
  email: string;
  role: 'admin' | 'agent' | 'supervisor' | 'customer';
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  // Global Signal holding the logged-in user state
  currentUser = signal<User | null>(null);

  login(credentials: { email: string; password: string }) {
    return of({
      token: 'fake-jwt-token-ey1234567890',
      user: {
        id: 'user-123',
        email: credentials.email,
        role: 'agent', // Change this to 'customer' to test the portal routing
      } as User,
    }).pipe(delay(1500));
    
    // Note: In production, this points to your .NET Web API
    /*return this.http.post<{ token: string, user: User }>(
      `${environment.apiUrl}/api/auth/login`, 
      credentials
    );*/
  }

  handleLoginSuccess(token: string, user: User) {
    localStorage.setItem('jwt_token', token);
    this.currentUser.set(user); // Update the global signal

    // The Traffic Cop Logic
    if (user.role === 'customer') {
      this.router.navigate(['/customer-portal']);
    } else {
      this.router.navigate(['/agent-dashboard']);
    }
  }
}
