import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs'; // ◄ Added map
import { environment } from '../../../../environments/environment';

export interface NotificationLog {
  id: string;
  ticketId: string;
  ticketNumber?: string;
  recipientEmail: string;
  errorMessage: string | null;
  message?: string; // ◄ Added this optional helper property for your toast components
  isFailedDelivery: boolean;
  sentAt: string;
  templateId: string;
  isRead?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/api/notifications`;

  /**
   * GET /api/notifications/unread
   * Fetches all unread logs and normalizes message formatting
   */
  getUnreadNotifications(): Observable<NotificationLog[]> {
    return this.http.get<NotificationLog[]>(`${this.apiUrl}/unread`).pipe(
      map((logs) =>
        logs.map((log) => ({
          ...log,
          // ✅ Dynamically map the data string so your toaster can read '.message' safely
          message: log.message || 'System notification received.',
        }))
      ),
      catchError(() => {
        return of([]);
      })
    );
  }

  /**
   * POST /api/notifications/{id}/read
   */
  markAsRead(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/${id}/read`, {});
  }

  /**
   * POST /api/notifications/read-all
   */
  markAllAsRead(): Observable<{ count: number; message: string }> {
    return this.http.post<{ count: number; message: string }>(`${this.apiUrl}/read-all`, {});
  }
}
