import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { QueueService, Queue, Agent } from './queue.service';

@Injectable()
export class ApiQueueService extends QueueService {
  private http = inject(HttpClient);

  override getAgents(): Observable<Record<string, Agent>> {
    return this.http.get<Record<string, Agent>>(`${environment.apiBaseUrl}/api/admin/agents`);
  }

  override getQueues(): Observable<Queue[]> {
    return this.http.get<Queue[]>(`${environment.apiBaseUrl}/api/admin/queues`);
  }

  override createQueue(queue: Partial<Queue>): Observable<Queue> {
    return this.http.post<Queue>(`${environment.apiBaseUrl}/api/admin/queues`, queue);
  }

  override updateQueue(id: string, partial: Partial<Queue>): Observable<void> {
    return this.http.put<void>(`${environment.apiBaseUrl}/api/admin/queues/${id}`, partial);
  }

  override updateMembers(queueId: string, members: string[]): Observable<void> {
    return this.http.put<void>(`${environment.apiBaseUrl}/api/admin/queues/${queueId}/members`, { members });
  }
}