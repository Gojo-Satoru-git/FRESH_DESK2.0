import { Observable } from 'rxjs';

export interface Factor {
  id: string;
  name: string;
  kind: 'hard' | 'soft' | 'attr';
  criteria: boolean;
  type?: 'enum' | 'num';
  master?: string[];
  hint?: string;
  reason?: string;
}

export interface Condition {
  field: string;
  op: string;
  vals: any[];
}

export interface Rule {
  when: Condition[];
  then: string;
}

export interface RoutingConfig {
  factors: Factor[];
  availableQueues: string[];
  fallbackQueue: string;
  rules: Rule[];
}

export abstract class RoutingRuleService {
  abstract getConfig(): Observable<RoutingConfig>;
  abstract saveConfig(rules: Rule[], fallbackQueue: string): Observable<void>;
}