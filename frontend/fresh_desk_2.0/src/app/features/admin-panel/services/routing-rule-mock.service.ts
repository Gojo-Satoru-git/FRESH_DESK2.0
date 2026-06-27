import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import { RoutingRuleService, RoutingConfig, Rule, Factor } from './routing-rule.service';

@Injectable()
export class MockRoutingRuleService extends RoutingRuleService {
  
  private mockFactors: Factor[] = [
    { id: 'category', name: 'Category', kind: 'hard', criteria: true, type: 'enum', master: ['Network', 'Database', 'SAP', 'Email', 'Security', 'Hardware'] },
    { id: 'priority', name: 'Priority', kind: 'attr', criteria: true, type: 'enum', master: ['P1', 'P2', 'P3', 'P4'] },
    { id: 'region', name: 'Region', kind: 'soft', criteria: true, type: 'enum', master: ['EMEA', 'AMER', 'APAC'] },
    { id: 'skill', name: 'Required skill', kind: 'hard', criteria: true, type: 'enum', master: ['Network', 'VPN', 'Firewall', 'SAP', 'Database', 'AD'] },
    { id: 'slarisk', name: 'SLA risk %', kind: 'attr', criteria: true, type: 'num' },
    { id: 'workload', name: 'Current workload', kind: 'soft', criteria: false, reason: 'an agent metric — there is no agent at routing time' },
  ];

  private mockQueues = ['Network Operations', 'Major Incident', 'EMEA Support', 'Database Team', 'General Triage'];
  
  private mockFallbackQueue = 'General Triage';
  
  private mockRules: Rule[] = [
    { when: [{ field: 'category', op: 'is one of', vals: ['Network'] }], then: 'Network Operations' },
    { when: [{ field: 'priority', op: 'is one of', vals: ['P1', 'P2'] }], then: 'Major Incident' },
    { when: [{ field: 'region', op: 'is', vals: ['EMEA'] }], then: 'EMEA Support' }
  ];

  override getConfig(): Observable<RoutingConfig> {
    return of({
      factors: this.mockFactors,
      availableQueues: this.mockQueues,
      fallbackQueue: this.mockFallbackQueue,
      rules: this.mockRules
    }).pipe(delay(400));
  }

  override saveConfig(rules: Rule[], fallbackQueue: string): Observable<void> {
    this.mockRules = [...rules];
    this.mockFallbackQueue = fallbackQueue;
    return of(void 0).pipe(delay(600)); 
  }
}