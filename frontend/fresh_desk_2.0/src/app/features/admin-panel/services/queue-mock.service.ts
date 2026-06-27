import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { QueueService, Queue, Agent } from './queue.service';

@Injectable()
export class MockQueueService extends QueueService {
  
  private mockAgents: Record<string, Agent> = {
    'ak': { id: 'ak', name: 'Asha Admin', role: 'L2', region: 'EMEA', initial: 'AA', color: '#2066D1' },
    'ds': { id: 'ds', name: 'Diego Santos', role: 'L2', region: 'AMER', initial: 'DS', color: '#509675' },
    'ml': { id: 'ml', name: 'Mei Lin', role: 'L2', region: 'APAC', initial: 'ML', color: '#F57536' },
  };

  private mockQueues: Queue[] = [
    {
      id: 'netops', name: 'Network Operations', color: '#2066D1', 
      desc: 'First-line network incidents and requests. Leads triage and dispatch to the on-shift network engineers.',
      members: ['ak', 'ds', 'ml'], dispatchers: ['Lead'], slaN: 15, slaU: 'min', fb: 'least-load',
      usedBy: [{ workflow: 'Incident Management', stage: 'New' }]
    },
    {
      id: 'major', name: 'Major Incident', color: '#E86161',
      desc: 'P1/P2 majors. Watched by Leads and the on-call Incident Manager for immediate dispatch.',
      members: ['ds'], dispatchers: ['Lead', 'Incident Manager'], slaN: 5, slaU: 'min', fb: 'escalate to manager',
      usedBy: [{ workflow: 'Security Response', stage: 'Triage' }]
    }
  ];

  override getAgents(): Observable<Record<string, Agent>> {
    return of(this.mockAgents).pipe(delay(300));
  }

  override getQueues(): Observable<Queue[]> {
    return of(this.mockQueues).pipe(delay(300));
  }

  override createQueue(queue: Partial<Queue>): Observable<Queue> {
    const newQueue = { ...queue, id: 'q' + Date.now() } as Queue;
    this.mockQueues.push(newQueue);
    return of(newQueue).pipe(delay(400));
  }

  override updateQueue(id: string, partial: Partial<Queue>): Observable<void> {
    const index = this.mockQueues.findIndex(q => q.id === id);
    if (index > -1) this.mockQueues[index] = { ...this.mockQueues[index], ...partial };
    return of(void 0).pipe(delay(400));
  }

  override updateMembers(queueId: string, members: string[]): Observable<void> {
    const index = this.mockQueues.findIndex(q => q.id === queueId);
    if (index > -1) this.mockQueues[index].members = members;
    return of(void 0).pipe(delay(400));
  }
}