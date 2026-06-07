import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
// import { environment } from '../../../environments/environment'; // Uncomment when using real API
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

  // The key we use to store the token in the browser
  private readonly TOKEN_KEY = 'jwt_token';

  // --- MOCK LOGIN LOGIC ---
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

  // --- JWT INTERCEPTOR UTILITIES ---

  // The Interceptor will call this on every outgoing HTTP request
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  // Clear everything out and send them to the login screen
  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  // Helper check for Route Guards
  isAuthenticated(): boolean {
    return !!this.getToken();
  }
}