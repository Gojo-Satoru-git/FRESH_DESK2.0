import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../../environments/environment.development';

export interface NotificationLog {
  id: string;
  ticketId: string;
  ticketNumber?: string;
  recipientEmail: string;
  errorMessage: string | null;
  isFailedDelivery: boolean;
  sentAt: string;
  templateId: string;
  isRead?: boolean; // Optional: depending on your entity configuration tracking properties
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/notifications`;

  /**
   * GET /api/notifications/unread
   * Fetches all unread logs (including SLA breach alerts) out of the database repository pipeline
   */
  getUnreadNotifications(): Observable<NotificationLog[]> {
    return this.http.get<NotificationLog[]>(`${this.apiUrl}/unread`).pipe(
      catchError(() => {
        // Return an empty array if the notification center is not implemented or errors out
        return of([]);
      })
    );
  }

  /**
   * POST /api/notifications/{id}/read
   * Marks a single notification item as read to clear badge notifications or counters
   */
  markAsRead(id: string): Observable<{ success: boolean; message: string }> {
    return this.http.post<{ success: boolean; message: string }>(`${this.apiUrl}/${id}/read`, {});
  }

  /**
   * POST /api/notifications/read-all
   * Clean sweep clear for the agent's notification panel view console interface
   */
  markAllAsRead(): Observable<{ count: number; message: string }> {
    return this.http.post<{ count: number; message: string }>(`${this.apiUrl}/read-all`, {});
  }
}
