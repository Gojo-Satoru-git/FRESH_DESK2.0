import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { RoutingRuleService, RoutingConfig, Rule } from './routing-rule.service';

@Injectable()
export class ApiRoutingRuleService extends RoutingRuleService {
  private http = inject(HttpClient);

  override getConfig(): Observable<RoutingConfig> {
    return this.http.get<RoutingConfig>(`${environment.apiBaseUrl}/api/admin/routing-rules`);
  }

  override saveConfig(rules: Rule[], fallbackQueue: string): Observable<void> {
    return this.http.put<void>(`${environment.apiBaseUrl}/api/admin/routing-rules`, { rules, fallbackQueue });
  }
}