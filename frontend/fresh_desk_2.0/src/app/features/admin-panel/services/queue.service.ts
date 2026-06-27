import { Observable } from 'rxjs';

// --- Shared DTOs ---
export interface Agent { id: string; name: string; role: string; region: string; initial: string; color: string; }
export interface Queue {
  id: string; name: string; color: string; desc: string;
  members: string[]; dispatchers: string[];
  slaN: number; slaU: string; fb: string;
  usedBy: { workflow: string, stage: string }[];
}

export abstract class QueueService {
  abstract getAgents(): Observable<Record<string, Agent>>;
  abstract getQueues(): Observable<Queue[]>;
  abstract createQueue(queue: Partial<Queue>): Observable<Queue>;
  abstract updateQueue(id: string, partial: Partial<Queue>): Observable<void>;
  abstract updateMembers(queueId: string, members: string[]): Observable<void>;
}