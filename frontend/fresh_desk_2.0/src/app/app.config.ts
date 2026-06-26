import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
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

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([jwtInterceptor])),
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
    }

  ],
};
