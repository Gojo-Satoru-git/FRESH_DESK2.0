import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService, LoginResponse } from './auth.service';

@Injectable()
export class ApiAuthService extends AuthService {
  private http = inject(HttpClient);

  constructor() {
    super();
  }

  override loadUserMetadata(): Observable<any> {
    return this.http
      .get<{ permissions: string[]; groups: string[]; companyId: string; contactId: string; }>(`${environment.apiBaseUrl}/api/me`)
      .pipe(
        tap((res) => {
          this.permissions.set(res.permissions || []);
          this.groups.set(res.groups || []);
          this.companyId.set(res.companyId || null);
          this.contactId.set(res.contactId || null);
        })
      );
  }

  override login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiBaseUrl}/api/auth/login`, credentials);
  }

  override register(userData: {
    firstName: string;
    lastName: string;
    email: string;
    password: string;
    phone: string;
    username?: string;
  }): Observable<{ userId: string }> {
    return this.http.post<{ userId: string }>(
      `${environment.apiBaseUrl}/api/auth/register`,
      userData,
    );
  }
}
