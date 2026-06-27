import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { DashboardService, DashboardData } from './dashboard.service';

@Injectable()
export class ApiDashboardService extends DashboardService {
  private http = inject(HttpClient);

  override getDashboard(): Observable<DashboardData> {
    return this.http.get<DashboardData>(`${environment.apiBaseUrl}/api/tickets/dashboard`);
  }
}