import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideAppInitializer,
  inject,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { jwtInterceptor } from './core/auth/jwt.interceptor';
import { ApiAuthService } from './core/auth/auth-api.service';
import { AuthService } from './core/auth/auth.service';
import { MockAuthService } from './core/auth/auth-mock.service';
import { environment } from '../environments/environment';
import { ApiTicketService } from './features/tickets/services/ticket-api.service';
import { MockTicketService } from './features/tickets/services/ticket-mock.service';
import { TicketService } from './features/tickets/services/ticket.service';
import { KnowledgeBaseService } from './features/knowledgebase/components/services/knowledge-base.service';
import { MockKnowledgeBaseService } from './features/knowledgebase/components/services/knowledge-base-mock.service';
import { ApiKnowledgeBaseService } from './features/knowledgebase/components/services/knowledge-base-api.service';
import { MockQueueService } from './features/admin-panel/services/queue-mock.service';
import { QueueService } from './features/admin-panel/services/queue.service';
import { ApiQueueService } from './features/admin-panel/services/queue-api.service';
import { DashboardService } from './features/dashboard/services/dashboard.service';
import { MockDashboardService } from './features/dashboard/services/dashboard-mock.service';
import { ApiDashboardService } from './features/dashboard/services/dashboard-api.service';
import { RoutingRuleService } from './features/admin-panel/services/routing-rule.service';
import { MockRoutingRuleService } from './features/admin-panel/services/routing-rule-mock.service';
import { ApiRoutingRuleService } from './features/admin-panel/services/routing-rule-api.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor])),
    // --- THE NEW ANGULAR 19 BOOTSTRAPPER ---
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      return authService.initializeSession();
    }),
    {
      provide: AuthService,
      useClass: environment.useMockData ? MockAuthService : ApiAuthService,
    },
    {
      provide: TicketService,
      useClass: environment.useMockData ? MockTicketService : ApiTicketService,
    },
    {
      provide: KnowledgeBaseService,
      useClass: environment.useMockData ? MockKnowledgeBaseService : ApiKnowledgeBaseService,
    },
    {
      provide: QueueService,
      useClass: environment.useMockData ? MockQueueService : ApiQueueService,
    },
    {
      provide: DashboardService,
      useClass: environment.useMockData ? MockDashboardService : ApiDashboardService,
    },
    {
      provide: RoutingRuleService,
      useClass: environment.useMockData ? MockRoutingRuleService : ApiRoutingRuleService,
    },
  ],
};
